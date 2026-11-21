using System.Globalization;
using EtherscanApi;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>Reflection-free (AOT-safe) binding of <see cref="EtherscanClientOptions"/> from configuration.</summary>
internal static class EtherscanOptionsBinder
{
    public static void Bind(IConfiguration configuration, EtherscanClientOptions options)
    {
        if (configuration[nameof(EtherscanClientOptions.ApiKey)] is { Length: > 0 } apiKey)
        {
            options.ApiKey = apiKey;
        }

        if (configuration[nameof(EtherscanClientOptions.DefaultChain)] is { Length: > 0 } chain)
        {
            options.DefaultChain = ChainId.TryParse(chain, CultureInfo.InvariantCulture, out var id)
                ? id
                : throw Invalid(nameof(EtherscanClientOptions.DefaultChain), chain);
        }

        if (configuration[nameof(EtherscanClientOptions.BaseAddress)] is { Length: > 0 } baseAddress)
        {
            options.BaseAddress = ParseUri(nameof(EtherscanClientOptions.BaseAddress), baseAddress);
        }

        if (configuration[nameof(EtherscanClientOptions.MetadataBaseAddress)] is { Length: > 0 } metadataAddress)
        {
            options.MetadataBaseAddress = ParseUri(nameof(EtherscanClientOptions.MetadataBaseAddress), metadataAddress);
        }

        if (configuration[nameof(EtherscanClientOptions.Plan)] is { Length: > 0 } plan)
        {
            options.Plan = Enum.TryParse<EtherscanPlan>(plan, ignoreCase: true, out var parsed) && Enum.IsDefined(parsed)
                ? parsed
                : throw Invalid(nameof(EtherscanClientOptions.Plan), plan);
        }

        if (configuration[nameof(EtherscanClientOptions.RequestsPerSecond)] is { Length: > 0 } rps)
        {
            options.RequestsPerSecond = ParseInt(nameof(EtherscanClientOptions.RequestsPerSecond), rps);
        }

        if (configuration[nameof(EtherscanClientOptions.EnableRateLimiting)] is { Length: > 0 } enable)
        {
            options.EnableRateLimiting = bool.TryParse(enable, out var value)
                ? value
                : throw Invalid(nameof(EtherscanClientOptions.EnableRateLimiting), enable);
        }

        if (configuration[nameof(EtherscanClientOptions.MaxQueuedRequests)] is { Length: > 0 } queue)
        {
            options.MaxQueuedRequests = ParseInt(nameof(EtherscanClientOptions.MaxQueuedRequests), queue);
        }

        if (configuration[nameof(EtherscanClientOptions.MaxRetryAttempts)] is { Length: > 0 } retries)
        {
            options.MaxRetryAttempts = ParseInt(nameof(EtherscanClientOptions.MaxRetryAttempts), retries);
        }

        if (configuration[nameof(EtherscanClientOptions.RetryBaseDelay)] is { Length: > 0 } delay)
        {
            options.RetryBaseDelay = ParseTimeSpan(nameof(EtherscanClientOptions.RetryBaseDelay), delay);
        }

        if (configuration[nameof(EtherscanClientOptions.Timeout)] is { Length: > 0 } timeout)
        {
            options.Timeout = ParseTimeSpan(nameof(EtherscanClientOptions.Timeout), timeout);
        }
    }

    private static Uri ParseUri(string key, string value)
        => Uri.TryCreate(value, UriKind.Absolute, out var uri) ? uri : throw Invalid(key, value);

    private static int ParseInt(string key, string value)
        => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : throw Invalid(key, value);

    private static TimeSpan ParseTimeSpan(string key, string value)
        => TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out var result) ? result : throw Invalid(key, value);

    private static InvalidOperationException Invalid(string key, string value)
        => new($"Invalid Etherscan configuration value for '{key}': '{value}'.");
}
