using System.Numerics;
using EtherscanApi.Models;

namespace EtherscanApi;

/// <summary>Network statistics endpoints (<c>module=stats</c>): supply, price, node counts and daily network data.</summary>
public interface IStatsModule
{
    /// <summary>Gets the circulating supply of the native token, in wei (<c>ethsupply</c>).</summary>
    /// <remarks>Excludes ETH2 staking rewards and EIP-1559 burned fees. See <see cref="GetEtherSupplyDetailsAsync"/>.</remarks>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/ethsupply"/>
    Task<BigInteger> GetEtherSupplyAsync(ChainId? chain = null, CancellationToken cancellationToken = default);

    /// <summary>Gets the supply including staking rewards, burned fees and beacon withdrawals (<c>ethsupply2</c>).</summary>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/ethsupply2"/>
    Task<EtherSupplyDetails> GetEtherSupplyDetailsAsync(ChainId? chain = null, CancellationToken cancellationToken = default);

    /// <summary>Gets the latest price of the chain's native token in USD and BTC (<c>ethprice</c>).</summary>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/ethprice"/>
    Task<NativeTokenPrice> GetLastPriceAsync(ChainId? chain = null, CancellationToken cancellationToken = default);

    /// <summary>Gets the blockchain size per day (<c>chainsize</c>).</summary>
    /// <param name="startDate">First day (UTC).</param>
    /// <param name="endDate">Last day (UTC).</param>
    /// <param name="clientType">Node client.</param>
    /// <param name="syncMode">Node sync mode.</param>
    /// <param name="sort">Sort order.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/chainsize"/>
    Task<IReadOnlyList<ChainSizeEntry>> GetChainSizeAsync(
        DateOnly startDate,
        DateOnly endDate,
        NodeClientType clientType = NodeClientType.Geth,
        NodeSyncMode syncMode = NodeSyncMode.Default,
        SortOrder? sort = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the total count of discoverable nodes (<c>nodecount</c>).</summary>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/nodecount"/>
    Task<NodeCount> GetNodeCountAsync(ChainId? chain = null, CancellationToken cancellationToken = default);

    /// <summary>Gets the daily count of discoverable nodes (<c>nodecounthistory</c>).</summary>
    /// <remarks><b>PRO</b> endpoint (Standard plan and above).</remarks>
    /// <param name="startDate">First day (UTC).</param>
    /// <param name="endDate">Last day (UTC).</param>
    /// <param name="sort">Sort order.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/nodecounthistory"/>
    Task<IReadOnlyList<NodeCount>> GetNodeCountHistoryAsync(
        DateOnly startDate, DateOnly endDate, SortOrder? sort = null, ChainId? chain = null, CancellationToken cancellationToken = default);

    /// <summary>Gets the total transaction fees paid each day (<c>dailytxnfee</c>).</summary>
    /// <remarks><b>PRO</b> endpoint (Standard plan and above).</remarks>
    /// <param name="startDate">First day (UTC).</param>
    /// <param name="endDate">Last day (UTC).</param>
    /// <param name="sort">Sort order.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/dailytxnfee"/>
    Task<IReadOnlyList<DailyTransactionFee>> GetDailyTransactionFeesAsync(
        DateOnly startDate, DateOnly endDate, SortOrder? sort = null, ChainId? chain = null, CancellationToken cancellationToken = default);

    /// <summary>Gets the daily count of new addresses (<c>dailynewaddress</c>).</summary>
    /// <remarks><b>PRO</b> endpoint (Standard plan and above).</remarks>
    /// <param name="startDate">First day (UTC).</param>
    /// <param name="endDate">Last day (UTC).</param>
    /// <param name="sort">Sort order.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/dailynewaddress"/>
    Task<IReadOnlyList<DailyNewAddressCount>> GetDailyNewAddressCountAsync(
        DateOnly startDate, DateOnly endDate, SortOrder? sort = null, ChainId? chain = null, CancellationToken cancellationToken = default);

    /// <summary>Gets the daily count of ENS registrations (<c>dailyensregister</c>).</summary>
    /// <remarks><b>PRO</b> endpoint (Standard plan and above). Ethereum only.</remarks>
    /// <param name="startDate">First day (UTC).</param>
    /// <param name="endDate">Last day (UTC).</param>
    /// <param name="sort">Sort order.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/dailyensregister"/>
    Task<IReadOnlyList<DailyEnsRegistrationCount>> GetDailyEnsRegistrationCountAsync(
        DateOnly startDate, DateOnly endDate, SortOrder? sort = null, ChainId? chain = null, CancellationToken cancellationToken = default);

    /// <summary>Gets the daily network utilization (<c>dailynetutilization</c>).</summary>
    /// <remarks><b>PRO</b> endpoint (Standard plan and above).</remarks>
    /// <param name="startDate">First day (UTC).</param>
    /// <param name="endDate">Last day (UTC).</param>
    /// <param name="sort">Sort order.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/dailynetutilization"/>
    Task<IReadOnlyList<DailyNetworkUtilization>> GetDailyNetworkUtilizationAsync(
        DateOnly startDate, DateOnly endDate, SortOrder? sort = null, ChainId? chain = null, CancellationToken cancellationToken = default);

    /// <summary>Gets the daily average network hash rate (<c>dailyavghashrate</c>).</summary>
    /// <remarks><b>PRO</b> endpoint (Standard plan and above).</remarks>
    /// <param name="startDate">First day (UTC).</param>
    /// <param name="endDate">Last day (UTC).</param>
    /// <param name="sort">Sort order.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/dailyavghashrate"/>
    Task<IReadOnlyList<DailyAverageHashRate>> GetDailyAverageHashRateAsync(
        DateOnly startDate, DateOnly endDate, SortOrder? sort = null, ChainId? chain = null, CancellationToken cancellationToken = default);

    /// <summary>Gets the daily transaction count (<c>dailytx</c>).</summary>
    /// <remarks><b>PRO</b> endpoint (Standard plan and above).</remarks>
    /// <param name="startDate">First day (UTC).</param>
    /// <param name="endDate">Last day (UTC).</param>
    /// <param name="sort">Sort order.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/dailytx"/>
    Task<IReadOnlyList<DailyTransactionCount>> GetDailyTransactionCountAsync(
        DateOnly startDate, DateOnly endDate, SortOrder? sort = null, ChainId? chain = null, CancellationToken cancellationToken = default);

    /// <summary>Gets the daily average network difficulty (<c>dailyavgnetdifficulty</c>).</summary>
    /// <remarks><b>PRO</b> endpoint (Standard plan and above).</remarks>
    /// <param name="startDate">First day (UTC).</param>
    /// <param name="endDate">Last day (UTC).</param>
    /// <param name="sort">Sort order.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/dailyavgnetdifficulty"/>
    Task<IReadOnlyList<DailyAverageNetworkDifficulty>> GetDailyAverageNetworkDifficultyAsync(
        DateOnly startDate, DateOnly endDate, SortOrder? sort = null, ChainId? chain = null, CancellationToken cancellationToken = default);

    /// <summary>Gets the historical daily price of the native token (<c>ethdailyprice</c>).</summary>
    /// <remarks><b>PRO</b> endpoint (Standard plan and above).</remarks>
    /// <param name="startDate">First day (UTC).</param>
    /// <param name="endDate">Last day (UTC).</param>
    /// <param name="sort">Sort order.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/ethdailyprice"/>
    Task<IReadOnlyList<DailyNativeTokenPrice>> GetDailyPriceAsync(
        DateOnly startDate, DateOnly endDate, SortOrder? sort = null, ChainId? chain = null, CancellationToken cancellationToken = default);
}
