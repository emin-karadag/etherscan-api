using System.Numerics;
using EtherscanApi.Models;

namespace EtherscanApi;

/// <summary>Gas tracker endpoints (<c>module=gastracker</c>) and daily gas statistics (<c>module=stats</c>).</summary>
public interface IGasTrackerModule
{
    /// <summary>Gets the current safe, proposed and fast gas prices (<c>gasoracle</c>).</summary>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/gasoracle"/>
    Task<GasOracle> GetGasOracleAsync(ChainId? chain = null, CancellationToken cancellationToken = default);

    /// <summary>Estimates the confirmation time, in seconds, for a gas price (<c>gasestimate</c>).</summary>
    /// <param name="gasPriceWei">Gas price paid per unit of gas, in wei.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/gasestimate"/>
    Task<TimeSpan> GetConfirmationTimeEstimateAsync(
        BigInteger gasPriceWei,
        ChainId? chain = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the daily average gas limit (<c>stats/dailyavggaslimit</c>).</summary>
    /// <remarks><b>PRO</b> endpoint (Standard plan and above).</remarks>
    /// <param name="startDate">First day (UTC).</param>
    /// <param name="endDate">Last day (UTC).</param>
    /// <param name="sort">Sort order.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/dailyavggaslimit"/>
    Task<IReadOnlyList<DailyAverageGasLimit>> GetDailyAverageGasLimitAsync(
        DateOnly startDate, DateOnly endDate, SortOrder? sort = null, ChainId? chain = null, CancellationToken cancellationToken = default);

    /// <summary>Gets the total gas used per day (<c>stats/dailygasused</c>).</summary>
    /// <remarks><b>PRO</b> endpoint (Standard plan and above).</remarks>
    /// <param name="startDate">First day (UTC).</param>
    /// <param name="endDate">Last day (UTC).</param>
    /// <param name="sort">Sort order.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/dailygasused"/>
    Task<IReadOnlyList<DailyGasUsed>> GetDailyGasUsedAsync(
        DateOnly startDate, DateOnly endDate, SortOrder? sort = null, ChainId? chain = null, CancellationToken cancellationToken = default);

    /// <summary>Gets daily maximum, minimum and average gas prices (<c>stats/dailyavggasprice</c>).</summary>
    /// <remarks><b>PRO</b> endpoint (Standard plan and above).</remarks>
    /// <param name="startDate">First day (UTC).</param>
    /// <param name="endDate">Last day (UTC).</param>
    /// <param name="sort">Sort order.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/dailyavggasprice"/>
    Task<IReadOnlyList<DailyAverageGasPrice>> GetDailyAverageGasPriceAsync(
        DateOnly startDate, DateOnly endDate, SortOrder? sort = null, ChainId? chain = null, CancellationToken cancellationToken = default);
}
