using System.Globalization;
using System.Numerics;
using System.Text.Json.Serialization;

namespace EtherscanApi.Models;

/// <summary>Current gas price recommendations (<c>gasoracle</c>). Prices are in gwei.</summary>
public sealed record GasOracle
{
    /// <summary>Latest block.</summary>
    [JsonPropertyName("LastBlock")]
    public long LastBlock { get; init; }

    /// <summary>Safe (slow) gas price, gwei.</summary>
    [JsonPropertyName("SafeGasPrice")]
    public decimal SafeGasPrice { get; init; }

    /// <summary>Proposed (standard) gas price, gwei.</summary>
    [JsonPropertyName("ProposeGasPrice")]
    public decimal ProposeGasPrice { get; init; }

    /// <summary>Fast gas price, gwei.</summary>
    [JsonPropertyName("FastGasPrice")]
    public decimal FastGasPrice { get; init; }

    /// <summary>Suggested base fee of the next block, gwei.</summary>
    [JsonPropertyName("suggestBaseFee")]
    public decimal SuggestBaseFee { get; init; }

    /// <summary>Gas used ratios of recent blocks, comma-separated as returned.</summary>
    [JsonPropertyName("gasUsedRatio")]
    public string? GasUsedRatio { get; init; }

    /// <summary>Parses <see cref="GasUsedRatio"/> into numbers.</summary>
    public IReadOnlyList<decimal> GetGasUsedRatios()
        => string.IsNullOrWhiteSpace(GasUsedRatio)
            ? []
            : GasUsedRatio.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => decimal.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var d) ? d : 0m)
                .ToArray();
}

/// <summary>Daily average gas limit (<c>dailyavggaslimit</c>).</summary>
public sealed record DailyAverageGasLimit : DailyStatistic
{
    /// <summary>Average gas limit.</summary>
    [JsonPropertyName("gasLimit")]
    public BigInteger GasLimit { get; init; }
}

/// <summary>Daily total gas used (<c>dailygasused</c>).</summary>
public sealed record DailyGasUsed : DailyStatistic
{
    /// <summary>Total gas used.</summary>
    [JsonPropertyName("gasUsed")]
    public BigInteger GasUsed { get; init; }
}

/// <summary>Daily gas price statistics (<c>dailyavggasprice</c>). Values are in wei.</summary>
public sealed record DailyAverageGasPrice : DailyStatistic
{
    /// <summary>Maximum gas price, wei.</summary>
    [JsonPropertyName("maxGasPrice_Wei")]
    public BigInteger MaxGasPriceWei { get; init; }

    /// <summary>Minimum gas price, wei.</summary>
    [JsonPropertyName("minGasPrice_Wei")]
    public BigInteger MinGasPriceWei { get; init; }

    /// <summary>Average gas price, wei.</summary>
    [JsonPropertyName("avgGasPrice_Wei")]
    public BigInteger AverageGasPriceWei { get; init; }
}

/// <summary>Ether supply breakdown (<c>ethsupply2</c>). Values are in wei.</summary>
public sealed record EtherSupplyDetails
{
    /// <summary>Circulating supply, wei.</summary>
    [JsonPropertyName("EthSupply")]
    public BigInteger EthSupply { get; init; }

    /// <summary>ETH2 staking rewards, wei.</summary>
    [JsonPropertyName("Eth2Staking")]
    public BigInteger Eth2Staking { get; init; }

    /// <summary>EIP-1559 burned fees, wei.</summary>
    [JsonPropertyName("BurntFees")]
    public BigInteger BurntFees { get; init; }

    /// <summary>Total withdrawn from the beacon chain, wei.</summary>
    [JsonPropertyName("WithdrawnTotal")]
    public BigInteger WithdrawnTotal { get; init; }
}

/// <summary>Latest price of the native token (<c>ethprice</c>).</summary>
public sealed record NativeTokenPrice
{
    /// <summary>Price in BTC.</summary>
    [JsonPropertyName("ethbtc")]
    public decimal BtcPrice { get; init; }

