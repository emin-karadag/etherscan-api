using EtherscanApi.Models;

namespace EtherscanApi.Tests;

/// <summary>
/// Every OpenAPI operation published in the Etherscan docs (/openapi/.../endpoint/*.json, 97 files in
/// November 2026) is mapped to a client method, and each call sends the documented route, module and action.
/// </summary>
public sealed class EndpointCoverageTests
{
    private const string A = "0xde0B295669a9FD93d5F28D9Ec85E40f4cb697BAe";
    private const string C = "0xdAC17F958D2ee523a2206206994597C13D831ec7";
    private const string H = "0x2f1c5c2b44f771e942a8506148e256f94f1a464babc938ae0690c6e34cd79190";
    private const string Url = "\"https://metadata-export.etherscan.io/1/file.csv\"";

    /// <summary>The OpenAPI file names listed by the Etherscan Docs MCP (find /openapi -name '*.json').</summary>
    private static readonly string[] DocumentedOperations =
    [
        "addresstokenbalance", "addresstokennftbalance", "addresstokennftinventory", "advanced-filter-token1155tx",
        "advanced-filter-tokennfttx", "advanced-filter-tokentx", "advanced-filter-txlist", "advanced-filter-txlistinternal",
        "balance", "balancehistory", "balancemulti", "chainlist", "chainsize", "checkproxyverification", "checkverifystatus",
        "dailyavgblocksize", "dailyavgblocktime", "dailyavggaslimit", "dailyavggasprice", "dailyavghashrate",
        "dailyavgnetdifficulty", "dailyblkcount", "dailyblockrewards", "dailyensregister", "dailygasused",
        "dailynetutilization", "dailynewaddress", "dailytx", "dailytxnfee", "dailyuncleblkcount", "ethblocknumber", "ethcall",
        "ethdailyprice", "ethestimategas", "ethgasprice", "ethgetblockbynumber", "ethgetblocktransactioncountbynumber",
        "ethgetcode", "ethgetstorageat", "ethgettransactionbyblocknumberandindex", "ethgettransactionbyhash",
        "ethgettransactioncount", "ethgettransactionreceipt", "ethgetunclebyblocknumberandindex", "ethprice",
        "ethsendrawtransaction", "ethsupply", "ethsupply2", "exportaddresstags", "exportaddresstags-v2", "forwardresolve",
        "fundedby", "gasestimate", "gasoracle", "getabi", "getaddresstag", "getapilimit", "getblockcountdown",
        "getblocknobytime", "getblockreward", "getblocktxnscount", "getcontractcreation", "getdeposittxs",
        "getlabelmasterlist", "getlabelmasterlist-v2", "getlogs", "getlogs-address-topics", "getlogs-topics",
        "getminedblocks", "getsourcecode", "getstatus", "gettxreceiptstatus", "getwithdrawaltxs", "nodecount",
        "nodecounthistory", "token1155tx", "tokenbalance", "tokenbalancehistory", "tokenholdercount", "tokenholderlist",
        "tokeninfo", "tokennfttx", "tokensupply", "tokensupplyhistory", "tokentx", "topholders", "txlist", "txlistinternal",
        "txlistinternal-blockrange", "txlistinternal-txhash", "txnbridge", "txsbeaconwithdrawal", "verifyproxycontract",
        "verifysourcecode", "verifystylus", "verifyvyper", "verifyzksyncsourcecode",
    ];

    private static readonly DateOnly D1 = new(2019, 2, 1);
    private static readonly DateOnly D2 = new(2019, 2, 28);

    public sealed record EndpointCase(
        string OpenApiId,
        string? Module,
        string? Action,
        string Response,
        Func<EtherscanClient, Task> Invoke,
        string Method = "GET",
        string Path = "/v2/api",
        string Host = "api.etherscan.io",
        bool HasChain = true);

    private static EndpointCase Std(string id, string module, string action, string result, Func<EtherscanClient, Task> invoke)
        => new(id, module, action, TestClient.Ok(result), invoke);

    private static EndpointCase Daily(string id, string action, Func<EtherscanClient, Task> invoke)
        => new(id, "stats", action, TestClient.Ok("[]"), invoke);

    private static EndpointCase Rpc(string id, string action, string result, Func<EtherscanClient, Task> invoke)
        => new(id, "proxy", action, TestClient.Rpc(result), invoke);

