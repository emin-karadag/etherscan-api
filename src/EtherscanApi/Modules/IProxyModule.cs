using System.Numerics;
using EtherscanApi.Models;

namespace EtherscanApi;

/// <summary>
/// Geth/Parity JSON-RPC proxy endpoints (<c>module=proxy</c>, <c>eth_*</c> actions).
/// Quantities are decoded from hex; block references use <see cref="BlockTag"/>.
/// </summary>
/// <remarks>
/// JSON-RPC errors raise <see cref="EtherscanRpcException"/>. Unknown blocks or transactions return
/// <see langword="null"/>.
/// </remarks>
public interface IProxyModule
{
    /// <summary>Gets the most recent block number (<c>eth_blockNumber</c>).</summary>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/ethblocknumber"/>
    Task<long> GetBlockNumberAsync(ChainId? chain = null, CancellationToken cancellationToken = default);

    /// <summary>Gets a block with transaction hashes (<c>eth_getBlockByNumber</c>, <c>boolean=false</c>).</summary>
    /// <param name="block">The block number or tag.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/ethgetblockbynumber"/>
    Task<RpcBlock?> GetBlockByNumberAsync(BlockTag block, ChainId? chain = null, CancellationToken cancellationToken = default);

    /// <summary>Gets a block with full transaction objects (<c>eth_getBlockByNumber</c>, <c>boolean=true</c>).</summary>
    /// <param name="block">The block number or tag.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/ethgetblockbynumber"/>
    Task<RpcBlockWithTransactions?> GetBlockWithTransactionsByNumberAsync(BlockTag block, ChainId? chain = null, CancellationToken cancellationToken = default);

    /// <summary>Gets an uncle by block number and index (<c>eth_getUncleByBlockNumberAndIndex</c>).</summary>
    /// <param name="block">The block number.</param>
    /// <param name="index">The uncle index.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/ethgetunclebyblocknumberandindex"/>
    Task<RpcBlock?> GetUncleByBlockNumberAndIndexAsync(BlockTag block, int index, ChainId? chain = null, CancellationToken cancellationToken = default);

    /// <summary>Gets the number of transactions in a block (<c>eth_getBlockTransactionCountByNumber</c>).</summary>
    /// <param name="block">The block number or tag.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/ethgetblocktransactioncountbynumber"/>
    Task<long> GetBlockTransactionCountByNumberAsync(BlockTag block, ChainId? chain = null, CancellationToken cancellationToken = default);

    /// <summary>Gets a transaction by hash (<c>eth_getTransactionByHash</c>).</summary>
    /// <param name="transactionHash">The transaction hash.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The transaction, or <see langword="null"/> if unknown.</returns>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/ethgettransactionbyhash"/>
    Task<RpcTransaction?> GetTransactionByHashAsync(string transactionHash, ChainId? chain = null, CancellationToken cancellationToken = default);

    /// <summary>Gets a transaction by block number and index (<c>eth_getTransactionByBlockNumberAndIndex</c>).</summary>
    /// <param name="block">The block number or tag.</param>
    /// <param name="index">The transaction index in the block.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/ethgettransactionbyblocknumberandindex"/>
    Task<RpcTransaction?> GetTransactionByBlockNumberAndIndexAsync(BlockTag block, int index, ChainId? chain = null, CancellationToken cancellationToken = default);

    /// <summary>Gets the number of transactions sent from an address, i.e. its nonce (<c>eth_getTransactionCount</c>).</summary>
    /// <param name="address">The address.</param>
    /// <param name="block">The block tag; defaults to <see cref="BlockTag.Latest"/>.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/ethgettransactioncount"/>
    Task<long> GetTransactionCountAsync(string address, BlockTag? block = null, ChainId? chain = null, CancellationToken cancellationToken = default);

    /// <summary>Broadcasts a signed raw transaction (<c>eth_sendRawTransaction</c>).</summary>
    /// <remarks>
    /// This publishes a transaction to the network. It is sent exactly once and never retried automatically,
    /// even on rate-limit errors. Sign transactions locally; never send private keys anywhere.
    /// </remarks>
    /// <param name="signedTransactionHex">The signed transaction, <c>0x</c>-prefixed hex.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The transaction hash.</returns>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/ethsendrawtransaction"/>
    Task<string> SendRawTransactionAsync(string signedTransactionHex, ChainId? chain = null, CancellationToken cancellationToken = default);

    /// <summary>Gets a transaction receipt (<c>eth_getTransactionReceipt</c>).</summary>
    /// <param name="transactionHash">The transaction hash.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The receipt, or <see langword="null"/> if the transaction is unknown or pending.</returns>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/ethgettransactionreceipt"/>
    Task<RpcTransactionReceipt?> GetTransactionReceiptAsync(string transactionHash, ChainId? chain = null, CancellationToken cancellationToken = default);

    /// <summary>Executes a read-only message call without creating a transaction (<c>eth_call</c>).</summary>
    /// <param name="to">The contract address.</param>
    /// <param name="data">ABI-encoded call data, <c>0x</c>-prefixed.</param>
    /// <param name="block">The block tag; defaults to <see cref="BlockTag.Latest"/>.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The return data as hex.</returns>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/ethcall"/>
    Task<string> CallAsync(string to, string data, BlockTag? block = null, ChainId? chain = null, CancellationToken cancellationToken = default);

    /// <summary>Gets the code at an address (<c>eth_getCode</c>).</summary>
    /// <param name="address">The address.</param>
    /// <param name="block">The block tag; defaults to <see cref="BlockTag.Latest"/>.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The bytecode as hex; <c>0x</c> for externally owned accounts.</returns>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/ethgetcode"/>
    Task<string> GetCodeAsync(string address, BlockTag? block = null, ChainId? chain = null, CancellationToken cancellationToken = default);

    /// <summary>Gets the value of a storage slot (<c>eth_getStorageAt</c>).</summary>
    /// <param name="address">The contract address.</param>
    /// <param name="position">The storage slot.</param>
    /// <param name="block">The block tag; defaults to <see cref="BlockTag.Latest"/>.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The 32-byte slot value as hex.</returns>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/ethgetstorageat"/>
    Task<string> GetStorageAtAsync(string address, BigInteger position, BlockTag? block = null, ChainId? chain = null, CancellationToken cancellationToken = default);

    /// <summary>Gets the current gas price, in wei (<c>eth_gasPrice</c>).</summary>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/ethgasprice"/>
    Task<BigInteger> GetGasPriceAsync(ChainId? chain = null, CancellationToken cancellationToken = default);

    /// <summary>Estimates the gas a transaction would use (<c>eth_estimateGas</c>).</summary>
    /// <param name="to">The recipient or contract.</param>
    /// <param name="data">Optional call data.</param>
    /// <param name="value">Optional value, in wei.</param>
    /// <param name="gasPrice">Optional gas price, in wei.</param>
    /// <param name="gas">Optional gas limit.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/ethestimategas"/>
    Task<BigInteger> EstimateGasAsync(
        string to,
        string? data = null,
        BigInteger? value = null,
        BigInteger? gasPrice = null,
        BigInteger? gas = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default);
}
