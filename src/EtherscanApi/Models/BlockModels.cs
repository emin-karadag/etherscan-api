using System.Numerics;
using System.Text.Json.Serialization;

namespace EtherscanApi.Models;

/// <summary>Block and uncle rewards for a block (<c>getblockreward</c>).</summary>
public sealed record BlockReward
{
    /// <summary>Block number.</summary>
    [JsonPropertyName("blockNumber")]
    public long BlockNumber { get; init; }

    /// <summary>Block timestamp.</summary>
    [JsonPropertyName("timeStamp")]
    public DateTimeOffset TimeStamp { get; init; }

    /// <summary>Address that produced the block.</summary>
    [JsonPropertyName("blockMiner")]
    public string? BlockMiner { get; init; }

    /// <summary>Block reward, in wei.</summary>
    [JsonPropertyName("blockReward")]
    public BigInteger Reward { get; init; }

    /// <summary>Uncles included in the block.</summary>
    [JsonPropertyName("uncles")]
    public IReadOnlyList<UncleReward> Uncles { get; init; } = [];

    /// <summary>Reward for including uncles, in wei.</summary>
    [JsonPropertyName("uncleInclusionReward")]
    public BigInteger UncleInclusionReward { get; init; }
}

/// <summary>An uncle block reward.</summary>
public sealed record UncleReward
{
    /// <summary>Uncle miner.</summary>
    [JsonPropertyName("miner")]
    public string? Miner { get; init; }

    /// <summary>Uncle position.</summary>
    [JsonPropertyName("unclePosition")]
    public int UnclePosition { get; init; }

    /// <summary>Uncle reward, in wei.</summary>
    [JsonPropertyName("blockreward")]
    public BigInteger Reward { get; init; }
}

/// <summary>Transaction counts in a block (<c>getblocktxnscount</c>).</summary>
public sealed record BlockTransactionCounts
{
    /// <summary>Block number.</summary>
    [JsonPropertyName("block")]
    public long Block { get; init; }

    /// <summary>Normal transactions.</summary>
    [JsonPropertyName("txsCount")]
    public long TransactionCount { get; init; }

    /// <summary>Internal transactions.</summary>
    [JsonPropertyName("internalTxsCount")]
    public long InternalTransactionCount { get; init; }

    /// <summary>ERC-20 transfers.</summary>
    [JsonPropertyName("erc20TxsCount")]
    public long Erc20TransferCount { get; init; }

    /// <summary>ERC-721 transfers.</summary>
    [JsonPropertyName("erc721TxsCount")]
    public long Erc721TransferCount { get; init; }

    /// <summary>ERC-1155 transfers.</summary>
    [JsonPropertyName("erc1155TxsCount")]
    public long Erc1155TransferCount { get; init; }
}

/// <summary>Estimated time until a block is produced (<c>getblockcountdown</c>).</summary>
public sealed record BlockCountdown
{
    /// <summary>Current block.</summary>
    [JsonPropertyName("CurrentBlock")]
    public long CurrentBlock { get; init; }

    /// <summary>Target block.</summary>
    [JsonPropertyName("CountdownBlock")]
    public long CountdownBlock { get; init; }

    /// <summary>Blocks remaining.</summary>
    [JsonPropertyName("RemainingBlock")]
    public long RemainingBlocks { get; init; }

    /// <summary>Estimated seconds remaining.</summary>
    [JsonPropertyName("EstimateTimeInSec")]
    public decimal EstimatedSeconds { get; init; }

    /// <summary>Estimated time remaining.</summary>
    [JsonIgnore]
    public TimeSpan EstimatedTime => TimeSpan.FromSeconds((double)EstimatedSeconds);
}

/// <summary>Base of every daily statistics row: the UTC date and its Unix timestamp.</summary>
public abstract record DailyStatistic
{
    /// <summary>The UTC date.</summary>
    [JsonPropertyName("UTCDate")]
    public DateOnly UtcDate { get; init; }

    /// <summary>Start of the UTC day.</summary>
    [JsonPropertyName("unixTimeStamp")]
    public DateTimeOffset UnixTimeStamp { get; init; }
}

/// <summary>Daily average block size (<c>dailyavgblocksize</c>).</summary>
public sealed record DailyAverageBlockSize : DailyStatistic
{
    /// <summary>Average block size in bytes.</summary>
    [JsonPropertyName("blockSize_bytes")]
    public decimal BlockSizeBytes { get; init; }
}

/// <summary>Daily block count and rewards (<c>dailyblkcount</c>).</summary>
public sealed record DailyBlockCount : DailyStatistic
{
    /// <summary>Blocks produced.</summary>
    [JsonPropertyName("blockCount")]
    public long BlockCount { get; init; }

    /// <summary>Block rewards, in the native token (ETH).</summary>
    [JsonPropertyName("blockRewards_Eth")]
    public decimal BlockRewardsEth { get; init; }
}

/// <summary>Daily block rewards (<c>dailyblockrewards</c>).</summary>
public sealed record DailyBlockRewards : DailyStatistic
{
    /// <summary>Block rewards, in the native token (ETH).</summary>
    [JsonPropertyName("blockRewards_Eth")]
    public decimal BlockRewardsEth { get; init; }
}

/// <summary>Daily average block time (<c>dailyavgblocktime</c>).</summary>
public sealed record DailyAverageBlockTime : DailyStatistic
{
    /// <summary>Average seconds per block.</summary>
    [JsonPropertyName("blockTime_sec")]
    public decimal BlockTimeSeconds { get; init; }
}

/// <summary>Daily uncle block count and rewards (<c>dailyuncleblkcount</c>).</summary>
public sealed record DailyUncleBlockCount : DailyStatistic
{
    /// <summary>Uncle blocks.</summary>
    [JsonPropertyName("uncleBlockCount")]
    public long UncleBlockCount { get; init; }

    /// <summary>Uncle rewards, in the native token (ETH).</summary>
    [JsonPropertyName("uncleBlockRewards_Eth")]
    public decimal UncleBlockRewardsEth { get; init; }
}
