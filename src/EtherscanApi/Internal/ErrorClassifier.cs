using System.Globalization;
using System.Text.RegularExpressions;

namespace EtherscanApi.Internal;

/// <summary>
/// Maps Etherscan error envelopes to typed exceptions. Etherscan reports errors with HTTP 200, so this
/// classification (not HTTP status codes) drives retries. See
/// <see href="https://docs.etherscan.io/common-error-messages">Common Error Messages</see>.
/// </summary>
internal static partial class ErrorClassifier
{
    [GeneratedRegex(@"Resets\s+(\d{4}-\d{2}-\d{2}\s+\d{2}:\d{2}:\d{2})\s*UTC", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex ResetRegex();

    /// <summary>
    /// Whether a <c>status = "0"</c> envelope means "valid request, no records" rather than an error,
    /// for example <c>{"status":"0","message":"No transactions found","result":[]}</c>.
    /// </summary>
    public static bool IsNoRecords(string? message, string? resultText)
        => LooksLikeNoRecords(message) || LooksLikeNoRecords(resultText);

    private static bool LooksLikeNoRecords(string? text)
        => text is not null
           && text.StartsWith("No ", StringComparison.OrdinalIgnoreCase)
           && text.Contains("found", StringComparison.OrdinalIgnoreCase);

    public static EtherscanApiException Create(string? message, string? result, EtherscanRequest request)
    {
        var detail = string.IsNullOrWhiteSpace(result) ? message ?? "Unknown error" : result;
        var text = detail + " " + message;
        var description = $"Etherscan {request.Module}/{request.Action} failed: {detail}";

        EtherscanApiException exception = Classify(text) switch
        {
            ErrorKind.Authentication => new EtherscanAuthenticationException(description),
            ErrorKind.Quota => new EtherscanQuotaExceededException(description) { ResetsAt = TryParseReset(text) },
            ErrorKind.RateLimit => new EtherscanRateLimitException(description),
            ErrorKind.Timeout => new EtherscanQueryTimeoutException(description),
            ErrorKind.UnsupportedChain => new EtherscanUnsupportedChainException(description),
            ErrorKind.PlanRequired => new EtherscanPlanRequiredException(description),
            _ => new EtherscanApiException(description),
        };

        exception.ApiMessage = message;
        exception.ApiResult = result;
        return Decorate(exception, request);
    }

    /// <summary>
    /// Errors that end a request even on endpoints where <c>status = "0"</c> is otherwise a normal answer
    /// (such as "Pending in queue" from verification status checks).
    /// </summary>
    public static bool IsFatal(ErrorKind kind) => kind != ErrorKind.Other;

    public static T Decorate<T>(T exception, EtherscanRequest request)
        where T : EtherscanException
    {
        exception.Module = request.Module;
        exception.Action = request.Action;
        exception.Chain = request.Chain;
        exception.RequestUri = request.BuildRedactedUri();
        return exception;
    }

    internal static ErrorKind Classify(string text)
    {
        // Note: "Max rate limit reached, please use API Key for higher rate limit" mentions "API Key",
        // so match the specific invalid/missing phrases only.
        if (Contains(text, "Invalid API Key") || Contains(text, "Missing API Key"))
        {
            return ErrorKind.Authentication;
        }

        if (Contains(text, "Community Free API limit") || Contains(text, "daily limit") || Contains(text, "spend cap"))
        {
            return ErrorKind.Quota;
        }

        if (Contains(text, "rate limit"))
        {
            return ErrorKind.RateLimit;
        }

        if (Contains(text, "timeout") || Contains(text, "server too busy"))
        {
            return ErrorKind.Timeout;
        }

        if (Contains(text, "chainid") || Contains(text, "unsupported chain"))
        {
            return ErrorKind.UnsupportedChain;
        }

        if (Contains(text, "Free API access is not supported")
            || Contains(text, "upgrade your api plan")
            || Contains(text, "API Pro")
            || Contains(text, "Missing Or invalid Action name"))
        {
            return ErrorKind.PlanRequired;
        }

        return ErrorKind.Other;
    }

    private static bool Contains(string text, string value) => text.Contains(value, StringComparison.OrdinalIgnoreCase);

    private static DateTimeOffset? TryParseReset(string text)
    {
        var match = ResetRegex().Match(text);
        return match.Success
               && DateTimeOffset.TryParseExact(
                   match.Groups[1].Value,
                   "yyyy-MM-dd HH:mm:ss",
                   CultureInfo.InvariantCulture,
                   DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                   out var reset)
            ? reset
            : null;
    }

    internal enum ErrorKind
    {
        Other,
        Authentication,
        Quota,
        RateLimit,
        Timeout,
        UnsupportedChain,
        PlanRequired,
    }
}
