using EtherscanApi.Internal;
using Microsoft.Extensions.Logging;

namespace EtherscanApi;

/// <summary>
/// Client for the Etherscan API V2. One API key, 50+ EVM chains. Endpoints are grouped by module.
/// </summary>
/// <remarks>
/// This interface and the module interfaces exist so you can depend on and mock the client. They are meant to
/// be consumed, not implemented: new members may be added in minor versions as Etherscan adds endpoints.
/// </remarks>
public interface IEtherscanClient
{
    /// <summary>Account endpoints: balances, transactions, token transfers, validated blocks, beacon withdrawals.</summary>
    IAccountsModule Accounts { get; }

    /// <summary>Token endpoints: supply, balances, holders, token info, address holdings.</summary>
    ITokensModule Tokens { get; }

    /// <summary>Block endpoints and daily block statistics.</summary>
    IBlocksModule Blocks { get; }

    /// <summary>Contract endpoints: ABI, source code, creation info and verification.</summary>
    IContractsModule Contracts { get; }

    /// <summary>Gas tracker endpoints and daily gas statistics.</summary>
    IGasTrackerModule GasTracker { get; }

    /// <summary>Geth/Parity JSON-RPC proxy endpoints (<c>eth_*</c>).</summary>
    IProxyModule Proxy { get; }

    /// <summary>Event log endpoints.</summary>
    ILogsModule Logs { get; }

    /// <summary>Network statistics endpoints.</summary>
    IStatsModule Stats { get; }

    /// <summary>Transaction status endpoints.</summary>
    ITransactionsModule Transactions { get; }

    /// <summary>L2 bridge deposit and withdrawal endpoints.</summary>
    IL2Module L2 { get; }

    /// <summary>ENS endpoints.</summary>
    IEnsModule Ens { get; }

    /// <summary>Name tag, label and metadata export endpoints (multichain).</summary>
    INametagsModule Nametags { get; }

    /// <summary>Legacy (V1) metadata export endpoints.</summary>
    ILegacyMetadataModule LegacyMetadata { get; }

    /// <summary>API usage endpoint.</summary>
    IUsageModule Usage { get; }

    /// <summary>Supported chain list endpoint.</summary>
    IChainsModule Chains { get; }
}

/// <summary>
/// Client for the Etherscan API V2. Thread-safe; create one instance and reuse it
/// (or register it with <c>services.AddEtherscanApi(...)</c>).
/// </summary>
/// <example>
/// <code>
/// using var client = new EtherscanClient("YourApiKeyToken");
/// var wei = await client.Accounts.GetBalanceAsync("0xde0B295669a9FD93d5F28D9Ec85E40f4cb697BAe");
/// var baseTxs = await client.Accounts.GetTransactionsAsync(address, chain: ChainId.Base);
/// </code>
/// </example>
public sealed class EtherscanClient : IEtherscanClient, IDisposable
{
    private readonly EtherscanTransport _transport;

    /// <summary>Creates a client for an API key with default options (Free plan limits, Ethereum Mainnet).</summary>
    /// <param name="apiKey">Your Etherscan API key.</param>
    public EtherscanClient(string apiKey)
        : this(new EtherscanClientOptions { ApiKey = apiKey })
    {
    }

    /// <summary>Creates a client.</summary>
    /// <param name="options">Client options. A copy is taken; later changes have no effect.</param>
    /// <param name="httpClient">
    /// An optional <see cref="HttpClient"/>. When supplied, the caller owns it and its timeout and handlers apply.
    /// When omitted, the client creates one with a pooled <see cref="SocketsHttpHandler"/>.
    /// </param>
    /// <param name="logger">Optional logger. API keys and request URIs are never logged.</param>
    public EtherscanClient(EtherscanClientOptions options, HttpClient? httpClient = null, ILogger<EtherscanClient>? logger = null)
        : this(options, httpClient, logger, rateLimiter: null, timeProvider: null)
    {
    }

    internal EtherscanClient(
        EtherscanClientOptions options,
        HttpClient? httpClient,
        ILogger? logger,
        EtherscanRateLimiter? rateLimiter,
        TimeProvider? timeProvider)
    {
        ArgumentNullException.ThrowIfNull(options);
        var snapshot = options.Clone();
        snapshot.Validate();

        var ownsHttpClient = httpClient is null;
        httpClient ??= new HttpClient(new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(15),
            AutomaticDecompression = System.Net.DecompressionMethods.All,
        })
        {
            Timeout = snapshot.Timeout,
        };

        var ownsLimiter = false;
        if (rateLimiter is null && snapshot.EnableRateLimiting)
        {
            rateLimiter = new EtherscanRateLimiter(snapshot.ResolveRequestsPerSecond(), snapshot.MaxQueuedRequests, timeProvider);
            ownsLimiter = true;
        }
        else if (!snapshot.EnableRateLimiting)
        {
            rateLimiter = null;
        }

        _transport = new EtherscanTransport(httpClient, snapshot, rateLimiter, logger, timeProvider, ownsHttpClient, ownsLimiter);

        Accounts = new AccountsModule(_transport);
        Tokens = new TokensModule(_transport);
        Blocks = new BlocksModule(_transport);
        Contracts = new ContractsModule(_transport);
        GasTracker = new GasTrackerModule(_transport);
        Proxy = new ProxyModule(_transport);
        Logs = new LogsModule(_transport);
        Stats = new StatsModule(_transport);
        Transactions = new TransactionsModule(_transport);
        L2 = new L2Module(_transport);
        Ens = new EnsModule(_transport);
        Nametags = new NametagsModule(_transport);
        LegacyMetadata = new LegacyMetadataModule(_transport);
        Usage = new UsageModule(_transport);
        Chains = new ChainsModule(_transport);
    }

    /// <summary>The chain used when a call does not pass one.</summary>
    public ChainId DefaultChain => _transport.Options.DefaultChain;

    /// <inheritdoc />
    public IAccountsModule Accounts { get; }

    /// <inheritdoc />
    public ITokensModule Tokens { get; }

    /// <inheritdoc />
    public IBlocksModule Blocks { get; }

    /// <inheritdoc />
    public IContractsModule Contracts { get; }

    /// <inheritdoc />
    public IGasTrackerModule GasTracker { get; }

    /// <inheritdoc />
    public IProxyModule Proxy { get; }

    /// <inheritdoc />
    public ILogsModule Logs { get; }

    /// <inheritdoc />
    public IStatsModule Stats { get; }

    /// <inheritdoc />
    public ITransactionsModule Transactions { get; }

    /// <inheritdoc />
    public IL2Module L2 { get; }

    /// <inheritdoc />
    public IEnsModule Ens { get; }

    /// <inheritdoc />
    public INametagsModule Nametags { get; }

    /// <inheritdoc />
    public ILegacyMetadataModule LegacyMetadata { get; }

    /// <inheritdoc />
    public IUsageModule Usage { get; }

    /// <inheritdoc />
    public IChainsModule Chains { get; }

    /// <summary>Releases the <see cref="HttpClient"/> and rate limiter the client created itself.</summary>
    public void Dispose() => _transport.Dispose();
}
