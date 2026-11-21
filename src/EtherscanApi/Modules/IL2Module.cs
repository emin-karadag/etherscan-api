using System.Numerics;
using EtherscanApi.Models;

namespace EtherscanApi;

/// <summary>L2 bridge endpoints: Plasma deposits and L2 deposit/withdrawal transactions (<c>module=account</c>).</summary>
public interface IL2Module
{
    /// <summary>Gets Plasma (Polygon PoS bridge) deposits received by an address (<c>txnbridge</c>).</summary>
    /// <param name="address">The receiving address.</param>
    /// <param name="page">Pagination.</param>
    /// <param name="chain">The chain (for example <see cref="ChainId.Polygon"/>).</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/txnbridge"/>
    Task<IReadOnlyList<PlasmaDeposit>> GetPlasmaDepositsAsync(
        string? address = null,
        PageQuery? page = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets L1→L2 deposit transactions of an address (<c>getdeposittxs</c>).</summary>
    /// <param name="address">The address.</param>
    /// <param name="page">Pagination and sort.</param>
    /// <param name="chain">The L2 chain (for example <see cref="ChainId.ArbitrumOne"/> or <see cref="ChainId.Optimism"/>).</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/getdeposittxs"/>
    Task<IReadOnlyList<L2DepositTransaction>> GetDepositTransactionsAsync(
        string address,
        PageQuery? page = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets L2→L1 withdrawal transactions of an address (<c>getwithdrawaltxs</c>).</summary>
    /// <param name="address">The address.</param>
    /// <param name="page">Pagination and sort.</param>
    /// <param name="chain">The L2 chain.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/getwithdrawaltxs"/>
    Task<IReadOnlyList<L2WithdrawalTransaction>> GetWithdrawalTransactionsAsync(
        string address,
        PageQuery? page = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default);
}