    private static EndpointCase Post(string id, string action, Func<EtherscanClient, Task> invoke)
        => new(id, "contract", action, TestClient.Ok("\"guid123\""), invoke, Method: "POST");

    public static readonly EndpointCase[] Cases =
    [
        // account
        Std("balance", "account", "balance", "\"1\"", c => c.Accounts.GetBalanceAsync(A)),
        Std("balancemulti", "account", "balancemulti", "[]", c => c.Accounts.GetBalancesAsync([A, C])),
        Std("balancehistory", "account", "balancehistory", "\"1\"", c => c.Accounts.GetHistoricalBalanceAsync(A, 100)),
        Std("txlist", "account", "txlist", "[]", c => c.Accounts.GetTransactionsAsync(A)),
        Std("advanced-filter-txlist", "account", "txlist", "[]", c => c.Accounts.GetTransactionsAdvancedFilterAsync(new AddressFilter(From: A))),
        Std("txlistinternal", "account", "txlistinternal", "[]", c => c.Accounts.GetInternalTransactionsAsync(A)),
        Std("txlistinternal-txhash", "account", "txlistinternal", "[]", c => c.Accounts.GetInternalTransactionsByHashAsync(H)),
        Std("txlistinternal-blockrange", "account", "txlistinternal", "[]", c => c.Accounts.GetInternalTransactionsByBlockRangeAsync(1, 2)),
        Std("advanced-filter-txlistinternal", "account", "txlistinternal", "[]", c => c.Accounts.GetInternalTransactionsAdvancedFilterAsync(new AddressFilter(To: A))),
        Std("tokentx", "account", "tokentx", "[]", c => c.Accounts.GetErc20TransfersAsync(A)),
        Std("advanced-filter-tokentx", "account", "tokentx", "[]", c => c.Accounts.GetErc20TransfersAdvancedFilterAsync(new AddressFilter(A, C, FilterOperator.Or))),
        Std("tokennfttx", "account", "tokennfttx", "[]", c => c.Accounts.GetErc721TransfersAsync(A)),
        Std("advanced-filter-tokennfttx", "account", "tokennfttx", "[]", c => c.Accounts.GetErc721TransfersAdvancedFilterAsync(new AddressFilter(From: A))),
        Std("token1155tx", "account", "token1155tx", "[]", c => c.Accounts.GetErc1155TransfersAsync(A)),
        Std("advanced-filter-token1155tx", "account", "token1155tx", "[]", c => c.Accounts.GetErc1155TransfersAdvancedFilterAsync(new AddressFilter(From: A))),
        Std("getminedblocks", "account", "getminedblocks", "[]", c => c.Accounts.GetMinedBlocksAsync(A)),
        Std("txsbeaconwithdrawal", "account", "txsBeaconWithdrawal", "[]", c => c.Accounts.GetBeaconWithdrawalsAsync(A)),
        Std("fundedby", "account", "fundedby", "{}", c => c.Accounts.GetFundedByAsync(A)),
        Std("tokenbalance", "account", "tokenbalance", "\"1\"", c => c.Tokens.GetBalanceAsync(C, A)),
        Std("tokenbalancehistory", "account", "tokenbalancehistory", "\"1\"", c => c.Tokens.GetHistoricalBalanceAsync(C, A, 1)),
        Std("addresstokenbalance", "account", "addresstokenbalance", "[]", c => c.Tokens.GetAddressTokenBalancesAsync(A)),
        Std("addresstokennftbalance", "account", "addresstokennftbalance", "[]", c => c.Tokens.GetAddressNftBalancesAsync(A)),
        Std("addresstokennftinventory", "account", "addresstokennftinventory", "[]", c => c.Tokens.GetAddressNftInventoryAsync(A, C)),
        Std("txnbridge", "account", "txnbridge", "[]", c => c.L2.GetPlasmaDepositsAsync(A)),
        Std("getdeposittxs", "account", "getdeposittxs", "[]", c => c.L2.GetDepositTransactionsAsync(A)),
        Std("getwithdrawaltxs", "account", "getwithdrawaltxs", "[]", c => c.L2.GetWithdrawalTransactionsAsync(A)),

        // token
        Std("topholders", "token", "topholders", "[]", c => c.Tokens.GetTopHoldersAsync(C, 10)),
        Std("tokenholderlist", "token", "tokenholderlist", "[]", c => c.Tokens.GetHoldersAsync(C)),
        Std("tokenholdercount", "token", "tokenholdercount", "\"5\"", c => c.Tokens.GetHolderCountAsync(C)),
        Std("tokeninfo", "token", "tokeninfo", "[]", c => c.Tokens.GetTokenInfoAsync(C)),

        // block
        Std("getblockreward", "block", "getblockreward", "{}", c => c.Blocks.GetBlockRewardAsync(1)),
        Std("getblocktxnscount", "block", "getblocktxnscount", "{}", c => c.Blocks.GetBlockTransactionCountsAsync(1)),
        Std("getblockcountdown", "block", "getblockcountdown", "{}", c => c.Blocks.GetBlockCountdownAsync(1)),
        Std("getblocknobytime", "block", "getblocknobytime", "\"123\"", c => c.Blocks.GetBlockNumberByTimeAsync(DateTimeOffset.UnixEpoch.AddYears(50))),

        // stats
        Daily("dailyavgblocksize", "dailyavgblocksize", c => c.Blocks.GetDailyAverageBlockSizeAsync(D1, D2)),
        Daily("dailyblkcount", "dailyblkcount", c => c.Blocks.GetDailyBlockCountAsync(D1, D2)),
        Daily("dailyblockrewards", "dailyblockrewards", c => c.Blocks.GetDailyBlockRewardsAsync(D1, D2)),
        Daily("dailyavgblocktime", "dailyavgblocktime", c => c.Blocks.GetDailyAverageBlockTimeAsync(D1, D2)),
        Daily("dailyuncleblkcount", "dailyuncleblkcount", c => c.Blocks.GetDailyUncleBlockCountAsync(D1, D2)),
        Daily("dailyavggaslimit", "dailyavggaslimit", c => c.GasTracker.GetDailyAverageGasLimitAsync(D1, D2)),
        Daily("dailygasused", "dailygasused", c => c.GasTracker.GetDailyGasUsedAsync(D1, D2)),
        Daily("dailyavggasprice", "dailyavggasprice", c => c.GasTracker.GetDailyAverageGasPriceAsync(D1, D2)),
        Daily("nodecounthistory", "nodecounthistory", c => c.Stats.GetNodeCountHistoryAsync(D1, D2)),
        Daily("dailytxnfee", "dailytxnfee", c => c.Stats.GetDailyTransactionFeesAsync(D1, D2)),
        Daily("dailynewaddress", "dailynewaddress", c => c.Stats.GetDailyNewAddressCountAsync(D1, D2)),
        Daily("dailyensregister", "dailyensregister", c => c.Stats.GetDailyEnsRegistrationCountAsync(D1, D2)),
        Daily("dailynetutilization", "dailynetutilization", c => c.Stats.GetDailyNetworkUtilizationAsync(D1, D2)),
        Daily("dailyavghashrate", "dailyavghashrate", c => c.Stats.GetDailyAverageHashRateAsync(D1, D2)),
        Daily("dailytx", "dailytx", c => c.Stats.GetDailyTransactionCountAsync(D1, D2)),
        Daily("dailyavgnetdifficulty", "dailyavgnetdifficulty", c => c.Stats.GetDailyAverageNetworkDifficultyAsync(D1, D2)),
        Daily("ethdailyprice", "ethdailyprice", c => c.Stats.GetDailyPriceAsync(D1, D2)),
        Daily("chainsize", "chainsize", c => c.Stats.GetChainSizeAsync(D1, D2)),
        Std("ethsupply", "stats", "ethsupply", "\"1\"", c => c.Stats.GetEtherSupplyAsync()),
        Std("ethsupply2", "stats", "ethsupply2", "{}", c => c.Stats.GetEtherSupplyDetailsAsync()),
        Std("ethprice", "stats", "ethprice", "{}", c => c.Stats.GetLastPriceAsync()),
        Std("nodecount", "stats", "nodecount", "{}", c => c.Stats.GetNodeCountAsync()),
        Std("tokensupply", "stats", "tokensupply", "\"1\"", c => c.Tokens.GetTotalSupplyAsync(C)),
        Std("tokensupplyhistory", "stats", "tokensupplyhistory", "\"1\"", c => c.Tokens.GetHistoricalTotalSupplyAsync(C, 1)),

        // contract
        Std("getabi", "contract", "getabi", "\"[]\"", c => c.Contracts.GetAbiAsync(C)),
        Std("getsourcecode", "contract", "getsourcecode", "[]", c => c.Contracts.GetSourceCodeAsync(C)),
        Std("getcontractcreation", "contract", "getcontractcreation", "[]", c => c.Contracts.GetContractCreationAsync([C])),
        Std("checkverifystatus", "contract", "checkverifystatus", "\"Pass - Verified\"", c => c.Contracts.CheckVerificationStatusAsync("guid123")),
        Std("checkproxyverification", "contract", "checkproxyverification", "\"ok\"", c => c.Contracts.CheckProxyVerificationAsync("guid123")),
        Post("verifysourcecode", "verifysourcecode", c => c.Contracts.VerifySolidityAsync(new SolidityVerificationRequest
        {
            ContractAddress = C, SourceCode = "{}", ContractName = "a.sol:A", CompilerVersion = "v0.8.24+commit.e11b9ed9",
        })),
        Post("verifyvyper", "verifysourcecode", c => c.Contracts.VerifyVyperAsync(new VyperVerificationRequest
        {
            ContractAddress = C, SourceCode = "{}", ContractName = "a.vy:A", CompilerVersion = "vyper:0.4.0", OptimizationUsed = false,
        })),
        Post("verifystylus", "verifysourcecode", c => c.Contracts.VerifyStylusAsync(new StylusVerificationRequest
        {
            ContractAddress = C, SourceCode = "https://github.com/o/r", ContractName = "x", CompilerVersion = "stylus:0.5.3",
        })),
        Post("verifyzksyncsourcecode", "verifysourcecode", c => c.Contracts.VerifyZkSyncAsync(new ZkSyncVerificationRequest
        {
            ContractAddress = C, SourceCode = "{}", ContractName = "a.sol:A", CompilerVersion = "v0.8.24+commit.e11b9ed9", ZksolcVersion = "v1.3.13",
        })),
        Post("verifyproxycontract", "verifyproxycontract", c => c.Contracts.VerifyProxyAsync(C)),

        // gastracker
        Std("gasoracle", "gastracker", "gasoracle", "{}", c => c.GasTracker.GetGasOracleAsync()),
        Std("gasestimate", "gastracker", "gasestimate", "\"9\"", c => c.GasTracker.GetConfirmationTimeEstimateAsync(2_000_000_000)),

        // proxy
        Rpc("ethblocknumber", "eth_blockNumber", "\"0x10\"", c => c.Proxy.GetBlockNumberAsync()),
        Rpc("ethgetblockbynumber", "eth_getBlockByNumber", "{}", c => c.Proxy.GetBlockByNumberAsync(BlockTag.Latest)),
        Rpc("ethgetunclebyblocknumberandindex", "eth_getUncleByBlockNumberAndIndex", "null", c => c.Proxy.GetUncleByBlockNumberAndIndexAsync(1, 0)),
        Rpc("ethgetblocktransactioncountbynumber", "eth_getBlockTransactionCountByNumber", "\"0x3\"", c => c.Proxy.GetBlockTransactionCountByNumberAsync(1)),
        Rpc("ethgettransactionbyhash", "eth_getTransactionByHash", "null", c => c.Proxy.GetTransactionByHashAsync(H)),
        Rpc("ethgettransactionbyblocknumberandindex", "eth_getTransactionByBlockNumberAndIndex", "null", c => c.Proxy.GetTransactionByBlockNumberAndIndexAsync(1, 0)),
        Rpc("ethgettransactioncount", "eth_getTransactionCount", "\"0x1\"", c => c.Proxy.GetTransactionCountAsync(A)),
        Rpc("ethsendrawtransaction", "eth_sendRawTransaction", $"\"{H}\"", c => c.Proxy.SendRawTransactionAsync("0xf86c")),
        Rpc("ethgettransactionreceipt", "eth_getTransactionReceipt", "null", c => c.Proxy.GetTransactionReceiptAsync(H)),
        Rpc("ethcall", "eth_call", "\"0x\"", c => c.Proxy.CallAsync(C, "0x18160ddd")),
        Rpc("ethgetcode", "eth_getCode", "\"0x\"", c => c.Proxy.GetCodeAsync(C)),
        Rpc("ethgetstorageat", "eth_getStorageAt", "\"0x0\"", c => c.Proxy.GetStorageAtAsync(C, 0)),
        Rpc("ethgasprice", "eth_gasPrice", "\"0x1\"", c => c.Proxy.GetGasPriceAsync()),
        Rpc("ethestimategas", "eth_estimateGas", "\"0x5208\"", c => c.Proxy.EstimateGasAsync(C)),

        // logs
        Std("getlogs", "logs", "getLogs", "[]", c => c.Logs.GetLogsByAddressAsync(C, 1, 2)),
        Std("getlogs-topics", "logs", "getLogs", "[]", c => c.Logs.GetLogsAsync(new LogQuery { FromBlock = 1, ToBlock = 2, Topic0 = H })),
        Std("getlogs-address-topics", "logs", "getLogs", "[]", c => c.Logs.GetLogsAsync(new LogQuery { FromBlock = 1, ToBlock = 2, Address = C, Topic0 = H })),

        // transaction
        Std("getstatus", "transaction", "getstatus", "{}", c => c.Transactions.GetExecutionStatusAsync(H)),
        Std("gettxreceiptstatus", "transaction", "gettxreceiptstatus", "{}", c => c.Transactions.GetReceiptStatusAsync(H)),

        // ens
        Std("forwardresolve", "ens", "forwardresolve", $"\"{A}\"", c => c.Ens.ResolveAsync("vitalik.eth")),

        // nametag
        Std("getaddresstag", "nametag", "getaddresstag", "[]", c => c.Nametags.GetAddressMetadataAsync(A)),
        Std("getlabelmasterlist-v2", "nametag", "getlabelmasterlist", Url, c => c.Nametags.GetLabelMasterListUrlAsync()),
        Std("exportaddresstags-v2", "nametag", "exportaddresstags", Url, c => c.Nametags.GetAddressTagsExportUrlAsync("phish-hack")),
        new("getlabelmasterlist", "nametag", "getlabelmasterlist", TestClient.Ok("[]"),
            c => c.LegacyMetadata.GetLabelMasterListAsync(), Path: "/v1/api.ashx", Host: "api-metadata.etherscan.io", HasChain: false),
        new("exportaddresstags", "nametag", "exportaddresstags", "csv",
            async c => await (await c.LegacyMetadata.ExportAddressTagsCsvAsync("phish-hack")).DisposeAsync(),
            Path: "/v1/api.ashx", Host: "api-metadata.etherscan.io", HasChain: false),

        // usage and chains
        new("getapilimit", "getapilimit", "getapilimit", TestClient.Ok("{}"), c => c.Usage.GetApiUsageAsync(), HasChain: false),
        new("chainlist", null, null, TestClient.Ok("[]"), c => c.Chains.GetChainListAsync(), Path: "/v2/chainlist", HasChain: false),
    ];

