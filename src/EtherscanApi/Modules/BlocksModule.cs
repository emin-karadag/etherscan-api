using EtherscanApi.Internal;
using EtherscanApi.Models;

namespace EtherscanApi;

/// <summary>Block endpoints (<c>module=block</c>) and daily block statistics (<c>module=stats</c>).</summary>
internal sealed class BlocksModule : EtherscanModule, IBlocksModule
{
    internal BlocksModule(EtherscanTransport transport)
        : base(transport)
    {
    }

    /// <inheritdoc />
    public Task<BlockReward> GetBlockRewardAsync(long blockNumber, ChainId? chain = null, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(blockNumber);
        var request = Request("block", "getblockreward", chain).Add("blockno", blockNumber);
        return Transport.SendAsync(request, Json.BlockReward, cancellationToken);
    }

    /// <inheritdoc />
    public Task<BlockTransactionCounts> GetBlockTransactionCountsAsync(long blockNumber, ChainId? chain = null, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(blockNumber);
        var request = Request("block", "getblocktxnscount", chain).Add("blockno", blockNumber);
        return Transport.SendAsync(request, Json.BlockTransactionCounts, cancellationToken);
    }

    /// <inheritdoc />
    public Task<BlockCountdown> GetBlockCountdownAsync(long blockNumber, ChainId? chain = null, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(blockNumber);
        var request = Request("block", "getblockcountdown", chain).Add("blockno", blockNumber);
        return Transport.SendAsync(request, Json.BlockCountdown, cancellationToken);
    }

    /// <inheritdoc />
    public Task<long> GetBlockNumberByTimeAsync(
        DateTimeOffset timestamp,
        ClosestBlock closest = ClosestBlock.Before,
        ChainId? chain = null,
        CancellationToken cancellationToken = default)
    {
        var request = Request("block", "getblocknobytime", chain)
            .Add("timestamp", timestamp.ToUnixTimeSeconds())
            .Add("closest", WireFormat.Closest(closest));
        return Transport.SendAsync(request, Json.Int64, cancellationToken);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<DailyAverageBlockSize>> GetDailyAverageBlockSizeAsync(
        DateOnly startDate, DateOnly endDate, SortOrder? sort = null, ChainId? chain = null, CancellationToken cancellationToken = default)
        => Transport.SendListAsync(DailyRequest("dailyavgblocksize", startDate, endDate, sort, chain), Json.ListDailyAverageBlockSize, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<DailyBlockCount>> GetDailyBlockCountAsync(
        DateOnly startDate, DateOnly endDate, SortOrder? sort = null, ChainId? chain = null, CancellationToken cancellationToken = default)
        => Transport.SendListAsync(DailyRequest("dailyblkcount", startDate, endDate, sort, chain), Json.ListDailyBlockCount, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<DailyBlockRewards>> GetDailyBlockRewardsAsync(
        DateOnly startDate, DateOnly endDate, SortOrder? sort = null, ChainId? chain = null, CancellationToken cancellationToken = default)
        => Transport.SendListAsync(DailyRequest("dailyblockrewards", startDate, endDate, sort, chain), Json.ListDailyBlockRewards, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<DailyAverageBlockTime>> GetDailyAverageBlockTimeAsync(
        DateOnly startDate, DateOnly endDate, SortOrder? sort = null, ChainId? chain = null, CancellationToken cancellationToken = default)
        => Transport.SendListAsync(DailyRequest("dailyavgblocktime", startDate, endDate, sort, chain), Json.ListDailyAverageBlockTime, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<DailyUncleBlockCount>> GetDailyUncleBlockCountAsync(
        DateOnly startDate, DateOnly endDate, SortOrder? sort = null, ChainId? chain = null, CancellationToken cancellationToken = default)
        => Transport.SendListAsync(DailyRequest("dailyuncleblkcount", startDate, endDate, sort, chain), Json.ListDailyUncleBlockCount, cancellationToken);
}
