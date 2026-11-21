using System.Numerics;

namespace EtherscanApi.IntegrationTests;

/// <summary>
/// Read-only calls against the live Etherscan API. They run only when the <c>ETHERSCAN_API_KEY</c>
/// environment variable is set, and use Free-tier endpoints and chains. Nothing here submits
/// verifications or broadcasts transactions.
/// </summary>
public sealed class LiveApiTests : IDisposable
{
    // Ethereum Foundation-era address with a long, stable history.
    private const string WellKnownAddress = "0xde0B295669a9FD93d5F28D9Ec85E40f4cb697BAe";

    // USDT on Ethereum Mainnet: verified, with a fixed creation transaction.
    private const string Usdt = "0xdAC17F958D2ee523a2206206994597C13D831ec7";
    private const string UsdtCreator = "0x36928500bc1dcd7af6a2b4008875cc336b927d57";
    private const string UsdtCreationTx = "0x2f1c5c2b44f771e942a8506148e256f94f1a464babc938ae0690c6e34cd79190";

    private readonly EtherscanClient? _client;

    public LiveApiTests()
    {
        var apiKey = Environment.GetEnvironmentVariable("ETHERSCAN_API_KEY");
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            _client = new EtherscanClient(new EtherscanClientOptions { ApiKey = apiKey, Plan = EtherscanPlan.Free });
        }
    }

    private EtherscanClient Client
    {
        get
        {
            Assert.SkipWhen(_client is null, "Set ETHERSCAN_API_KEY to run live tests.");
            return _client!;
        }
    }

    [Fact]
    public async Task Balance_is_positive()
    {
        var balance = await Client.Accounts.GetBalanceAsync(WellKnownAddress, cancellationToken: TestContext.Current.CancellationToken);
        Assert.True(balance > BigInteger.Zero);
    }

    [Fact]
    public async Task Transactions_first_page_has_requested_size()
    {
        var txs = await Client.Accounts.GetTransactionsAsync(
            WellKnownAddress,
            new BlockRangeQuery { Page = 1, Offset = 5, Sort = SortOrder.Ascending },
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(5, txs.Count);
        Assert.True(txs[0].BlockNumber <= txs[^1].BlockNumber);
    }

    [Fact]
    public async Task Usdt_contract_creation_matches_known_values()
    {
        var creation = Assert.Single(await Client.Contracts.GetContractCreationAsync([Usdt], cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(UsdtCreator, creation.ContractCreator, ignoreCase: true);
        Assert.Equal(UsdtCreationTx, creation.TransactionHash, ignoreCase: true);
    }

    [Fact]
    public async Task Usdt_source_is_verified_and_abi_is_json()
    {
        var source = await Client.Contracts.GetSourceCodeAsync(Usdt, cancellationToken: TestContext.Current.CancellationToken);
        var abi = await Client.Contracts.GetAbiAsync(Usdt, cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(source.IsVerified);
        Assert.Equal("TetherToken", source.ContractName);
        Assert.StartsWith("[", abi, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Proxy_receipt_of_creation_transaction_has_contract_address()
    {
        var receipt = await Client.Proxy.GetTransactionReceiptAsync(UsdtCreationTx, cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(receipt);
        Assert.Equal(Usdt, receipt.ContractAddress, ignoreCase: true);
        Assert.Equal(4634748, receipt.BlockNumber);
    }

    [Fact]
    public async Task Block_number_and_gas_oracle_are_current()
    {
        var blockNumber = await Client.Proxy.GetBlockNumberAsync(cancellationToken: TestContext.Current.CancellationToken);
        var oracle = await Client.GasTracker.GetGasOracleAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(blockNumber > 20_000_000);
        Assert.True(Math.Abs(oracle.LastBlock - blockNumber) < 1_000);
    }

    [Fact]
    public async Task Polygon_balance_works_with_same_key()
    {
        var balance = await Client.Accounts.GetBalanceAsync(WellKnownAddress, chain: ChainId.Polygon, cancellationToken: TestContext.Current.CancellationToken);
        Assert.True(balance >= BigInteger.Zero);
    }

    [Fact]
    public async Task Chain_list_contains_mainnet()
    {
        var chains = await Client.Chains.GetChainListAsync(TestContext.Current.CancellationToken);
        Assert.Contains(chains, c => c.ChainId == ChainId.EthereumMainnet);
    }

    [Fact]
    public async Task Page_beyond_result_window_raises_api_exception()
    {
        // Etherscan caps page × offset on history endpoints; 2 × 10,000 is past the documented-in-practice cap.
        var ex = await Assert.ThrowsAsync<EtherscanApiException>(() => Client.Accounts.GetTransactionsAsync(
            WellKnownAddress,
            new BlockRangeQuery { Page = 2, Offset = 10_000 },
            cancellationToken: TestContext.Current.CancellationToken));

        TestContext.Current.TestOutputHelper?.WriteLine($"Etherscan reply: {ex.ApiMessage} / {ex.ApiResult}");
        Assert.Equal(typeof(EtherscanApiException), ex.GetType());
    }

    [Fact]
    public async Task Enumeration_stops_with_api_exception_at_the_cap()
    {
        // pageSize 10,000 reaches the cap on page 2 if the address has more than 10,000 transactions;
        // otherwise the enumeration simply ends. Both outcomes are acceptable; anything else is not.
        var count = 0;
        var ex = await Record.ExceptionAsync(async () =>
        {
            await foreach (var _ in Client.Accounts.EnumerateTransactionsAsync(
                               WellKnownAddress, pageSize: 10_000, cancellationToken: TestContext.Current.CancellationToken))
            {
                count++;
            }
        });

        TestContext.Current.TestOutputHelper?.WriteLine($"Enumerated {count} transactions; exception: {ex?.GetType().Name} {(ex as EtherscanApiException)?.ApiResult}");
        Assert.True(ex is null || ex.GetType() == typeof(EtherscanApiException), ex?.ToString());
        Assert.True(count > 0);
    }

    [Fact]
    public async Task Api_usage_is_reported()
    {
        var usage = await Client.Usage.GetApiUsageAsync(TestContext.Current.CancellationToken);
        Assert.True(usage.CreditLimit > 0);
    }

    public void Dispose() => _client?.Dispose();
}