    public static TheoryData<string> CaseIds => new(Cases.Select(c => c.OpenApiId));

    [Fact]
    public void Every_documented_operation_is_mapped_exactly_once()
    {
        var mapped = Cases.Select(c => c.OpenApiId).ToList();
        Assert.Equal(mapped.Count, mapped.Distinct().Count());
        Assert.Equal(DocumentedOperations.Order(), mapped.Order());
        Assert.Equal(97, mapped.Count);
    }

    [Theory]
    [MemberData(nameof(CaseIds))]
    public async Task Operation_sends_documented_route(string id)
    {
        var @case = Cases.Single(c => c.OpenApiId == id);
        var handler = new FakeHttpMessageHandler((request, _) =>
        {
            if (@case.Response == "csv")
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent("\"address\";\"nametag\"", System.Text.Encoding.UTF8, "text/csv"),
                };
            }

            return FakeHttpMessageHandler.JsonResponse(@case.Response);
        });
        using var client = TestClient.Create(handler);

        await @case.Invoke(client);

        var sent = Assert.Single(handler.Requests);
        Assert.Equal(@case.Method, sent.Method.Method);
        Assert.Equal(@case.Host, sent.Uri.Host);
        Assert.Equal(@case.Path, sent.Uri.AbsolutePath);
        Assert.Equal(@case.Module, sent.Query["module"]);
        Assert.Equal(@case.Action, sent.Query["action"]);
        Assert.Equal(@case.HasChain ? "1" : null, sent.Query["chainid"]);

        if (@case.Method == "POST")
        {
            Assert.Null(sent.Query["apikey"]);
            Assert.Equal(TestClient.ApiKey, sent.Form["apikey"]);
        }
        else
        {
            Assert.Equal(TestClient.ApiKey, sent.Query["apikey"]);
        }
    }
}
