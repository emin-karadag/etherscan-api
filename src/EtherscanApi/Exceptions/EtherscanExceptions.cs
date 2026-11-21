using System.Net;

namespace EtherscanApi;

/// <summary>
/// Base type of every exception thrown by the Etherscan client.
/// </summary>
/// <remarks>
/// Messages never contain the API key. <see cref="RequestUri"/> holds the request URI with the key masked.
/// </remarks>
public class EtherscanException : Exception
{
    /// <summary>Creates an exception.</summary>
    public EtherscanException()
    {
    }

    /// <summary>Creates an exception with a message.</summary>
    public EtherscanException(string message)
        : base(message)
    {
    }

    /// <summary>Creates an exception with a message and an inner exception.</summary>
    public EtherscanException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }

    /// <summary>The Etherscan <c>module</c> of the failed request, when known.</summary>
    public string? Module { get; internal set; }

    /// <summary>The Etherscan <c>action</c> of the failed request, when known.</summary>
    public string? Action { get; internal set; }

    /// <summary>The chain of the failed request, when it had one.</summary>
    public ChainId? Chain { get; internal set; }

    /// <summary>The request URI with the API key masked (<c>apikey=***</c>).</summary>
    public string? RequestUri { get; internal set; }
}

/// <summary>
/// Etherscan returned an error envelope (<c>status = "0"</c>). Etherscan reports these errors with HTTP 200.
/// </summary>
public class EtherscanApiException : EtherscanException
{
    /// <summary>Creates an exception.</summary>
    public EtherscanApiException()
    {
    }

    /// <summary>Creates an exception with a message.</summary>
    public EtherscanApiException(string message)
        : base(message)
    {
    }

    /// <summary>Creates an exception with a message and an inner exception.</summary>
    public EtherscanApiException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }

    /// <summary>The envelope's <c>message</c> field, such as <c>NOTOK</c>.</summary>
    public string? ApiMessage { get; internal set; }

    /// <summary>The envelope's <c>result</c> field, which carries the error description.</summary>
    public string? ApiResult { get; internal set; }
}

/// <summary>
/// A rate limit was hit. Either Etherscan rejected the request (<c>Max rate limit reached</c>), which is
/// retried automatically for idempotent requests before this is thrown, or the client-side rate limiter queue
/// was full (<see cref="IsClientSide"/>), which is never retried.
/// </summary>
public sealed class EtherscanRateLimitException : EtherscanApiException
{
    /// <summary>
    /// <see langword="true"/> when the client-side queue was full (too many concurrent requests waiting for a
    /// permit) rather than Etherscan rejecting the request. Reduce concurrency or raise
    /// <see cref="EtherscanClientOptions.MaxQueuedRequests"/>.
    /// </summary>
    public bool IsClientSide { get; internal set; }

    /// <summary>Creates an exception.</summary>
    public EtherscanRateLimitException()
    {
    }

    /// <summary>Creates an exception with a message.</summary>
    public EtherscanRateLimitException(string message)
        : base(message)
    {
    }

    /// <summary>Creates an exception with a message and an inner exception.</summary>
    public EtherscanRateLimitException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// The query timed out or the server was busy (<c>Query Timeout occured</c>,
/// <c>server too busy</c>). Narrow the block or date range. Retried automatically for idempotent requests.
/// </summary>
public sealed class EtherscanQueryTimeoutException : EtherscanApiException
{
    /// <summary>Creates an exception.</summary>
    public EtherscanQueryTimeoutException()
    {
    }

    /// <summary>Creates an exception with a message.</summary>
    public EtherscanQueryTimeoutException(string message)
        : base(message)
    {
    }

    /// <summary>Creates an exception with a message and an inner exception.</summary>
    public EtherscanQueryTimeoutException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// The API key is missing, invalid, or the IP was throttled after too many invalid attempts.
/// Never retried: more than 5 invalid attempts in 30 seconds throttle your IP.
/// </summary>
public sealed class EtherscanAuthenticationException : EtherscanApiException
{
    /// <summary>Creates an exception.</summary>
    public EtherscanAuthenticationException()
    {
    }

