using System.Numerics;
using EtherscanApi.Models;

namespace EtherscanApi;

/// <summary>Block endpoints (<c>module=block</c>) and daily block statistics (<c>module=stats</c>).</summary>
public interface IBlocksModule
{
    /// <summary>Gets block and uncle rewards for a block (<c>getblockreward</c>).</summary>
    /// <param name="blockNumber">The block.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/getblockreward"/>
    Task<BlockReward> GetBlockRewardAsync(long blockNumber, ChainId? chain = null, CancellationToken cancellationToken = default);

    /// <summary>Gets normal, internal and token transfer counts for a block (<c>getblocktxnscount</c>).</summary>
    /// <param name="blockNumber">The block.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/getblocktxnscount"/>
    Task<BlockTransactionCounts> GetBlockTransactionCountsAsync(long blockNumber, ChainId? chain = null, CancellationToken cancellationToken = default);

    /// <summary>Gets the estimated time until a future block is produced (<c>getblockcountdown</c>).</summary>
    /// <param name="blockNumber">A future block.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/getblockcountdown"/>
    Task<BlockCountdown> GetBlockCountdownAsync(long blockNumber, ChainId? chain = null, CancellationToken cancellationToken = default);

    /// <summary>Gets the block number produced closest to a timestamp (<c>getblocknobytime</c>).</summary>
    /// <param name="timestamp">The time.</param>
    /// <param name="closest">Whether to take the closest block before or after the timestamp.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/getblocknobytime"/>
    Task<long> GetBlockNumberByTimeAsync(
        DateTimeOffset timestamp,
        ClosestBlock closest = ClosestBlock.Before,
        ChainId? chain = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the daily average block size (<c>stats/dailyavgblocksize</c>).</summary>
    /// <remarks><b>PRO</b> endpoint (Standard plan and above).</remarks>
    /// <param name="startDate">First day (UTC).</param>
    /// <param name="endDate">Last day (UTC).</param>
    /// <param name="sort">Sort order.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/dailyavgblocksize"/>
    Task<IReadOnlyList<DailyAverageBlockSize>> GetDailyAverageBlockSizeAsync(
        DateOnly startDate, DateOnly endDate, SortOrder? sort = null, ChainId? chain = null, CancellationToken cancellationToken = default);

    /// <summary>Gets the daily block count and rewards (<c>stats/dailyblkcount</c>).</summary>
    /// <remarks><b>PRO</b> endpoint (Standard plan and above).</remarks>
    /// <param name="startDate">First day (UTC).</param>
    /// <param name="endDate">Last day (UTC).</param>
    /// <param name="sort">Sort order.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/dailyblkcount"/>
    Task<IReadOnlyList<DailyBlockCount>> GetDailyBlockCountAsync(
        DateOnly startDate, DateOnly endDate, SortOrder? sort = null, ChainId? chain = null, CancellationToken cancellationToken = default);

    /// <summary>Gets the daily block rewards (<c>stats/dailyblockrewards</c>).</summary>
    /// <remarks><b>PRO</b> endpoint (Standard plan and above).</remarks>
    /// <param name="startDate">First day (UTC).</param>
    /// <param name="endDate">Last day (UTC).</param>
    /// <param name="sort">Sort order.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/dailyblockrewards"/>
    Task<IReadOnlyList<DailyBlockRewards>> GetDailyBlockRewardsAsync(
        DateOnly startDate, DateOnly endDate, SortOrder? sort = null, ChainId? chain = null, CancellationToken cancellationToken = default);

    /// <summary>Gets the daily average block time (<c>stats/dailyavgblocktime</c>).</summary>
    /// <remarks><b>PRO</b> endpoint (Standard plan and above).</remarks>
    /// <param name="startDate">First day (UTC).</param>
    /// <param name="endDate">Last day (UTC).</param>
    /// <param name="sort">Sort order.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/dailyavgblocktime"/>
    Task<IReadOnlyList<DailyAverageBlockTime>> GetDailyAverageBlockTimeAsync(
        DateOnly startDate, DateOnly endDate, SortOrder? sort = null, ChainId? chain = null, CancellationToken cancellationToken = default);

    /// <summary>Gets the daily uncle block count and rewards (<c>stats/dailyuncleblkcount</c>).</summary>
    /// <remarks><b>PRO</b> endpoint (Standard plan and above).</remarks>
    /// <param name="startDate">First day (UTC).</param>
    /// <param name="endDate">Last day (UTC).</param>
    /// <param name="sort">Sort order.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/dailyuncleblkcount"/>
    Task<IReadOnlyList<DailyUncleBlockCount>> GetDailyUncleBlockCountAsync(
        DateOnly startDate, DateOnly endDate, SortOrder? sort = null, ChainId? chain = null, CancellationToken cancellationToken = default);
}