    /// <summary>Time of the BTC price.</summary>
    [JsonPropertyName("ethbtc_timestamp")]
    public DateTimeOffset BtcPriceTimestamp { get; init; }

    /// <summary>Price in USD.</summary>
    [JsonPropertyName("ethusd")]
    public decimal UsdPrice { get; init; }

    /// <summary>Time of the USD price.</summary>
    [JsonPropertyName("ethusd_timestamp")]
    public DateTimeOffset UsdPriceTimestamp { get; init; }
}

/// <summary>Blockchain size on a day (<c>chainsize</c>).</summary>
public sealed record ChainSizeEntry
{
    /// <summary>Block number.</summary>
    [JsonPropertyName("blockNumber")]
    public long BlockNumber { get; init; }

    /// <summary>The date.</summary>
    [JsonPropertyName("chainTimeStamp")]
    public DateOnly ChainTimeStamp { get; init; }

    /// <summary>Chain size in bytes.</summary>
    [JsonPropertyName("chainSize")]
    public BigInteger ChainSizeBytes { get; init; }

    /// <summary>Node client.</summary>
    [JsonPropertyName("clientType")]
    public string? ClientType { get; init; }

    /// <summary>Sync mode.</summary>
    [JsonPropertyName("syncMode")]
    public string? SyncMode { get; init; }
}

/// <summary>Discoverable node count (<c>nodecount</c>, <c>nodecounthistory</c>).</summary>
public sealed record NodeCount
{
    /// <summary>The UTC date.</summary>
    [JsonPropertyName("UTCDate")]
    public DateOnly UtcDate { get; init; }

    /// <summary>Total discoverable nodes.</summary>
    [JsonPropertyName("TotalNodeCount")]
    public long TotalNodeCount { get; init; }
}

/// <summary>Daily network transaction fees (<c>dailytxnfee</c>).</summary>
public sealed record DailyTransactionFee : DailyStatistic
{
    /// <summary>Fees paid, in the native token (ETH).</summary>
    [JsonPropertyName("transactionFee_Eth")]
    public decimal TransactionFeeEth { get; init; }
}

/// <summary>Daily new address count (<c>dailynewaddress</c>).</summary>
public sealed record DailyNewAddressCount : DailyStatistic
{
    /// <summary>New addresses.</summary>
    [JsonPropertyName("newAddressCount")]
    public long NewAddressCount { get; init; }
}

/// <summary>Daily ENS registrations (<c>dailyensregister</c>).</summary>
public sealed record DailyEnsRegistrationCount : DailyStatistic
{
    /// <summary>ENS registrations.</summary>
    [JsonPropertyName("ensRegistrationCount")]
    public long EnsRegistrationCount { get; init; }
}

/// <summary>Daily network utilization (<c>dailynetutilization</c>).</summary>
public sealed record DailyNetworkUtilization : DailyStatistic
{
    /// <summary>Average gas used relative to the gas limit (0–1).</summary>
    [JsonPropertyName("networkUtilization")]
    public decimal NetworkUtilization { get; init; }
}

/// <summary>Daily average network hash rate (<c>dailyavghashrate</c>).</summary>
public sealed record DailyAverageHashRate : DailyStatistic
{
    /// <summary>Hash rate, GH/s as reported by Etherscan.</summary>
    [JsonPropertyName("networkHashRate")]
    public decimal NetworkHashRate { get; init; }
}

/// <summary>Daily transaction count (<c>dailytx</c>).</summary>
public sealed record DailyTransactionCount : DailyStatistic
{
    /// <summary>Transactions.</summary>
    [JsonPropertyName("transactionCount")]
    public long TransactionCount { get; init; }
}

/// <summary>Daily average network difficulty (<c>dailyavgnetdifficulty</c>).</summary>
public sealed record DailyAverageNetworkDifficulty : DailyStatistic
{
    /// <summary>Difficulty, TH as reported by Etherscan.</summary>
    [JsonPropertyName("networkDifficulty")]
    public decimal NetworkDifficulty { get; init; }
}

