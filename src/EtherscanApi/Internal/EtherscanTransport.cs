using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace EtherscanApi.Internal;

/// <summary>
/// The single channel every module uses: throttle → HTTP → envelope parsing → error classification → retry.
/// </summary>
/// <remarks>
/// Responses are read once into a byte buffer. The envelope is scanned with <see cref="Utf8JsonReader"/>,
/// and <c>result</c> is deserialized directly from its byte range (no intermediate DOM).
/// </remarks>
internal sealed partial class EtherscanTransport : IDisposable
{
    private const int MaxErrorBodyPreview = 200;

    private static readonly MediaTypeWithQualityHeaderValue JsonMediaType = new("application/json");

    private readonly HttpClient _httpClient;
    private readonly EtherscanClientOptions _options;
    private readonly EtherscanRateLimiter? _rateLimiter;
    private readonly ILogger _logger;
    private readonly TimeProvider _timeProvider;
    private readonly bool _ownsHttpClient;
    private readonly bool _ownsRateLimiter;

    public EtherscanTransport(
        HttpClient httpClient,
        EtherscanClientOptions options,
        EtherscanRateLimiter? rateLimiter,
        ILogger? logger,
        TimeProvider? timeProvider,
        bool ownsHttpClient,
        bool ownsRateLimiter)
    {
        _httpClient = httpClient;
        _options = options;
        _rateLimiter = rateLimiter;
        _logger = logger ?? NullLogger.Instance;
        _timeProvider = timeProvider ?? TimeProvider.System;
        _ownsHttpClient = ownsHttpClient;
        _ownsRateLimiter = ownsRateLimiter;
    }

    private delegate T BodyParser<out T>(byte[] body, EtherscanRequest request);

    public EtherscanClientOptions Options => _options;

    public TimeProvider TimeProvider => _timeProvider;

    public ChainId Resolve(ChainId? chain) => chain ?? _options.DefaultChain;

    /// <summary>Sends a request whose envelope is <c>{status, message, result}</c> and returns <c>result</c>.</summary>
    public Task<T> SendAsync<T>(EtherscanRequest request, JsonTypeInfo<T> resultType, CancellationToken cancellationToken)
        => SendCoreAsync(request, (body, req) => ParseStandard(body, req, resultType, emptyResult: null), cancellationToken);

    /// <summary>
    /// Sends a request that returns a list. A "no records found" envelope yields an empty list instead of an error.
    /// </summary>
    public async Task<IReadOnlyList<T>> SendListAsync<T>(
        EtherscanRequest request,
        JsonTypeInfo<List<T>> resultType,
        CancellationToken cancellationToken)
    {
        var list = await SendCoreAsync(
                request,
                (body, req) => ParseStandard(body, req, resultType, emptyResult: static () => []),
                cancellationToken)
            .ConfigureAwait(false);
        return list ?? [];
    }

    /// <summary>
    /// Sends a request and returns the envelope text without throwing on <c>status = "0"</c>, except for
    /// authentication, rate-limit, quota, timeout, chain and plan errors. Used by status-polling endpoints such as
    /// <c>checkverifystatus</c>, where "Pending in queue" is a normal answer.
    /// </summary>
    public Task<EnvelopeText> SendEnvelopeAsync(EtherscanRequest request, CancellationToken cancellationToken)
        => SendCoreAsync(request, ParseEnvelopeText, cancellationToken);

    /// <summary>
    /// Sends a Geth/Parity proxy request whose <c>result</c> may legitimately be <c>null</c> (unknown block,
    /// transaction or receipt). Accepts the JSON-RPC shape (<c>{jsonrpc, id, result|error}</c>) and, for throttling
    /// and other API errors, the standard envelope.
    /// </summary>
    public Task<T?> SendRpcAsync<T>(EtherscanRequest request, JsonTypeInfo<T> resultType, CancellationToken cancellationToken)
        => SendCoreAsync(request, (body, req) => ParseRpc(body, req, resultType, required: false), cancellationToken);

    /// <summary>Sends a Geth/Parity proxy request whose <c>result</c> must be present.</summary>
    public async Task<T> SendRpcRequiredAsync<T>(EtherscanRequest request, JsonTypeInfo<T> resultType, CancellationToken cancellationToken)
        => (await SendCoreAsync(request, (body, req) => ParseRpc(body, req, resultType, required: true), cancellationToken)
            .ConfigureAwait(false))!;

