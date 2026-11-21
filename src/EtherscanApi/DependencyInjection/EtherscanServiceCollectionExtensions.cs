using EtherscanApi;
using EtherscanApi.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>Registers <see cref="IEtherscanClient"/> with dependency injection.</summary>
public static class EtherscanServiceCollectionExtensions
{
    /// <summary>The name of the <see cref="HttpClient"/> used by the Etherscan client.</summary>
    public const string HttpClientName = "EtherscanApi";

    /// <summary>
    /// Registers <see cref="IEtherscanClient"/> and <see cref="EtherscanClient"/> as singletons backed by
    /// <c>IHttpClientFactory</c>, with one shared client-side rate limiter.
    /// </summary>
    /// <remarks>
    /// Options are validated when the host starts (and on first use without a host): a missing API key
    /// fails fast with a clear message instead of on the first request.
    /// </remarks>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Configures the options.</param>
    /// <returns>
    /// The <see cref="IHttpClientBuilder"/> of the underlying HTTP client, so you can add handlers such as
    /// <c>AddStandardResilienceHandler()</c>. If you add a resilience handler, disable its retries for unsafe
    /// methods: Etherscan verification submissions are POSTs and must not be resent.
    /// </returns>
    /// <example>
    /// <code>
    /// builder.Services.AddEtherscanApi(o =>
    /// {
    ///     o.ApiKey = builder.Configuration["Etherscan:ApiKey"];
    ///     o.Plan = EtherscanPlan.Standard;
    /// });
    /// </code>
    /// </example>
    public static IHttpClientBuilder AddEtherscanApi(this IServiceCollection services, Action<EtherscanClientOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);
        services.AddOptions<EtherscanClientOptions>().Configure(configure);
        return AddCore(services);
    }

    /// <summary>
    /// Registers <see cref="IEtherscanClient"/> using a configuration section such as <c>"Etherscan"</c>.
    /// </summary>
    /// <remarks>
    /// Supported keys: <c>ApiKey</c>, <c>DefaultChain</c> (numeric chain id), <c>BaseAddress</c>,
    /// <c>MetadataBaseAddress</c>, <c>Plan</c> (Free, Lite, Standard, Advanced, Professional, ProPlus, Custom),
    /// <c>RequestsPerSecond</c>, <c>EnableRateLimiting</c>, <c>MaxQueuedRequests</c>, <c>MaxRetryAttempts</c>,
    /// <c>RetryBaseDelay</c> and <c>Timeout</c> (TimeSpan, e.g. <c>00:00:30</c>). Binding is reflection-free,
    /// so it is trimming- and AOT-safe.
    /// </remarks>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration section, e.g. <c>builder.Configuration.GetSection("Etherscan")</c>.</param>
    /// <param name="configure">Optional extra configuration applied after the section.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/> of the underlying HTTP client.</returns>
    public static IHttpClientBuilder AddEtherscanApi(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<EtherscanClientOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        services.AddOptions<EtherscanClientOptions>().Configure(options =>
        {
            EtherscanOptionsBinder.Bind(configuration, options);
            configure?.Invoke(options);
        });
        return AddCore(services);
    }

    private static IHttpClientBuilder AddCore(IServiceCollection services)
    {
        services.AddOptions<EtherscanClientOptions>().ValidateOnStart();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<EtherscanClientOptions>, EtherscanOptionsValidator>());

        services.TryAddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<EtherscanClientOptions>>().Value;
            return new RateLimiterHolder(options.EnableRateLimiting
                ? new EtherscanRateLimiter(options.ResolveRequestsPerSecond(), options.MaxQueuedRequests, sp.GetService<TimeProvider>())
                : null);
        });

        services.TryAddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<EtherscanClientOptions>>().Value;
            var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName);
            var logger = sp.GetService<ILoggerFactory>()?.CreateLogger<EtherscanClient>();
            var limiter = sp.GetRequiredService<RateLimiterHolder>().Limiter;
            var timeProvider = sp.GetService<TimeProvider>();
            return new EtherscanClient(options, httpClient, logger, limiter, timeProvider);
        });
        services.TryAddSingleton<IEtherscanClient>(sp => sp.GetRequiredService<EtherscanClient>());

        // The client is a singleton that keeps its HttpClient, so DNS changes are handled by
        // PooledConnectionLifetime instead of handler rotation.
        return services
            .AddHttpClient(HttpClientName, (sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<EtherscanClientOptions>>().Value;
                client.Timeout = options.Timeout;
            })
            .ConfigurePrimaryHttpMessageHandler(static () => new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(15),
                AutomaticDecompression = System.Net.DecompressionMethods.All,
            })
            .SetHandlerLifetime(Timeout.InfiniteTimeSpan);
    }

    /// <summary>Validates <see cref="EtherscanClientOptions"/> with a descriptive failure message.</summary>
    private sealed class EtherscanOptionsValidator : IValidateOptions<EtherscanClientOptions>
    {
        public ValidateOptionsResult Validate(string? name, EtherscanClientOptions options)
            => options.GetValidationError() is { } error ? ValidateOptionsResult.Fail(error) : ValidateOptionsResult.Success;
    }

    /// <summary>Owns the shared rate limiter so the container disposes it.</summary>
    private sealed class RateLimiterHolder(EtherscanRateLimiter? limiter) : IDisposable
    {
        public EtherscanRateLimiter? Limiter { get; } = limiter;

        public void Dispose() => Limiter?.Dispose();
    }
}