    /// <summary>Creates an exception with a message.</summary>
    public EtherscanAuthenticationException(string message)
        : base(message)
    {
    }

    /// <summary>Creates an exception with a message and an inner exception.</summary>
    public EtherscanAuthenticationException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>The <c>chainid</c> is missing or not supported by Etherscan.</summary>
public sealed class EtherscanUnsupportedChainException : EtherscanApiException
{
    /// <summary>Creates an exception.</summary>
    public EtherscanUnsupportedChainException()
    {
    }

    /// <summary>Creates an exception with a message.</summary>
    public EtherscanUnsupportedChainException(string message)
        : base(message)
    {
    }

    /// <summary>Creates an exception with a message and an inner exception.</summary>
    public EtherscanUnsupportedChainException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// The endpoint or chain needs a higher API plan (for example a PRO endpoint on the Free tier,
/// a paid-only chain, or a PRO endpoint on a testnet).
/// </summary>
public sealed class EtherscanPlanRequiredException : EtherscanApiException
{
    /// <summary>Creates an exception.</summary>
    public EtherscanPlanRequiredException()
    {
    }

    /// <summary>Creates an exception with a message.</summary>
    public EtherscanPlanRequiredException(string message)
        : base(message)
    {
    }

    /// <summary>Creates an exception with a message and an inner exception.</summary>
    public EtherscanPlanRequiredException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// A daily or shared quota was used up (for example <c>Community Free API limit reached</c>).
/// Not retried automatically.
/// </summary>
public sealed class EtherscanQuotaExceededException : EtherscanApiException
{
    /// <summary>Creates an exception.</summary>
    public EtherscanQuotaExceededException()
    {
    }

    /// <summary>Creates an exception with a message.</summary>
    public EtherscanQuotaExceededException(string message)
        : base(message)
    {
    }

    /// <summary>Creates an exception with a message and an inner exception.</summary>
    public EtherscanQuotaExceededException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }

    /// <summary>When the quota resets, if Etherscan reported it.</summary>
    public DateTimeOffset? ResetsAt { get; internal set; }
}

/// <summary>The HTTP call failed: a network error, a timeout, or a non-success status code.</summary>
public sealed class EtherscanHttpException : EtherscanException
{
    /// <summary>Creates an exception.</summary>
    public EtherscanHttpException()
    {
    }

    /// <summary>Creates an exception with a message.</summary>
    public EtherscanHttpException(string message)
        : base(message)
    {
    }

    /// <summary>Creates an exception with a message and an inner exception.</summary>
    public EtherscanHttpException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }

    /// <summary>The HTTP status code, when a response was received.</summary>
    public HttpStatusCode? StatusCode { get; internal set; }
}

/// <summary>A Geth/Parity proxy call returned a JSON-RPC error object.</summary>
public sealed class EtherscanRpcException : EtherscanException
{
    /// <summary>Creates an exception.</summary>
    public EtherscanRpcException()
    {
    }

    /// <summary>Creates an exception with a message.</summary>
    public EtherscanRpcException(string message)
        : base(message)
    {
    }

    /// <summary>Creates an exception with a message and an inner exception.</summary>
    public EtherscanRpcException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }

    /// <summary>The JSON-RPC error code, such as <c>-32000</c>.</summary>
    public int RpcCode { get; internal set; }

    /// <summary>The JSON-RPC error message.</summary>
    public string? RpcMessage { get; internal set; }
}

/// <summary>The response could not be parsed (unexpected shape or non-JSON body).</summary>
public sealed class EtherscanResponseException : EtherscanException
{
    /// <summary>Creates an exception.</summary>
    public EtherscanResponseException()
    {
    }

    /// <summary>Creates an exception with a message.</summary>
    public EtherscanResponseException(string message)
        : base(message)
    {
    }

    /// <summary>Creates an exception with a message and an inner exception.</summary>
    public EtherscanResponseException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
