using EtherscanApi.Internal;
using EtherscanApi.Models;

namespace EtherscanApi;

/// <summary>Transaction status endpoints (<c>module=transaction</c>).</summary>
internal sealed class TransactionsModule : EtherscanModule, ITransactionsModule
{
    internal TransactionsModule(EtherscanTransport transport)
        : base(transport)
    {
    }

    /// <inheritdoc />
    public Task<ContractExecutionStatus> GetExecutionStatusAsync(string transactionHash, ChainId? chain = null, CancellationToken cancellationToken = default)
    {
        var request = Request("transaction", "getstatus", chain).Add("txhash", Guard.Hash(transactionHash, nameof(transactionHash)));
        return Transport.SendAsync(request, Json.ContractExecutionStatus, cancellationToken);
    }

    /// <inheritdoc />
    public Task<TransactionReceiptStatus> GetReceiptStatusAsync(string transactionHash, ChainId? chain = null, CancellationToken cancellationToken = default)
    {
        var request = Request("transaction", "gettxreceiptstatus", chain).Add("txhash", Guard.Hash(transactionHash, nameof(transactionHash)));
        return Transport.SendAsync(request, Json.TransactionReceiptStatus, cancellationToken);
    }
}