/// <summary>Daily native token price (<c>ethdailyprice</c>).</summary>
public sealed record DailyNativeTokenPrice : DailyStatistic
{
    /// <summary>Price in USD.</summary>
    [JsonPropertyName("value")]
    public decimal UsdPrice { get; init; }
}

/// <summary>Contract execution status of a transaction (<c>getstatus</c>).</summary>
public sealed record ContractExecutionStatus
{
    /// <summary>Whether execution failed.</summary>
    [JsonPropertyName("isError")]
    public bool IsError { get; init; }

    /// <summary>Failure reason, such as <c>Bad jump destination</c>.</summary>
    [JsonPropertyName("errDescription")]
    public string? ErrorDescription { get; init; }
}

/// <summary>Receipt status of a transaction (<c>gettxreceiptstatus</c>).</summary>
public sealed record TransactionReceiptStatus
{
    /// <summary><see langword="true"/> for success, <see langword="false"/> for failure, <see langword="null"/> before Byzantium.</summary>
    [JsonPropertyName("status")]
    public bool? IsSuccess { get; init; }
}

/// <summary>API credit usage for the current interval (<c>getapilimit</c>).</summary>
public sealed record ApiUsage
{
    /// <summary>Credits used.</summary>
    [JsonPropertyName("creditsUsed")]
    public long CreditsUsed { get; init; }

    /// <summary>Credits remaining.</summary>
    [JsonPropertyName("creditsAvailable")]
    public long CreditsAvailable { get; init; }

    /// <summary>Credits allowed per interval.</summary>
    [JsonPropertyName("creditLimit")]
    public long CreditLimit { get; init; }

    /// <summary>Interval length, such as <c>daily</c>.</summary>
    [JsonPropertyName("limitInterval")]
    public string? LimitInterval { get; init; }

    /// <summary>Time remaining until reset, as <c>HH:MM:SS</c>.</summary>
    [JsonPropertyName("intervalExpiryTimespan")]
    public string? IntervalExpiryTimespan { get; init; }

    /// <summary>Parses <see cref="IntervalExpiryTimespan"/>.</summary>
    [JsonIgnore]
    public TimeSpan? IntervalExpiresIn
    {
        get
        {
            if (string.IsNullOrWhiteSpace(IntervalExpiryTimespan))
            {
                return null;
            }

            var parts = IntervalExpiryTimespan.Split(':');
            return parts.Length == 3
                   && int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out var h)
                   && int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var m)
                   && int.TryParse(parts[2], NumberStyles.None, CultureInfo.InvariantCulture, out var s)
                ? new TimeSpan(h, m, s)
                : null;
        }
    }
}

/// <summary>A chain in the Etherscan chain list (<c>/v2/chainlist</c>).</summary>
public sealed record ChainInfo
{
    /// <summary>Chain name.</summary>
    [JsonPropertyName("chainname")]
    public string ChainName { get; init; } = "";

    /// <summary>Chain id.</summary>
    [JsonPropertyName("chainid")]
    public ChainId ChainId { get; init; }

    /// <summary>Block explorer URL.</summary>
    [JsonPropertyName("blockexplorer")]
    public string? BlockExplorer { get; init; }

    /// <summary>API URL for the chain.</summary>
    [JsonPropertyName("apiurl")]
    public string? ApiUrl { get; init; }

    /// <summary>1 if the chain is currently supported.</summary>
    [JsonPropertyName("status")]
    public int Status { get; init; }

    /// <summary>Notes, such as <c>Free-tier available</c> or <c>Paid Plan required</c>.</summary>
    [JsonPropertyName("comment")]
    public string? Comment { get; init; }

    /// <summary>Whether the chain is currently supported.</summary>
    [JsonIgnore]
    public bool IsSupported => Status == 1;
}
