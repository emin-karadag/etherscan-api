using System.Numerics;

namespace EtherscanApi.Tests;

/// <summary>Parses the example responses published in the Etherscan docs.</summary>
public sealed class ResponseParsingTests
{
    private const string Address = "0xde0B295669a9FD93d5F28D9Ec85E40f4cb697BAe";

    [Fact]
    public async Task Txlist_example_is_parsed_with_typed_fields()
    {
        var json = """
        {"status":"1","message":"OK","result":[{"blockNumber":"23467053","blockHash":"0xf5646226f819fbdd6b2f1cb99de6e5d17da3d876ec166e69f4e9736c8ebf7ab4","timeStamp":"1759129619","hash":"0xf9db905d77704596d3600816bc70201586cfeec13bcf576320e2f38d6ca851a0","nonce":"8","transactionIndex":"184","from":"0x2449ecef5012f0a0e153b278ef4fcc9625bc4c78","to":"0xc5102fe9359fd9a28f877a67e36b0f050d81a3cc","value":"0","gas":"73271","gasPrice":"238744402","input":"0x5c19a95c","methodId":"0x5c19a95c","functionName":"delegate(address to)","contractAddress":"","cumulativeGasUsed":"22498564","txreceipt_status":"1","gasUsed":"48847","confirmations":"589","isError":"0"}]}
        """;
        using var client = TestClient.Create(FakeHttpMessageHandler.Json(json));

        var tx = Assert.Single(await client.Accounts.GetTransactionsAsync(Address));

        Assert.Equal(23467053, tx.BlockNumber);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1759129619), tx.TimeStamp);
        Assert.Equal(new BigInteger(238744402), tx.GasPrice);
        Assert.True(tx.ReceiptStatus);
        Assert.False(tx.IsError);
        Assert.Equal("delegate(address to)", tx.FunctionName);
    }

    [Fact]
    public async Task No_transactions_found_returns_empty_list()
    {
        using var client = TestClient.Create(FakeHttpMessageHandler.Json("""{"status":"0","message":"No transactions found","result":[]}"""));

        var result = await client.Accounts.GetTransactionsAsync(Address);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Getlogs_hex_quantities_are_decoded()
    {
        var json = """
        {"status":"1","message":"OK","result":[{"address":"0xbd3531da5cf5857e7cfaa92426877b022e612cf8","topics":["0xddf252ad1be2c89b69c2b068fc378daa952ba7f163c4a11628f55a4df523b3ef"],"data":"0x","blockNumber":"0xc48174","blockHash":"0x837e109ab8b1b40ec7d1032bff82397325d85e719b97d900fa0d9aa9745b2c27","timeStamp":"0x60f9ce56","gasPrice":"0x2e90edd000","gasUsed":"0x247205","logIndex":"0x","transactionHash":"0x4ffd22d986913d33927a392fe4319bcd2b62f3afe1c15a2c59f77fc2cc4c20a9","transactionIndex":"0x"}]}
        """;
        using var client = TestClient.Create(FakeHttpMessageHandler.Json(json));

        var log = Assert.Single(await client.Logs.GetLogsByAddressAsync("0xbd3531da5cf5857e7cfaa92426877b022e612cf8", 1, 2));

        Assert.Equal(0xc48174, log.BlockNumber);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(0x60f9ce56), log.TimeStamp);
        Assert.Equal(BigInteger.Parse("200000000000", System.Globalization.CultureInfo.InvariantCulture), log.GasPrice);
        Assert.Equal(0, log.LogIndex);
    }

    [Fact]
    public async Task Fundedby_numeric_block_field_is_read()
    {
        var json = """{"status":"1","message":"OK","result":{"block":53708500,"timeStamp":"1708349932","fundingAddress":"0x6969174fd72466430a46e18234d0b530c9fd5f49","fundingTxn":"0xbc0ca4a67eb1555920552246409626cd60df01314dd2bcdb99718b506d9c9946","value":"1000000000000000"}}""";
        using var client = TestClient.Create(FakeHttpMessageHandler.Json(json));

        var funding = await client.Accounts.GetFundedByAsync(Address);

        Assert.Equal(53708500, funding.Block);
        Assert.Equal(new BigInteger(1_000_000_000_000_000), funding.Value);
    }

    [Fact]
    public async Task Daily_difficulty_with_thousands_separator_is_read()
    {
        var json = """{"status":"1","message":"OK","result":[{"UTCDate":"2019-02-01","unixTimeStamp":"1548979200","networkDifficulty":"2,408.028"}]}""";
        using var client = TestClient.Create(FakeHttpMessageHandler.Json(json));

        var row = Assert.Single(await client.Stats.GetDailyAverageNetworkDifficultyAsync(new(2019, 2, 1), new(2019, 2, 1)));

        Assert.Equal(new DateOnly(2019, 2, 1), row.UtcDate);
        Assert.Equal(2408.028m, row.NetworkDifficulty);
    }

    [Fact]
    public async Task Ethsupply2_large_wei_values_fit_biginteger()
    {
        var json = """{"status":"1","message":"OK","result":{"EthSupply":"122373866217800000000000000","Eth2Staking":"2940327167090033000000000","BurntFees":"4610942856337430575173814","WithdrawnTotal":"7618584348954597000000000"}}""";
        using var client = TestClient.Create(FakeHttpMessageHandler.Json(json));

        var supply = await client.Stats.GetEtherSupplyDetailsAsync();

        Assert.Equal(BigInteger.Parse("122373866217800000000000000", System.Globalization.CultureInfo.InvariantCulture), supply.EthSupply);
    }

    [Fact]
    public async Task Gas_oracle_and_ratios_are_parsed()
    {
        var json = """{"status":"1","message":"OK","result":{"LastBlock":"23467872","SafeGasPrice":"0.496839934","ProposeGasPrice":"0.496840168","FastGasPrice":"0.55411917","suggestBaseFee":"0.496839934","gasUsedRatio":"0.405942555555556,0.784013777777778"}}""";
        using var client = TestClient.Create(FakeHttpMessageHandler.Json(json));

        var oracle = await client.GasTracker.GetGasOracleAsync();

        Assert.Equal(23467872, oracle.LastBlock);
        Assert.Equal(0.55411917m, oracle.FastGasPrice);
        Assert.Equal([0.405942555555556m, 0.784013777777778m], oracle.GetGasUsedRatios());
    }

    [Fact]
    public async Task Block_reward_uncles_are_parsed()
    {
        var json = """{"status":"1","message":"OK","result":{"blockNumber":"2165403","timeStamp":"1472533979","blockMiner":"0x13a06d3dfe21e0db5c016c03ea7d2509f7f8d1e3","blockReward":"5314181600000000000","uncles":[{"miner":"0xbcdfc35b86bedf72f0cda046a3c16829a2ef41d1","unclePosition":"0","blockreward":"3750000000000000000"}],"uncleInclusionReward":"312500000000000000"}}""";
        using var client = TestClient.Create(FakeHttpMessageHandler.Json(json));

        var reward = await client.Blocks.GetBlockRewardAsync(2165403);

        var uncle = Assert.Single(reward.Uncles);
        Assert.Equal(new BigInteger(3_750_000_000_000_000_000), uncle.Reward);
    }

    [Fact]
    public async Task Address_metadata_example_is_parsed()
    {
        var json = """{"status":"1","message":"OK","result":[{"address":"0xa9d1e08c7793af67e9d92fe308d5697fb81d3e43","nametag":"Coinbase 10","internal_nametag":"","url":"https://coinbase.com","shortdescription":"","notes_1":"","notes_2":"","labels":["Coinbase","Exchange"],"labels_slug":["coinbase","exchange"],"reputation":0,"other_attributes":[],"lastupdatedtimestamp":1721899658}]}""";
        using var client = TestClient.Create(FakeHttpMessageHandler.Json(json));

        var metadata = await client.Nametags.GetAddressMetadataAsync("0xa9d1e08c7793af67e9d92fe308d5697fb81d3e43");

        Assert.NotNull(metadata);
        Assert.Equal("Coinbase 10", metadata.Nametag);
        Assert.Equal(["coinbase", "exchange"], metadata.LabelSlugs);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1721899658), metadata.LastUpdated);
    }

    [Fact]
    public async Task Api_usage_is_parsed()
    {
        var json = """{"status":"1","message":"OK","result":{"creditsUsed":22,"creditsAvailable":1499978,"creditLimit":1500000,"limitInterval":"daily","intervalExpiryTimespan":"08:42:34"}}""";
        using var client = TestClient.Create(FakeHttpMessageHandler.Json(json));

        var usage = await client.Usage.GetApiUsageAsync();

        Assert.Equal(1_500_000, usage.CreditLimit);
        Assert.Equal(new TimeSpan(8, 42, 34), usage.IntervalExpiresIn);
    }

    [Fact]
    public async Task Chain_list_is_parsed()
    {
        var json = """{"comments":"x","totalcount":1,"status":"1","message":"OK","result":[{"chainid":"8453","chainname":"Base Mainnet","blockexplorer":"https://basescan.org/","apiurl":"https://api.etherscan.io/v2/api?chainid=8453","status":1,"comment":"Paid Plan required"}]}""";
        using var client = TestClient.Create(FakeHttpMessageHandler.Json(json));

        var chain = Assert.Single(await client.Chains.GetChainListAsync());

        Assert.Equal(ChainId.Base, chain.ChainId);
        Assert.True(chain.IsSupported);
    }

    [Fact]
    public async Task Proxy_receipt_is_decoded()
    {
        var json = """{"jsonrpc":"2.0","id":1,"result":{"blockHash":"0x1","blockNumber":"0xc8","contractAddress":null,"cumulativeGasUsed":"0x5208","effectiveGasPrice":"0x3b9aca00","from":"0xa","gasUsed":"0x5208","logs":[{"address":"0xb","topics":["0xddf252ad"],"data":"0x","blockNumber":"0xc8","logIndex":"0x0","removed":false}],"logsBloom":"0x0","status":"0x1","to":"0xc","transactionHash":"0x2","transactionIndex":"0x0","type":"0x2"}}""";
        using var client = TestClient.Create(FakeHttpMessageHandler.Json(json));

        var receipt = await client.Proxy.GetTransactionReceiptAsync("0x2f1c5c2b44f771e942a8506148e256f94f1a464babc938ae0690c6e34cd79190");

        Assert.NotNull(receipt);
        Assert.True(receipt.Status);
        Assert.Equal(200, receipt.BlockNumber);
        Assert.Equal(new BigInteger(1_000_000_000), receipt.EffectiveGasPrice);
        Assert.Equal(2, receipt.Type);
        Assert.Single(receipt.Logs);
    }

    [Fact]
    public async Task Proxy_block_with_transactions_is_decoded()
    {
        var json = """{"jsonrpc":"2.0","id":1,"result":{"number":"0x10","hash":"0xabc","timestamp":"0x60f9ce56","baseFeePerGas":"0x7","gasLimit":"0x1c9c380","gasUsed":"0x0","difficulty":"0x0","size":"0x220","uncles":[],"transactions":[{"hash":"0xt","nonce":"0x1","value":"0xde0b6b3a7640000","gas":"0x5208","type":"0x2","blockNumber":"0x10","accessList":[]}]}}""";
        using var client = TestClient.Create(FakeHttpMessageHandler.Json(json));

        var block = await client.Proxy.GetBlockWithTransactionsByNumberAsync(16);

        Assert.NotNull(block);
        Assert.Equal(16, block.Number);
        var tx = Assert.Single(block.Transactions);
        Assert.Equal(BigInteger.Pow(10, 18), tx.Value);
    }

    [Fact]
    public async Task Unknown_transaction_returns_null()
    {
        using var client = TestClient.Create(FakeHttpMessageHandler.Json(TestClient.Rpc("null")));

        var tx = await client.Proxy.GetTransactionByHashAsync("0x2f1c5c2b44f771e942a8506148e256f94f1a464babc938ae0690c6e34cd79190");

        Assert.Null(tx);
    }

    [Fact]
    public async Task Unverified_source_code_reports_not_verified()
    {
        var json = """{"status":"1","message":"OK","result":[{"SourceCode":"","ABI":"Contract source code not verified","ContractName":"","CompilerVersion":"","OptimizationUsed":"","Runs":"","ConstructorArguments":"","EVMVersion":"Default","Library":"","LicenseType":"Unknown","Proxy":"0","Implementation":"","SwarmSource":""}]}""";
        using var client = TestClient.Create(FakeHttpMessageHandler.Json(json));

        var source = await client.Contracts.GetSourceCodeAsync("0xdAC17F958D2ee523a2206206994597C13D831ec7");

        Assert.False(source.IsVerified);
        Assert.Null(source.Runs);
        Assert.False(source.IsProxy);
    }

    [Fact]
    public async Task Verification_status_maps_pending_success_and_failure()
    {
        using var client = TestClient.Create(FakeHttpMessageHandler.Sequence(
            TestClient.NotOk("Pending in queue"),
            TestClient.Ok("\"Pass - Verified\""),
            TestClient.NotOk("Fail - Unable to verify")));

        Assert.True((await client.Contracts.CheckVerificationStatusAsync("g")).IsPending);
        Assert.True((await client.Contracts.CheckVerificationStatusAsync("g")).IsSuccess);
        var failure = await client.Contracts.CheckVerificationStatusAsync("g");
        Assert.True(failure.IsFailure);
        Assert.Equal("Fail - Unable to verify", failure.Result);
    }
}
