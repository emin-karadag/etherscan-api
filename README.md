![EtherscanApi logo](https://raw.githubusercontent.com/emin-karadag/etherscan-api/main/assets/icon.png)

# EtherscanApi

**The complete, strongly-typed .NET client for the [Etherscan API V2](https://docs.etherscan.io/introduction).**
One API key, 29 EVM mainnets, every documented endpoint.

[![NuGet](https://img.shields.io/nuget/vpre/EtherscanApi?logo=nuget&label=NuGet&color=004880)](https://www.nuget.org/packages/EtherscanApi)
[![Downloads](https://img.shields.io/nuget/dt/EtherscanApi?logo=nuget&label=Downloads&color=004880)](https://www.nuget.org/packages/EtherscanApi)
[![Build](https://github.com/emin-karadag/etherscan-api/actions/workflows/ci.yml/badge.svg)](https://github.com/emin-karadag/etherscan-api/actions/workflows/ci.yml)
[![.NET](https://img.shields.io/badge/.NET-11-512BD4?logo=dotnet)](https://dotnet.microsoft.com/download/dotnet/11.0)
[![Native AOT](https://img.shields.io/badge/Native%20AOT-ready-2ea44f)](https://learn.microsoft.com/dotnet/core/deploying/native-aot/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/emin-karadag/etherscan-api/blob/main/LICENSE)

> **Note:** Unofficial community library. It is not affiliated with, endorsed by, or supported by Etherscan.

---

## ✨ Why EtherscanApi?

| | |
|---|---|
| 🌐 **Multichain** | One key covers every supported chain. Pick a `ChainId` per call or set a default. |
| 🧩 **Complete** | Accounts, tokens, contracts and verification, blocks, logs, gas, stats, `eth_*` proxy, L2, ENS, name tags, usage. |
| 🔢 **Typed** | Wei amounts are `BigInteger`, timestamps are `DateTimeOffset`, and hex values are decoded for you. |
| 🛡️ **Resilient** | Built-in rate limiting for your plan, automatic retries with backoff, and typed exceptions for Etherscan errors. |
| 🔁 **Streaming** | `IAsyncEnumerable<T>` pagination fetches pages only as you read them. |
| 🧪 **Testable** | Dependency injection with `IHttpClientFactory`; every module is a mockable interface. |
| ⚡ **Lean** | Native AOT and trimming compatible. The only package dependency is `Microsoft.Extensions.Http`. |

## 📦 Installation

```shell
dotnet add package EtherscanApi --prerelease
```

Get a free API key at [etherscan.io/apidashboard](https://etherscan.io/apidashboard) and keep it in user-secrets or an environment variable, never in source code.

## 🚀 Quick start

```csharp
using EtherscanApi;

using var client = new EtherscanClient(Environment.GetEnvironmentVariable("ETHERSCAN_API_KEY")!);

// Native balance in wei
var wei = await client.Accounts.GetBalanceAsync("0xde0B295669a9FD93d5F28D9Ec85E40f4cb697BAe");

// Same key, another chain
var polygonWei = await client.Accounts.GetBalanceAsync("0xde0B295669a9FD93d5F28D9Ec85E40f4cb697BAe", chain: ChainId.Polygon);

// Latest 10 transactions
var txs = await client.Accounts.GetTransactionsAsync(
    "0xde0B295669a9FD93d5F28D9Ec85E40f4cb697BAe",
    new BlockRangeQuery { Page = 1, Offset = 10, Sort = SortOrder.Descending });

foreach (var tx in txs)
{
    Console.WriteLine($"{tx.TimeStamp:u} {tx.Hash} {tx.Value} wei");
}

// Gas prices (gwei)
var gas = await client.GasTracker.GetGasOracleAsync();
Console.WriteLine($"safe {gas.SafeGasPrice}, fast {gas.FastGasPrice}");
```

## 🧱 ASP.NET Core

```json
// appsettings.json — put the key itself in user-secrets (Etherscan:ApiKey)
{
  "Etherscan": { "Plan": "Free", "DefaultChain": "1" }
}
```

```csharp
builder.Services.AddEtherscanApi(builder.Configuration.GetSection("Etherscan"));

// Inject IEtherscanClient wherever you need it.
public sealed class WalletService(IEtherscanClient etherscan)
{
    public Task<BigInteger> GetBalanceAsync(string address) => etherscan.Accounts.GetBalanceAsync(address);
}
```

A missing API key stops the app at startup with a clear message.

## 📚 Examples

**Contracts, tokens and the proxy**

```csharp
var source = await client.Contracts.GetSourceCodeAsync("0xdAC17F958D2ee523a2206206994597C13D831ec7");
var abi = await client.Contracts.GetAbiAsync("0xdAC17F958D2ee523a2206206994597C13D831ec7");
var supply = await client.Tokens.GetTotalSupplyAsync("0xdAC17F958D2ee523a2206206994597C13D831ec7");
var latestBlock = await client.Proxy.GetBlockNumberAsync();
var receipt = await client.Proxy.GetTransactionReceiptAsync("0x2f1c5c2b44f771e942a8506148e256f94f1a464babc938ae0690c6e34cd79190");
```

**Streaming pages on demand**

```csharp
await foreach (var transfer in client.Accounts.EnumerateErc20TransfersAsync(
                   contractAddress: "0xdAC17F958D2ee523a2206206994597C13D831ec7",
                   query: new BlockRangeQuery { StartBlock = 19_000_000 }))
{
    Console.WriteLine($"{transfer.From} -> {transfer.To}: {transfer.Value}");
}
```

**Event logs**

```csharp
var logs = await client.Logs.GetLogsAsync(new LogQuery
{
    FromBlock = 19_000_000,
    ToBlock = 19_000_100,
    Address = "0xdAC17F958D2ee523a2206206994597C13D831ec7",
    Topic0 = "0xddf252ad1be2c89b69c2b068fc378daa952ba7f163c4a11628f55a4df523b3ef", // Transfer
});
```

**Error handling**

```csharp
try
{
    var holders = await client.Tokens.GetTopHoldersAsync("0xdAC17F958D2ee523a2206206994597C13D831ec7", 10);
}
catch (EtherscanPlanRequiredException)
{
    // PRO endpoint, paid-only chain, or PRO endpoint on a testnet
}
catch (EtherscanApiException ex)
{
    Console.WriteLine($"{ex.Module}/{ex.Action} failed: {ex.ApiResult}");
}
```

Every exception derives from `EtherscanException`, and the API key never appears in exception messages.

## 🌐 Supported networks

| | | | | |
|---|---|---|---|---|
| Ethereum | BNB Smart Chain | Polygon | Base | Arbitrum One |
| OP Mainnet | Avalanche C-Chain | Linea | Gnosis | Celo |
| Mantle | Sonic | Unichain | Berachain | Monad |
| Sei | Taiko | World | Fraxtal | Katana |
| HyperEVM | Plasma | Stable | MegaETH | XDC |
| BitTorrent Chain | Memecore | Robinhood Chain | Arc | |

Their testnets (Sepolia, Hoodi, Amoy and others) are supported too. Some chains need a paid plan; see [Supported Chains](https://docs.etherscan.io/supported-chains), or get the live list in code with `client.Chains.GetChainListAsync()`.

## ⚙️ Configuration

| Option | Default | Description |
|---|---|---|
| `ApiKey` | — | Your Etherscan API key (required) |
| `DefaultChain` | `ChainId.EthereumMainnet` | Chain used when a call does not pass one |
| `Plan` | `Free` | Your plan; sets the client-side rate limit |
| `RequestsPerSecond` | from `Plan` | Custom rate limit |
| `MaxRetryAttempts` | `3` | Retries for rate-limit and timeout errors |
| `Timeout` | `00:01:40` | HTTP timeout |

Some endpoints are **PRO** (Standard plan and above) or **PRO+** (Pro Plus); each method's documentation states its tier. See [Rate Limits](https://docs.etherscan.io/rate-limits).

## 💖 Support the project

If EtherscanApi saves you time, you can support its development with a crypto donation. Thank you! 🙏

| Currency | Networks | Address |
|---|---|---|
| **Bitcoin (BTC)** | Bitcoin | `bc1qv26gt7xks04z94t885rg2jp9phwmqe38rjkpze` |
| **Tether (USDT)** | Ethereum, Plasma, Polygon, Arbitrum | `0x02A0C197B35E9b38EC4DEE84Af736453bC417cd8` |
| **Tether Gold (XAUT)** | Ethereum, Plasma, Polygon, Arbitrum | `0x02A0C197B35E9b38EC4DEE84Af736453bC417cd8` |

> **Warning:** Send only the listed currencies on the listed networks. Funds sent on any other network or as any other token may be lost.

## 🔗 Links

- [Etherscan API documentation](https://docs.etherscan.io)
- [Source code, issues and changelog](https://github.com/emin-karadag/etherscan-api)

## 📄 License

[MIT](https://github.com/emin-karadag/etherscan-api/blob/main/LICENSE) © Mehmet Emin KARADAĞ
