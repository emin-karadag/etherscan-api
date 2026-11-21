// EtherscanApi sample: run with ETHERSCAN_API_KEY set.
//   $env:ETHERSCAN_API_KEY = "<your key>"; dotnet run --project samples/EtherscanApi.Samples.Console
using System.Numerics;
using EtherscanApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;

var apiKey = Environment.GetEnvironmentVariable("ETHERSCAN_API_KEY");
if (string.IsNullOrWhiteSpace(apiKey))
{
    Console.Error.WriteLine("Set the ETHERSCAN_API_KEY environment variable first.");
    return 1;
}

const string address = "0xde0B295669a9FD93d5F28D9Ec85E40f4cb697BAe";
const string usdt = "0xdAC17F958D2ee523a2206206994597C13D831ec7";

// 1) Without dependency injection.
using (var client = new EtherscanClient(new EtherscanClientOptions { ApiKey = apiKey, Plan = EtherscanPlan.Free }))
{
    var wei = await client.Accounts.GetBalanceAsync(address);
    Console.WriteLine($"Ethereum balance: {FormatEther(wei)} ETH");

    // Same key, another chain.
    var polygonWei = await client.Accounts.GetBalanceAsync(address, chain: ChainId.Polygon);
    Console.WriteLine($"Polygon balance:  {FormatEther(polygonWei)} POL");

    var latest = await client.Accounts.GetTransactionsAsync(address, new BlockRangeQuery { Page = 1, Offset = 3, Sort = SortOrder.Descending });
    foreach (var tx in latest)
    {
        Console.WriteLine($"  {tx.TimeStamp:u} {tx.Hash} {FormatEther(tx.Value)} ETH");
    }

    var gas = await client.GasTracker.GetGasOracleAsync();
    Console.WriteLine($"Gas (gwei): safe {gas.SafeGasPrice}, propose {gas.ProposeGasPrice}, fast {gas.FastGasPrice}");

    var source = await client.Contracts.GetSourceCodeAsync(usdt);
    Console.WriteLine($"USDT verified: {source.IsVerified} ({source.ContractName}, {source.CompilerVersion})");

    // Stream ERC-20 transfers page by page.
    var count = 0;
    await foreach (var transfer in client.Accounts.EnumerateErc20TransfersAsync(contractAddress: usdt, query: new BlockRangeQuery { Sort = SortOrder.Descending }, pageSize: 100))
    {
        if (++count > 5)
        {
            break;
        }

        Console.WriteLine($"  USDT {transfer.From} -> {transfer.To}: {transfer.Value}");
    }

    try
    {
        await client.Tokens.GetTopHoldersAsync(usdt, 5);
    }
    catch (EtherscanPlanRequiredException ex)
    {
        Console.WriteLine($"PRO endpoint on a Free key: {ex.ApiResult}");
    }
}

// 2) With dependency injection.
var services = new ServiceCollection();
services.AddEtherscanApi(o =>
    {
        o.ApiKey = apiKey;
        o.DefaultChain = ChainId.EthereumMainnet;
    })
    // Optional transport resilience. Never retry unsafe methods: verification submissions are POSTs.
    .AddStandardResilienceHandler(o => o.Retry.DisableForUnsafeHttpMethods());

await using var provider = services.BuildServiceProvider();
var etherscan = provider.GetRequiredService<IEtherscanClient>();
var block = await etherscan.Proxy.GetBlockNumberAsync();
Console.WriteLine($"Latest block via DI: {block}");

return 0;

static string FormatEther(BigInteger wei)
{
    var whole = BigInteger.DivRem(wei, BigInteger.Pow(10, 18), out var fraction);
    return $"{whole}.{BigInteger.Abs(fraction).ToString("D18", System.Globalization.CultureInfo.InvariantCulture)[..6]}";
}
