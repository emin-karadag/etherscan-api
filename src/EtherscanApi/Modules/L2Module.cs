using EtherscanApi.Internal;
using EtherscanApi.Models;

namespace EtherscanApi;

/// <summary>L2 bridge endpoints: Plasma deposits and L2 deposit/withdrawal transactions (<c>module=account</c>).</summary>
internal sealed class L2Module : EtherscanModule, IL2Module
{
    internal L2Module(EtherscanTransport transport)
        : base(transport)
    {
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<PlasmaDeposit>> GetPlasmaDepositsAsync(
        string? address = null,
        PageQuery? page = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default)
    {
        var request = Request("account", "txnbridge", chain).Add("address", Guard.OptionalAddress(address, nameof(address)));
        AddPage(request, page, includeSort: false);
        return Transport.SendListAsync(request, Json.ListPlasmaDeposit, cancellationToken);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<L2DepositTransaction>> GetDepositTransactionsAsync(
        string address,
        PageQuery? page = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default)
    {
        var request = Request("account", "getdeposittxs", chain).Add("address", Guard.Address(address, nameof(address)));
        AddPage(request, page, includeSort: true);
        return Transport.SendListAsync(request, Json.ListL2DepositTransaction, cancellationToken);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<L2WithdrawalTransaction>> GetWithdrawalTransactionsAsync(
        string address,
        PageQuery? page = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default)
    {
        var request = Request("account", "getwithdrawaltxs", chain).Add("address", Guard.Address(address, nameof(address)));
        AddPage(request, page, includeSort: true);
        return Transport.SendListAsync(request, Json.ListL2WithdrawalTransaction, cancellationToken);
    }
}
