using System.Numerics;
using EtherscanApi.Internal;
using EtherscanApi.Models;

namespace EtherscanApi;

/// <summary>Gas tracker endpoints (<c>module=gastracker</c>) and daily gas statistics (<c>module=stats</c>).</summary>
internal sealed class GasTrackerModule : EtherscanModule, IGasTrackerModule
{
    internal GasTrackerModule(EtherscanTransport transport)
        : base(transport)
    {
    }

    /// <inheritdoc />
    public Task<GasOracle> GetGasOracleAsync(ChainId? chain = null, CancellationToken cancellationToken = default)
        => Transport.SendAsync(Request("gastracker", "gasoracle", chain), Json.GasOracle, cancellationToken);

    /// <inheritdoc />
    public async Task<TimeSpan> GetConfirmationTimeEstimateAsync(
        BigInteger gasPriceWei,
        ChainId? chain = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(gasPriceWei);
        var request = Request("gastracker", "gasestimate", chain).Add("gasprice", gasPriceWei);
        var seconds = await Transport.SendAsync(request, Json.Int64, cancellationToken).ConfigureAwait(false);
        return TimeSpan.FromSeconds(seconds);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<DailyAverageGasLimit>> GetDailyAverageGasLimitAsync(
        DateOnly startDate, DateOnly endDate, SortOrder? sort = null, ChainId? chain = null, CancellationToken cancellationToken = default)
        => Transport.SendListAsync(DailyRequest("dailyavggaslimit", startDate, endDate, sort, chain), Json.ListDailyAverageGasLimit, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<DailyGasUsed>> GetDailyGasUsedAsync(
        DateOnly startDate, DateOnly endDate, SortOrder? sort = null, ChainId? chain = null, CancellationToken cancellationToken = default)
        => Transport.SendListAsync(DailyRequest("dailygasused", startDate, endDate, sort, chain), Json.ListDailyGasUsed, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<DailyAverageGasPrice>> GetDailyAverageGasPriceAsync(
        DateOnly startDate, DateOnly endDate, SortOrder? sort = null, ChainId? chain = null, CancellationToken cancellationToken = default)
        => Transport.SendListAsync(DailyRequest("dailyavggasprice", startDate, endDate, sort, chain), Json.ListDailyAverageGasPrice, cancellationToken);
}
