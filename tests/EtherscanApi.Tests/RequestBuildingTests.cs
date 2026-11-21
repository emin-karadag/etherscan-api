using System.Numerics;
using EtherscanApi.Models;
using EtherscanApi.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EtherscanApi.Tests;

public sealed class RequestBuildingTests
{
    private const string Address = "0xde0B295669a9FD93d5F28D9Ec85E40f4cb697BAe";
    private const string Contract = "0xdAC17F958D2ee523a2206206994597C13D831ec7";
    private const string Topic = "0xddf252ad1be2c89b69c2b068fc378daa952ba7f163c4a11628f55a4df523b3ef";

    [Fact]
    public async Task Chain_override_and_default_chain_are_sent()
    {
        var handler = FakeHttpMessageHandler.Json(TestClient.Ok("\"1\""));
        using var client = TestClient.Create(handler, o => o.DefaultChain = ChainId.Polygon);

        await client.Accounts.GetBalanceAsync(Address);
        await client.Accounts.GetBalanceAsync(Address, chain: ChainId.Base);

        Assert.Equal("137", handler.Requests[0].Query["chainid"]);
        Assert.Equal("8453", handler.Requests[1].Query["chainid"]);
    }

    [Fact]
    public async Task Txlist_query_parameters_use_documented_names()
    {
        var handler = FakeHttpMessageHandler.Json(TestClient.Ok("[]"));
        using var client = TestClient.Create(handler);

        await client.Accounts.GetTransactionsAsync(Address, new BlockRangeQuery
        {
            StartBlock = 10, EndBlock = 20, Page = 2, Offset = 50, Sort = SortOrder.Descending,
        });

        var q = handler.Requests[0].Query;
        Assert.Equal(Address, q["address"]);
        Assert.Equal("10", q["startblock"]);
        Assert.Equal("20", q["endblock"]);
        Assert.Equal("2", q["page"]);
        Assert.Equal("50", q["offset"]);
        Assert.Equal("desc", q["sort"]);
        Assert.EndsWith("apikey=" + TestClient.ApiKey, handler.Requests[0].Uri.Query, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Advanced_filter_sends_from_to_and_operator()
    {
        var handler = FakeHttpMessageHandler.Json(TestClient.Ok("[]"));
        using var client = TestClient.Create(handler);

        await client.Accounts.GetErc20TransfersAdvancedFilterAsync(new AddressFilter(Address, Contract, FilterOperator.Or), Contract);

        var q = handler.Requests[0].Query;
        Assert.Equal(Address, q["from"]);
        Assert.Equal(Contract, q["to"]);
        Assert.Equal("or", q["fromto_opr"]);
        Assert.Equal(Contract, q["contractaddress"]);
        Assert.Null(q["address"]);
    }

    [Fact]
    public async Task Log_topic_operators_are_sent_only_for_set_topic_pairs()
    {
        var handler = FakeHttpMessageHandler.Json(TestClient.Ok("[]"));
        using var client = TestClient.Create(handler);

        await client.Logs.GetLogsAsync(new LogQuery
        {
            FromBlock = 1,
            ToBlock = 100,
            Topic0 = Topic,
            Topic1 = Topic,
            Topic0And1 = FilterOperator.And,
            Topic1And2 = FilterOperator.Or,
        });

        var q = handler.Requests[0].Query;
        Assert.Equal("1", q["fromBlock"]);
        Assert.Equal("100", q["toBlock"]);
        Assert.Equal("and", q["topic0_1_opr"]);
        Assert.Null(q["topic1_2_opr"]);
    }

    [Fact]
    public async Task Proxy_block_tags_are_hex_encoded()
    {
        var handler = FakeHttpMessageHandler.Json(TestClient.Rpc("null"));
        using var client = TestClient.Create(handler);

        await client.Proxy.GetBlockByNumberAsync(68943);
        await client.Proxy.GetTransactionByBlockNumberAndIndexAsync(BlockTag.Latest, 10);

        Assert.Equal("0x10d4f", handler.Requests[0].Query["tag"]);
        Assert.Equal("false", handler.Requests[0].Query["boolean"]);
        Assert.Equal("latest", handler.Requests[1].Query["tag"]);
        Assert.Equal("0xa", handler.Requests[1].Query["index"]);
    }

    [Fact]
    public async Task Daily_stats_send_iso_dates()
    {
        var handler = FakeHttpMessageHandler.Json(TestClient.Ok("[]"));
        using var client = TestClient.Create(handler);

        await client.Stats.GetDailyTransactionCountAsync(new DateOnly(2019, 2, 1), new DateOnly(2019, 2, 28), SortOrder.Ascending);

        var q = handler.Requests[0].Query;
        Assert.Equal("2019-02-01", q["startdate"]);
        Assert.Equal("2019-02-28", q["enddate"]);
        Assert.Equal("asc", q["sort"]);
    }

    [Fact]
    public async Task Verify_solidity_posts_documented_form_fields()
    {
        var handler = FakeHttpMessageHandler.Json(TestClient.Ok("\"guid\""));
        using var client = TestClient.Create(handler);

        var guid = await client.Contracts.VerifySolidityAsync(new SolidityVerificationRequest
        {
            ContractAddress = Contract,
            SourceCode = "{\"language\":\"Solidity\"}",
            ContractName = "contracts/MyToken.sol:MyToken",
            CompilerVersion = "v0.8.28+commit.7893614a",
            ConstructorArguments = "0x0000aa",
            LicenseType = 3,
        });

        Assert.Equal("guid", guid);
        var sent = handler.Requests[0];
        Assert.Equal(HttpMethod.Post, sent.Method);
        Assert.Equal("verifysourcecode", sent.Query["action"]);
        Assert.Equal(Contract, sent.Form["contractaddress"]);
        Assert.Equal("solidity-standard-json-input", sent.Form["codeformat"]);
        Assert.Equal("contracts/MyToken.sol:MyToken", sent.Form["contractname"]);
        Assert.Equal("0000aa", sent.Form["constructorArguments"]);
        Assert.Equal("3", sent.Form["licenseType"]);
        Assert.Null(sent.Form["optimizationUsed"]);
    }

    [Fact]
    public async Task Enumerate_pages_until_a_short_page()
    {
        var page = string.Join(',', Enumerable.Repeat("""{"hash":"0x1","blockNumber":"1"}""", 2));
        var handler = FakeHttpMessageHandler.Sequence(
            TestClient.Ok($"[{page}]"),
            TestClient.Ok($"[{page}]"),
            TestClient.Ok("""[{"hash":"0x1","blockNumber":"1"}]"""));
        using var client = TestClient.Create(handler);

        var items = new List<NormalTransaction>();
        await foreach (var tx in client.Accounts.EnumerateTransactionsAsync(Address, pageSize: 2))
        {
            items.Add(tx);
        }

        Assert.Equal(5, items.Count);
        Assert.Equal(["1", "2", "3"], handler.Requests.Select(r => r.Query["page"]!));
        Assert.All(handler.Requests, r => Assert.Equal("2", r.Query["offset"]));
    }

    [Fact]
    public async Task Enumerate_stops_on_no_records()
    {
        var handler = FakeHttpMessageHandler.Sequence(
            TestClient.Ok("""[{"hash":"0x1"},{"hash":"0x2"}]"""),
            """{"status":"0","message":"No transactions found","result":[]}""");
        using var client = TestClient.Create(handler);

        var count = 0;
        await foreach (var _ in client.Accounts.EnumerateTransactionsAsync(Address, pageSize: 2))
        {
            count++;
        }

        Assert.Equal(2, count);
        Assert.Equal(2, handler.Requests.Count);
    }

    [Fact]
    public async Task Ens_resolve_uses_mainnet_even_with_other_default_chain()
    {
        var handler = FakeHttpMessageHandler.Json(TestClient.Ok($"\"{Address}\""));
        using var client = TestClient.Create(handler, o => o.DefaultChain = ChainId.Base);

        var resolved = await client.Ens.ResolveAsync("vitalik.eth");

        Assert.Equal(Address, resolved);
        Assert.Equal("1", handler.Requests[0].Query["chainid"]);
        Assert.Equal("vitalik.eth", handler.Requests[0].Query["name"]);
    }

    [Theory]
    [InlineData("0x", "0")]
    [InlineData("0x80", "128")]
    [InlineData("0xff", "255")]
    [InlineData("0xde0b6b3a7640000", "1000000000000000000")]
    [InlineData("115792089237316195423570985008687907853269984665640564039457584007913129639935", "115792089237316195423570985008687907853269984665640564039457584007913129639935")]
    [InlineData("1e3", "1000")]
    public void Big_integer_parsing_handles_hex_and_decimal(string input, string expected)
    {
        Assert.True(EtherscanNumber.TryParseBigInteger(input, out var value));
        Assert.Equal(BigInteger.Parse(expected, System.Globalization.CultureInfo.InvariantCulture), value);
    }

    [Theory]
    [InlineData("1", true)]
    [InlineData("0", false)]
    [InlineData("true", true)]
    [InlineData("0x1", true)]
    [InlineData("0x0", false)]
    public void Boolean_parsing_handles_etherscan_flags(string input, bool expected)
    {
        Assert.True(EtherscanNumber.TryParseBoolean(input, out var value));
        Assert.Equal(expected, value);
    }

    [Fact]
    public void Chain_id_parses_and_formats()
    {
        Assert.True(ChainId.TryParse("8453", null, out var chain));
        Assert.Equal(ChainId.Base, chain);
        Assert.Equal("8453", chain.ToString());
        var raw = (ulong)chain;
        Assert.Equal(8453UL, raw);
    }

    [Fact]
    public void Options_validate_custom_plan_requires_rate()
    {
        Assert.Throws<InvalidOperationException>(() => new EtherscanClient(new EtherscanClientOptions { ApiKey = "k", Plan = EtherscanPlan.Custom }));
        using var ok = new EtherscanClient(new EtherscanClientOptions { ApiKey = "k", Plan = EtherscanPlan.Custom, RequestsPerSecond = 50 });
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Missing_api_key_fails_fast_with_clear_message(string? apiKey)
    {
        var ex = Assert.Throws<InvalidOperationException>(() => new EtherscanClient(new EtherscanClientOptions { ApiKey = apiKey }));
        Assert.Contains("ApiKey is required", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Dependency_injection_reports_missing_api_key_at_resolve_time()
    {
        var services = new ServiceCollection();
        services.AddEtherscanApi(_ => { });
        await using var provider = services.BuildServiceProvider();

        var ex = Assert.Throws<Microsoft.Extensions.Options.OptionsValidationException>(() => provider.GetRequiredService<IEtherscanClient>());
        Assert.Contains("ApiKey is required", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Modules_are_mockable_through_interfaces()
    {
        IAccountsModule accounts = new StubAccounts();
        var balance = await accounts.GetBalanceAsync(Address);
        Assert.Equal(new BigInteger(7), balance);
    }

    [Fact]
    public async Task Empty_array_with_whitespace_is_an_empty_list()
    {
        using var client = TestClient.Create(FakeHttpMessageHandler.Json("""{"status":"0","message":"NOTOK","result":[ ]}"""));
        Assert.Empty(await client.Accounts.GetTransactionsAsync(Address));
    }

    private sealed class StubAccounts : IAccountsModule
    {
        private static NotSupportedException NotUsed() => new();

        public Task<BigInteger> GetBalanceAsync(string address, BlockTag? tag = null, ChainId? chain = null, CancellationToken cancellationToken = default)
            => Task.FromResult(new BigInteger(7));

        public Task<IReadOnlyList<AddressBalance>> GetBalancesAsync(IEnumerable<string> addresses, BlockTag? tag = null, ChainId? chain = null, CancellationToken cancellationToken = default) => throw NotUsed();
        public Task<BigInteger> GetHistoricalBalanceAsync(string address, long blockNumber, ChainId? chain = null, CancellationToken cancellationToken = default) => throw NotUsed();
        public Task<IReadOnlyList<NormalTransaction>> GetTransactionsAsync(string address, BlockRangeQuery? query = null, ChainId? chain = null, CancellationToken cancellationToken = default) => throw NotUsed();
        public IAsyncEnumerable<NormalTransaction> EnumerateTransactionsAsync(string address, BlockRangeQuery? query = null, int? pageSize = null, ChainId? chain = null, CancellationToken cancellationToken = default) => throw NotUsed();
        public Task<IReadOnlyList<NormalTransaction>> GetTransactionsAdvancedFilterAsync(AddressFilter filter, BlockRangeQuery? query = null, ChainId? chain = null, CancellationToken cancellationToken = default) => throw NotUsed();
        public Task<IReadOnlyList<InternalTransaction>> GetInternalTransactionsAsync(string address, BlockRangeQuery? query = null, ChainId? chain = null, CancellationToken cancellationToken = default) => throw NotUsed();
        public IAsyncEnumerable<InternalTransaction> EnumerateInternalTransactionsAsync(string address, BlockRangeQuery? query = null, int? pageSize = null, ChainId? chain = null, CancellationToken cancellationToken = default) => throw NotUsed();
        public Task<IReadOnlyList<InternalTransaction>> GetInternalTransactionsByHashAsync(string transactionHash, ChainId? chain = null, CancellationToken cancellationToken = default) => throw NotUsed();
        public Task<IReadOnlyList<InternalTransaction>> GetInternalTransactionsByBlockRangeAsync(long startBlock, long endBlock, PageQuery? page = null, ChainId? chain = null, CancellationToken cancellationToken = default) => throw NotUsed();
        public Task<IReadOnlyList<InternalTransaction>> GetInternalTransactionsAdvancedFilterAsync(AddressFilter filter, BlockRangeQuery? query = null, ChainId? chain = null, CancellationToken cancellationToken = default) => throw NotUsed();
        public Task<IReadOnlyList<Erc20Transfer>> GetErc20TransfersAsync(string? address = null, string? contractAddress = null, BlockRangeQuery? query = null, ChainId? chain = null, CancellationToken cancellationToken = default) => throw NotUsed();
        public IAsyncEnumerable<Erc20Transfer> EnumerateErc20TransfersAsync(string? address = null, string? contractAddress = null, BlockRangeQuery? query = null, int? pageSize = null, ChainId? chain = null, CancellationToken cancellationToken = default) => throw NotUsed();
        public Task<IReadOnlyList<Erc20Transfer>> GetErc20TransfersAdvancedFilterAsync(AddressFilter filter, string? contractAddress = null, BlockRangeQuery? query = null, ChainId? chain = null, CancellationToken cancellationToken = default) => throw NotUsed();
        public Task<IReadOnlyList<Erc721Transfer>> GetErc721TransfersAsync(string? address = null, string? contractAddress = null, BlockRangeQuery? query = null, ChainId? chain = null, CancellationToken cancellationToken = default) => throw NotUsed();
        public IAsyncEnumerable<Erc721Transfer> EnumerateErc721TransfersAsync(string? address = null, string? contractAddress = null, BlockRangeQuery? query = null, int? pageSize = null, ChainId? chain = null, CancellationToken cancellationToken = default) => throw NotUsed();
        public Task<IReadOnlyList<Erc721Transfer>> GetErc721TransfersAdvancedFilterAsync(AddressFilter filter, string? contractAddress = null, BlockRangeQuery? query = null, ChainId? chain = null, CancellationToken cancellationToken = default) => throw NotUsed();
        public Task<IReadOnlyList<Erc1155Transfer>> GetErc1155TransfersAsync(string? address = null, string? contractAddress = null, BlockRangeQuery? query = null, ChainId? chain = null, CancellationToken cancellationToken = default) => throw NotUsed();
        public IAsyncEnumerable<Erc1155Transfer> EnumerateErc1155TransfersAsync(string? address = null, string? contractAddress = null, BlockRangeQuery? query = null, int? pageSize = null, ChainId? chain = null, CancellationToken cancellationToken = default) => throw NotUsed();
        public Task<IReadOnlyList<Erc1155Transfer>> GetErc1155TransfersAdvancedFilterAsync(AddressFilter filter, string? contractAddress = null, BlockRangeQuery? query = null, ChainId? chain = null, CancellationToken cancellationToken = default) => throw NotUsed();
        public Task<IReadOnlyList<MinedBlock>> GetMinedBlocksAsync(string address, MinedBlockType? blockType = null, PageQuery? page = null, ChainId? chain = null, CancellationToken cancellationToken = default) => throw NotUsed();
        public Task<IReadOnlyList<BeaconWithdrawal>> GetBeaconWithdrawalsAsync(string? address = null, BlockRangeQuery? query = null, ChainId? chain = null, CancellationToken cancellationToken = default) => throw NotUsed();
        public Task<FundingSource> GetFundedByAsync(string address, ChainId? chain = null, CancellationToken cancellationToken = default) => throw NotUsed();
    }

    [Fact]
    public async Task Client_side_rate_limiter_spaces_requests()
    {
        var handler = FakeHttpMessageHandler.Json(TestClient.Ok("\"1\""));
        using var client = TestClient.Create(handler, o =>
        {
            o.EnableRateLimiting = true;
            o.RequestsPerSecond = 2;
        });

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await Task.WhenAll(Enumerable.Range(0, 5).Select(_ => client.Accounts.GetBalanceAsync(Address)));
        stopwatch.Stop();

        Assert.Equal(5, handler.Requests.Count);
        // 2 permits per 1.1s window: 5 calls need at least two extra windows.
        Assert.True(stopwatch.Elapsed >= TimeSpan.FromSeconds(1.5), $"Elapsed {stopwatch.Elapsed}");
    }

    [Fact]
    public async Task Dependency_injection_registers_singleton_client_from_configuration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Etherscan:ApiKey"] = TestClient.ApiKey,
                ["Etherscan:DefaultChain"] = "137",
                ["Etherscan:Plan"] = "Standard",
                ["Etherscan:Timeout"] = "00:00:30",
            })
            .Build();

        var handler = FakeHttpMessageHandler.Json(TestClient.Ok("\"1\""));
        var services = new ServiceCollection();
        services.AddEtherscanApi(configuration.GetSection("Etherscan"))
            .ConfigurePrimaryHttpMessageHandler(() => handler);

        await using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IEtherscanClient>();

        Assert.Same(client, provider.GetRequiredService<IEtherscanClient>());
        Assert.Same(client, provider.GetRequiredService<EtherscanClient>());

        await client.Accounts.GetBalanceAsync(Address);
        Assert.Equal("137", handler.Requests[0].Query["chainid"]);
        Assert.Equal(TestClient.ApiKey, handler.Requests[0].Query["apikey"]);
    }
}
