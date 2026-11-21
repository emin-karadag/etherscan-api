using System.Numerics;
using EtherscanApi.Internal;
using EtherscanApi.Models;

namespace EtherscanApi;

/// <summary>Network statistics endpoints (<c>module=stats</c>): supply, price, node counts and daily network data.</summary>
internal sealed class StatsModule : EtherscanModule, IStatsModule
{
    private const string Module = "stats";

    internal StatsModule(EtherscanTransport transport)
        : base(transport)
    {
    }

    /// <inheritdoc />
    public Task<BigInteger> GetEtherSupplyAsync(ChainId? chain = null, CancellationToken cancellationToken = default)
        => Transport.SendAsync(Request(Module, "ethsupply", chain), Json.BigInteger, cancellationToken);

    /// <inheritdoc />
    public Task<EtherSupplyDetails> GetEtherSupplyDetailsAsync(ChainId? chain = null, CancellationToken cancellationToken = default)
        => Transport.SendAsync(Request(Module, "ethsupply2", chain), Json.EtherSupplyDetails, cancellationToken);

    /// <inheritdoc />
    public Task<NativeTokenPrice> GetLastPriceAsync(ChainId? chain = null, CancellationToken cancellationToken = default)
        => Transport.SendAsync(Request(Module, "ethprice", chain), Json.NativeTokenPrice, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<ChainSizeEntry>> GetChainSizeAsync(
        DateOnly startDate,
        DateOnly endDate,
        NodeClientType clientType = NodeClientType.Geth,
        NodeSyncMode syncMode = NodeSyncMode.Default,
        SortOrder? sort = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default)
    {
        var request = DailyRequest("chainsize", startDate, endDate, sort, chain)
            .Add("clienttype", WireFormat.ClientType(clientType))
            .Add("syncmode", WireFormat.SyncMode(syncMode));
        return Transport.SendListAsync(request, Json.ListChainSizeEntry, cancellationToken);
    }

    /// <inheritdoc />
    public Task<NodeCount> GetNodeCountAsync(ChainId? chain = null, CancellationToken cancellationToken = default)
        => Transport.SendAsync(Request(Module, "nodecount", chain), Json.NodeCount, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<NodeCount>> GetNodeCountHistoryAsync(
        DateOnly startDate, DateOnly endDate, SortOrder? sort = null, ChainId? chain = null, CancellationToken cancellationToken = default)
        => Transport.SendListAsync(DailyRequest("nodecounthistory", startDate, endDate, sort, chain), Json.ListNodeCount, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<DailyTransactionFee>> GetDailyTransactionFeesAsync(
        DateOnly startDate, DateOnly endDate, SortOrder? sort = null, ChainId? chain = null, CancellationToken cancellationToken = default)
        => Transport.SendListAsync(DailyRequest("dailytxnfee", startDate, endDate, sort, chain), Json.ListDailyTransactionFee, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<DailyNewAddressCount>> GetDailyNewAddressCountAsync(
        DateOnly startDate, DateOnly endDate, SortOrder? sort = null, ChainId? chain = null, CancellationToken cancellationToken = default)
        => Transport.SendListAsync(DailyRequest("dailynewaddress", startDate, endDate, sort, chain), Json.ListDailyNewAddressCount, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<DailyEnsRegistrationCount>> GetDailyEnsRegistrationCountAsync(
        DateOnly startDate, DateOnly endDate, SortOrder? sort = null, ChainId? chain = null, CancellationToken cancellationToken = default)
        => Transport.SendListAsync(DailyRequest("dailyensregister", startDate, endDate, sort, chain), Json.ListDailyEnsRegistrationCount, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<DailyNetworkUtilization>> GetDailyNetworkUtilizationAsync(
        DateOnly startDate, DateOnly endDate, SortOrder? sort = null, ChainId? chain = null, CancellationToken cancellationToken = default)
        => Transport.SendListAsync(DailyRequest("dailynetutilization", startDate, endDate, sort, chain), Json.ListDailyNetworkUtilization, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<DailyAverageHashRate>> GetDailyAverageHashRateAsync(
        DateOnly startDate, DateOnly endDate, SortOrder? sort = null, ChainId? chain = null, CancellationToken cancellationToken = default)
        => Transport.SendListAsync(DailyRequest("dailyavghashrate", startDate, endDate, sort, chain), Json.ListDailyAverageHashRate, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<DailyTransactionCount>> GetDailyTransactionCountAsync(
        DateOnly startDate, DateOnly endDate, SortOrder? sort = null, ChainId? chain = null, CancellationToken cancellationToken = default)
        => Transport.SendListAsync(DailyRequest("dailytx", startDate, endDate, sort, chain), Json.ListDailyTransactionCount, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<DailyAverageNetworkDifficulty>> GetDailyAverageNetworkDifficultyAsync(
        DateOnly startDate, DateOnly endDate, SortOrder? sort = null, ChainId? chain = null, CancellationToken cancellationToken = default)
        => Transport.SendListAsync(DailyRequest("dailyavgnetdifficulty", startDate, endDate, sort, chain), Json.ListDailyAverageNetworkDifficulty, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<DailyNativeTokenPrice>> GetDailyPriceAsync(
        DateOnly startDate, DateOnly endDate, SortOrder? sort = null, ChainId? chain = null, CancellationToken cancellationToken = default)
        => Transport.SendListAsync(DailyRequest("ethdailyprice", startDate, endDate, sort, chain), Json.ListDailyNativeTokenPrice, cancellationToken);
}
