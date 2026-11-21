namespace EtherscanApi;

/// <summary>
/// Configuration for <see cref="EtherscanClient"/>.
/// </summary>
public sealed class EtherscanClientOptions
{
    /// <summary>The default configuration section name used by the configuration overload of <c>AddEtherscanApi</c>.</summary>
    public const string DefaultSectionName = "Etherscan";

    /// <summary>
    /// Your Etherscan API key (required). One key works across every supported chain.
    /// Create one at <see href="https://etherscan.io/apidashboard"/>.
    /// </summary>
    /// <remarks>
    /// Keep it in a secret store (user-secrets, environment variables, a key vault), never in source control.
    /// The client never writes the key to logs or exception messages.
    /// </remarks>
    public string? ApiKey { get; set; }

    /// <summary>The chain used when a call does not pass a <see cref="ChainId"/>. Defaults to Ethereum Mainnet.</summary>
    public ChainId DefaultChain { get; set; } = ChainId.EthereumMainnet;

    /// <summary>The Etherscan API base address. Defaults to <c>https://api.etherscan.io/</c>.</summary>
    public Uri BaseAddress { get; set; } = new("https://api.etherscan.io/");

    /// <summary>The legacy metadata API base address. Defaults to <c>https://api-metadata.etherscan.io/</c>.</summary>
    public Uri MetadataBaseAddress { get; set; } = new("https://api-metadata.etherscan.io/");

    /// <summary>
    /// Your API plan. Selects the client-side rate-limit preset (Free 3/s, Lite 5/s, Standard 10/s,
    /// Advanced 20/s, Professional and Pro Plus 30/s). Defaults to <see cref="EtherscanPlan.Free"/>.
    /// </summary>
    public EtherscanPlan Plan { get; set; } = EtherscanPlan.Free;

    /// <summary>
    /// Overrides the per-second request limit of <see cref="Plan"/>. Required for <see cref="EtherscanPlan.Custom"/>.
    /// </summary>
    public int? RequestsPerSecond { get; set; }

    /// <summary>
    /// Whether the client throttles its own requests to stay within the plan limit and documented
    /// per-endpoint limits. Defaults to <see langword="true"/>. Disable it only if you throttle elsewhere.
    /// </summary>
    public bool EnableRateLimiting { get; set; } = true;

    /// <summary>
    /// The maximum number of requests that may wait for a rate-limit permit before
    /// <see cref="EtherscanRateLimitException"/> (with <see cref="EtherscanRateLimitException.IsClientSide"/>) is thrown.
    /// Defaults to 1000; at most 1,000,000.
    /// </summary>
    public int MaxQueuedRequests { get; set; } = 1000;

    private const int MaxQueuedRequestsLimit = 1_000_000;

    /// <summary>
    /// How many times a request that failed with a rate-limit or server-timeout error is retried.
    /// Defaults to 3. Set to 0 to disable. Non-idempotent requests (contract verification submissions,
    /// <c>eth_sendRawTransaction</c>) and authentication errors are never retried.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>The base delay of the exponential retry backoff (jitter is added). Defaults to 1 second.</summary>
    public TimeSpan RetryBaseDelay { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// The timeout applied to the <see cref="System.Net.Http.HttpClient"/> the client creates itself.
    /// Ignored when you supply your own <see cref="System.Net.Http.HttpClient"/>. Defaults to 100 seconds.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(100);

    internal int ResolveRequestsPerSecond() => RequestsPerSecond ?? Plan switch
    {
        EtherscanPlan.Free => 3,
        EtherscanPlan.Lite => 5,
        EtherscanPlan.Standard => 10,
        EtherscanPlan.Advanced => 20,
        EtherscanPlan.Professional => 30,
        EtherscanPlan.ProPlus => 30,
        _ => throw new System.Diagnostics.UnreachableException("Options must be validated before use."),
    };

    /// <summary>Throws <see cref="InvalidOperationException"/> with a clear message when the options are invalid.</summary>
    internal void Validate()
    {
        if (GetValidationError() is { } error)
        {
            throw new InvalidOperationException(error);
        }
    }

    /// <summary>Returns a description of the first invalid setting, or <see langword="null"/> when valid.</summary>
    internal string? GetValidationError()
    {
        if (string.IsNullOrWhiteSpace(ApiKey))
        {
            return "EtherscanClientOptions.ApiKey is required. Create a key at https://etherscan.io/apidashboard and supply it "
                   + "through configuration (for example the 'Etherscan:ApiKey' setting, user-secrets or the "
                   + "Etherscan__ApiKey environment variable). Never hard-code it in source.";
        }

        if (BaseAddress is null || !BaseAddress.IsAbsoluteUri)
        {
            return "EtherscanClientOptions.BaseAddress must be an absolute URI.";
        }

        if (MetadataBaseAddress is null || !MetadataBaseAddress.IsAbsoluteUri)
        {
            return "EtherscanClientOptions.MetadataBaseAddress must be an absolute URI.";
        }

        if (RequestsPerSecond is <= 0)
        {
            return "EtherscanClientOptions.RequestsPerSecond must be greater than zero.";
        }

        if (Plan == EtherscanPlan.Custom && RequestsPerSecond is null)
        {
            return "EtherscanClientOptions.RequestsPerSecond must be set when Plan is Custom.";
        }

        if (!Enum.IsDefined(Plan))
        {
            return "EtherscanClientOptions.Plan is not a known plan.";
        }

        if (MaxRetryAttempts < 0)
        {
            return "EtherscanClientOptions.MaxRetryAttempts must not be negative.";
        }

        if (MaxQueuedRequests is < 0 or > MaxQueuedRequestsLimit)
        {
            return $"EtherscanClientOptions.MaxQueuedRequests must be between 0 and {MaxQueuedRequestsLimit}.";
        }

        if (RetryBaseDelay < TimeSpan.Zero)
        {
            return "EtherscanClientOptions.RetryBaseDelay must not be negative.";
        }

        if (Timeout <= TimeSpan.Zero && Timeout != System.Threading.Timeout.InfiniteTimeSpan)
        {
            return "EtherscanClientOptions.Timeout must be positive.";
        }

        return null;
    }

    /// <summary>
    /// Copies the options and ensures base addresses end with '/', so relative paths are appended rather than
    /// replacing the last segment (<c>https://gw/etherscan</c> + <c>v2/api</c> → <c>https://gw/etherscan/v2/api</c>).
    /// </summary>
    internal EtherscanClientOptions Clone()
    {
        var copy = (EtherscanClientOptions)MemberwiseClone();
        copy.BaseAddress = WithTrailingSlash(BaseAddress);
        copy.MetadataBaseAddress = WithTrailingSlash(MetadataBaseAddress);
        return copy;
    }

    private static Uri WithTrailingSlash(Uri uri)
        => uri is { IsAbsoluteUri: true } && !uri.AbsolutePath.EndsWith('/') ? new Uri(uri.AbsoluteUri + "/") : uri;
}