    /// <summary>
    /// Sends a request whose successful response is a file (for example CSV) rather than JSON.
    /// A JSON body is treated as an error envelope. Not retried.
    /// </summary>
    public async Task<Stream> SendForStreamAsync(EtherscanRequest request, CancellationToken cancellationToken)
    {
        if (_rateLimiter is not null)
        {
            await _rateLimiter.AcquireAsync(request, cancellationToken).ConfigureAwait(false);
        }

        LogSending(_logger, request.Module, request.Action, request.Chain?.Value, 1);
        using var message = BuildHttpRequest(request);
        var response = await SendRawAsync(message, HttpCompletionOption.ResponseHeadersRead, request, cancellationToken)
            .ConfigureAwait(false);
        try
        {
            if (response.Content.Headers.ContentType?.MediaType?.Contains("json", StringComparison.OrdinalIgnoreCase) == true)
            {
                var body = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
                var envelope = ParseEnvelopeText(body, request);
                throw ErrorClassifier.Create(envelope.Message, envelope.Result, request);
            }

            return await OwnStreamAsync(response, cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            response.Dispose();
            throw;
        }
    }

    /// <summary>Opens a download stream for a temporary export URL returned by the metadata endpoints.</summary>
    /// <remarks>The client timeout covers the response headers; pass a cancellation token to bound the download itself.</remarks>
    public async Task<Stream> OpenDownloadAsync(Uri url, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(url);
        if (!url.IsAbsoluteUri || url.Scheme != Uri.UriSchemeHttps)
        {
            throw new ArgumentException("Export URLs must be absolute HTTPS URLs.", nameof(url));
        }

        using var message = new HttpRequestMessage(HttpMethod.Get, url);
        var response = await SendRawAsync(message, HttpCompletionOption.ResponseHeadersRead, request: null, cancellationToken)
            .ConfigureAwait(false);
        try
        {
            return await OwnStreamAsync(response, cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            response.Dispose();
            throw;
        }
    }

    private static async Task<Stream> OwnStreamAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        return new ResponseOwningStream(stream, response);
    }

    private async Task<T> SendCoreAsync<T>(
        EtherscanRequest request,
        BodyParser<T> parse,
        CancellationToken cancellationToken)
    {
        var maxAttempts = request.IsIdempotent ? _options.MaxRetryAttempts + 1 : 1;

        for (var attempt = 1; ; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var canRetry = attempt < maxAttempts;

            if (_rateLimiter is not null)
            {
                await _rateLimiter.AcquireAsync(request, cancellationToken).ConfigureAwait(false);
            }

            LogSending(_logger, request.Module, request.Action, request.Chain?.Value, attempt);

            byte[] body;
            try
            {
                body = await SendHttpAsync(request, cancellationToken).ConfigureAwait(false);
            }
            catch (EtherscanHttpException ex) when (canRetry && IsTransient(ex))
            {
                LogRetrying(_logger, request.Module, request.Action, attempt, ex.GetType().Name);
                await DelayAsync(attempt, cancellationToken).ConfigureAwait(false);
                continue;
            }

            try
            {
                return parse(body, request);
            }
            catch (EtherscanApiException ex) when (canRetry && ex is EtherscanRateLimitException or EtherscanQueryTimeoutException)
            {
                LogRetrying(_logger, request.Module, request.Action, attempt, ex.GetType().Name);
                await DelayAsync(attempt, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Buffers the whole response inside <see cref="HttpClient.SendAsync(HttpRequestMessage, HttpCompletionOption, CancellationToken)"/>,
    /// so <see cref="HttpClient.Timeout"/> also covers reading the body.
    /// </summary>
    private async Task<byte[]> SendHttpAsync(EtherscanRequest request, CancellationToken cancellationToken)
    {
        using var message = BuildHttpRequest(request);
        using var response = await SendRawAsync(message, HttpCompletionOption.ResponseContentRead, request, cancellationToken)
            .ConfigureAwait(false);
        return await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// The one place that sends HTTP: maps network failures, client timeouts and non-success status codes to
    /// <see cref="EtherscanHttpException"/>. The caller owns the returned response.
    /// </summary>
    private async Task<HttpResponseMessage> SendRawAsync(
        HttpRequestMessage message,
        HttpCompletionOption completionOption,
        EtherscanRequest? request,
        CancellationToken cancellationToken)
    {
        var operation = request is null ? "Etherscan export download" : $"Etherscan {request.Module}/{request.Action}";
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(message, completionOption, cancellationToken).ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            // HttpRequestException messages do not include the query string, but build our own message anyway.
            throw Decorate(new EtherscanHttpException($"{operation} request failed: {ex.HttpRequestError}.", ex) { StatusCode = ex.StatusCode }, request);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw Decorate(new EtherscanHttpException($"{operation} timed out.", ex), request);
        }

        if (!response.IsSuccessStatusCode)
        {
            var exception = new EtherscanHttpException(
                $"{operation} returned HTTP {(int)response.StatusCode} ({response.ReasonPhrase}).")
            {
                StatusCode = response.StatusCode,
            };
            response.Dispose();
            throw Decorate(exception, request);
        }

        return response;
    }

    private static EtherscanHttpException Decorate(EtherscanHttpException exception, EtherscanRequest? request)
        => request is null ? exception : ErrorClassifier.Decorate(exception, request);

    private HttpRequestMessage BuildHttpRequest(EtherscanRequest request)
    {
        var baseAddress = request.Host == EtherscanHost.Metadata ? _options.MetadataBaseAddress : _options.BaseAddress;
        HttpRequestMessage message = request.IsPost
            ? new(HttpMethod.Post, new Uri(baseAddress, request.BuildRelativeUri(apiKey: null)))
            {
                Content = request.BuildFormContent(_options.ApiKey),
            }
            : new(HttpMethod.Get, new Uri(baseAddress, request.BuildRelativeUri(_options.ApiKey)));
        message.Headers.Accept.Add(JsonMediaType);
        return message;
    }

    private static bool IsTransient(EtherscanHttpException ex)
        => ex.StatusCode is null // network failure or timeout
           || ex.StatusCode == HttpStatusCode.TooManyRequests
           || ex.StatusCode == HttpStatusCode.RequestTimeout
           || (int)ex.StatusCode.Value >= 500;

    private Task DelayAsync(int attempt, CancellationToken cancellationToken)
    {
        var baseDelay = _options.RetryBaseDelay;
        if (baseDelay <= TimeSpan.Zero)
        {
            return Task.CompletedTask;
        }

        var exponential = baseDelay.TotalMilliseconds * Math.Pow(2, attempt - 1);
#pragma warning disable CA5394 // Jitter does not need a cryptographic random source.
        var jitter = Random.Shared.NextDouble() * baseDelay.TotalMilliseconds;
#pragma warning restore CA5394
        var delay = TimeSpan.FromMilliseconds(Math.Min(exponential + jitter, 30_000));
        return Task.Delay(delay, _timeProvider, cancellationToken);
    }

    private ScannedEnvelope Scan(byte[] body, EtherscanRequest request)
    {
        try
        {
            return EnvelopeScanner.Scan(body);
        }
        catch (JsonException ex)
        {
            // The body is server-controlled text; mask the key in case it is echoed back.
            var preview = System.Text.Encoding.UTF8.GetString(body, 0, Math.Min(body.Length, MaxErrorBodyPreview));
            if (!string.IsNullOrEmpty(_options.ApiKey))
            {
                preview = preview.Replace(_options.ApiKey, "***", StringComparison.Ordinal);
            }

            throw ErrorClassifier.Decorate(
                new EtherscanResponseException(
                    $"Etherscan {request.Module}/{request.Action} returned a non-JSON response: {preview}",
                    ex),
                request);
        }
    }

    private T ParseStandard<T>(byte[] body, EtherscanRequest request, JsonTypeInfo<T> resultType, Func<T>? emptyResult)
    {
        var envelope = Scan(body, request);
        ThrowIfRpcError(envelope, request);

        if (envelope.Status == "1" || (envelope.Status is null && envelope.HasResult))
        {
            if (envelope.ResultIsNull)
            {
                return emptyResult is not null ? emptyResult() : throw MissingResult(request);
            }

            return Deserialize(envelope.Result, request, resultType);
        }

        if (emptyResult is not null
            && (envelope.ResultIsEmptyArray || ErrorClassifier.IsNoRecords(envelope.Message, envelope.ResultString)))
        {
            return emptyResult();
        }

        throw ErrorClassifier.Create(envelope.Message, envelope.ResultText, request);
    }

    private T? ParseRpc<T>(byte[] body, EtherscanRequest request, JsonTypeInfo<T> resultType, bool required)
    {
        var envelope = Scan(body, request);
        ThrowIfRpcError(envelope, request);

        // Throttling and API-level errors on proxy endpoints arrive in the standard envelope.
        if (!envelope.IsJsonRpc && envelope.Status == "0")
        {
            throw ErrorClassifier.Create(envelope.Message, envelope.ResultString, request);
        }

        if (envelope.ResultIsNull)
        {
            return required ? throw MissingResult(request) : default;
        }

        return Deserialize(envelope.Result, request, resultType);
    }

    private EnvelopeText ParseEnvelopeText(byte[] body, EtherscanRequest request)
    {
        var envelope = Scan(body, request);
        var resultText = envelope.ResultText;

        if (envelope.Status != "1" && ErrorClassifier.IsFatal(ErrorClassifier.Classify(resultText + " " + envelope.Message)))
        {
            throw ErrorClassifier.Create(envelope.Message, resultText, request);
        }

        return new EnvelopeText(envelope.Status == "1", envelope.Message, resultText);
    }

    private static EtherscanResponseException MissingResult(EtherscanRequest request)
        => ErrorClassifier.Decorate(
            new EtherscanResponseException($"Etherscan {request.Module}/{request.Action} returned no result."),
            request);

    private static void ThrowIfRpcError(in ScannedEnvelope envelope, EtherscanRequest request)
    {
        if (envelope.HasError)
        {
            throw ErrorClassifier.Decorate(
                new EtherscanRpcException(
                    $"Etherscan {request.Module}/{request.Action} returned JSON-RPC error {envelope.ErrorCode}: {envelope.ErrorMessage}")
                {
                    RpcCode = envelope.ErrorCode,
                    RpcMessage = envelope.ErrorMessage,
                },
                request);
        }
    }

    private static T Deserialize<T>(ReadOnlySpan<byte> json, EtherscanRequest request, JsonTypeInfo<T> typeInfo)
    {
        try
        {
            return JsonSerializer.Deserialize(json, typeInfo) ?? throw MissingResult(request);
        }
        catch (JsonException ex)
        {
            throw ErrorClassifier.Decorate(
                new EtherscanResponseException(
                    $"Etherscan {request.Module}/{request.Action} returned a result that could not be read as {typeof(T).Name}: {ex.Message}",
                    ex),
                request);
        }
    }

    public void Dispose()
    {
        if (_ownsHttpClient)
        {
            _httpClient.Dispose();
        }

        if (_ownsRateLimiter)
        {
            _rateLimiter?.Dispose();
        }
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "Sending Etherscan {Module}/{Action} (chain {ChainId}), attempt {Attempt}")]
    private static partial void LogSending(ILogger logger, string module, string action, ulong? chainId, int attempt);

    [LoggerMessage(EventId = 2, Level = LogLevel.Warning, Message = "Etherscan {Module}/{Action} attempt {Attempt} failed with {ErrorType}; retrying")]
    private static partial void LogRetrying(ILogger logger, string module, string action, int attempt, string errorType);

    private sealed class ResponseOwningStream(Stream inner, HttpResponseMessage response) : Stream
    {
        public override bool CanRead => inner.CanRead;
        public override bool CanSeek => inner.CanSeek;
        public override bool CanWrite => false;
        public override long Length => inner.Length;

        public override long Position
        {
            get => inner.Position;
            set => inner.Position = value;
        }

        public override void Flush() => inner.Flush();

        public override int Read(byte[] buffer, int offset, int count) => inner.Read(buffer, offset, count);

        public override int Read(Span<byte> buffer) => inner.Read(buffer);

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => inner.ReadAsync(buffer, offset, count, cancellationToken);

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            => inner.ReadAsync(buffer, cancellationToken);

        public override long Seek(long offset, SeekOrigin origin) => inner.Seek(offset, origin);

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                inner.Dispose();
                response.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}

/// <summary>The text of an Etherscan envelope.</summary>
internal readonly record struct EnvelopeText(bool IsOk, string? Message, string? Result);
