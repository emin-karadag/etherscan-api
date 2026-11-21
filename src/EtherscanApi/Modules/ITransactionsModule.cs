using System.Numerics;
using EtherscanApi.Models;

namespace EtherscanApi;

/// <summary>Transaction status endpoints (<c>module=transaction</c>).</summary>
public interface ITransactionsModule
{
    /// <summary>Gets the contract execution status of a transaction (<c>getstatus</c>).</summary>
    /// <param name="transactionHash">The transaction hash.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/getstatus"/>
    Task<ContractExecutionStatus> GetExecutionStatusAsync(string transactionHash, ChainId? chain = null, CancellationToken cancellationToken = default);

    /// <summary>Gets the receipt status of a transaction (<c>gettxreceiptstatus</c>).</summary>
    /// <param name="transactionHash">The transaction hash.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/gettxreceiptstatus"/>
    Task<TransactionReceiptStatus> GetReceiptStatusAsync(string transactionHash, ChainId? chain = null, CancellationToken cancellationToken = default);
}
