using System.Numerics;
using EtherscanApi.Internal;
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
internal sealed class ProxyModule : EtherscanModule, IProxyModule
{
    private const string Module = "proxy";

    internal ProxyModule(EtherscanTransport transport)
        : base(transport)
    {
    }

    /// <inheritdoc />
    public Task<long> GetBlockNumberAsync(ChainId? chain = null, CancellationToken cancellationToken = default)
        => Transport.SendRpcRequiredAsync(Request(Module, "eth_blockNumber", chain), Json.Int64, cancellationToken);

    /// <inheritdoc />
    public Task<RpcBlock?> GetBlockByNumberAsync(BlockTag block, ChainId? chain = null, CancellationToken cancellationToken = default)
    {
        var request = Request(Module, "eth_getBlockByNumber", chain).Add("tag", block.ToHexString()).Add("boolean", "false");
        return Transport.SendRpcAsync(request, Json.RpcBlock, cancellationToken);
    }

    /// <inheritdoc />
    public Task<RpcBlockWithTransactions?> GetBlockWithTransactionsByNumberAsync(BlockTag block, ChainId? chain = null, CancellationToken cancellationToken = default)
    {
        var request = Request(Module, "eth_getBlockByNumber", chain).Add("tag", block.ToHexString()).Add("boolean", "true");
        return Transport.SendRpcAsync(request, Json.RpcBlockWithTransactions, cancellationToken);
    }

    /// <inheritdoc />
    public Task<RpcBlock?> GetUncleByBlockNumberAndIndexAsync(BlockTag block, int index, ChainId? chain = null, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        var request = Request(Module, "eth_getUncleByBlockNumberAndIndex", chain)
            .Add("tag", block.ToHexString())
            .Add("index", WireFormat.Hex((ulong)index));
        return Transport.SendRpcAsync(request, Json.RpcBlock, cancellationToken);
    }

    /// <inheritdoc />
    public Task<long> GetBlockTransactionCountByNumberAsync(BlockTag block, ChainId? chain = null, CancellationToken cancellationToken = default)
    {
        var request = Request(Module, "eth_getBlockTransactionCountByNumber", chain).Add("tag", block.ToHexString());
        return Transport.SendRpcRequiredAsync(request, Json.Int64, cancellationToken);
    }

    /// <inheritdoc />
    public Task<RpcTransaction?> GetTransactionByHashAsync(string transactionHash, ChainId? chain = null, CancellationToken cancellationToken = default)
    {
        var request = Request(Module, "eth_getTransactionByHash", chain).Add("txhash", Guard.Hash(transactionHash, nameof(transactionHash)));
        return Transport.SendRpcAsync(request, Json.RpcTransaction, cancellationToken);
    }

    /// <inheritdoc />
    public Task<RpcTransaction?> GetTransactionByBlockNumberAndIndexAsync(BlockTag block, int index, ChainId? chain = null, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        var request = Request(Module, "eth_getTransactionByBlockNumberAndIndex", chain)
            .Add("tag", block.ToHexString())
            .Add("index", WireFormat.Hex((ulong)index));
        return Transport.SendRpcAsync(request, Json.RpcTransaction, cancellationToken);
    }

    /// <inheritdoc />
    public Task<long> GetTransactionCountAsync(string address, BlockTag? block = null, ChainId? chain = null, CancellationToken cancellationToken = default)
    {
        var request = Request(Module, "eth_getTransactionCount", chain)
            .Add("address", Guard.Address(address, nameof(address)))
            .Add("tag", (block ?? BlockTag.Latest).ToHexString());
        return Transport.SendRpcRequiredAsync(request, Json.Int64, cancellationToken);
    }

    /// <inheritdoc />
    public Task<string> SendRawTransactionAsync(string signedTransactionHex, ChainId? chain = null, CancellationToken cancellationToken = default)
    {
        var request = new EtherscanRequest(Module, "eth_sendRawTransaction", Transport.Resolve(chain)) { IsIdempotent = false }
            .Add("hex", Guard.HexData(signedTransactionHex, nameof(signedTransactionHex)));
        return Transport.SendRpcRequiredAsync(request, Json.String, cancellationToken);
    }

    /// <inheritdoc />
    public Task<RpcTransactionReceipt?> GetTransactionReceiptAsync(string transactionHash, ChainId? chain = null, CancellationToken cancellationToken = default)
    {
        var request = Request(Module, "eth_getTransactionReceipt", chain).Add("txhash", Guard.Hash(transactionHash, nameof(transactionHash)));
        return Transport.SendRpcAsync(request, Json.RpcTransactionReceipt, cancellationToken);
    }

    /// <inheritdoc />
    public Task<string> CallAsync(string to, string data, BlockTag? block = null, ChainId? chain = null, CancellationToken cancellationToken = default)
    {
        var request = Request(Module, "eth_call", chain)
            .Add("to", Guard.Address(to, nameof(to)))
            .Add("data", Guard.HexData(data, nameof(data)))
            .Add("tag", (block ?? BlockTag.Latest).ToHexString());
        return Transport.SendRpcRequiredAsync(request, Json.String, cancellationToken);
    }

    /// <inheritdoc />
    public Task<string> GetCodeAsync(string address, BlockTag? block = null, ChainId? chain = null, CancellationToken cancellationToken = default)
    {
        var request = Request(Module, "eth_getCode", chain)
            .Add("address", Guard.Address(address, nameof(address)))
            .Add("tag", (block ?? BlockTag.Latest).ToHexString());
        return Transport.SendRpcRequiredAsync(request, Json.String, cancellationToken);
    }

    /// <inheritdoc />
    public Task<string> GetStorageAtAsync(string address, BigInteger position, BlockTag? block = null, ChainId? chain = null, CancellationToken cancellationToken = default)
    {
        var request = Request(Module, "eth_getStorageAt", chain)
            .Add("address", Guard.Address(address, nameof(address)))
            .Add("position", WireFormat.Hex(position))
            .Add("tag", (block ?? BlockTag.Latest).ToHexString());
        return Transport.SendRpcRequiredAsync(request, Json.String, cancellationToken);
    }

    /// <inheritdoc />
    public Task<BigInteger> GetGasPriceAsync(ChainId? chain = null, CancellationToken cancellationToken = default)
        => Transport.SendRpcRequiredAsync(Request(Module, "eth_gasPrice", chain), Json.BigInteger, cancellationToken);

    /// <inheritdoc />
    public Task<BigInteger> EstimateGasAsync(
        string to,
        string? data = null,
        BigInteger? value = null,
        BigInteger? gasPrice = null,
        BigInteger? gas = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default)
    {
        var request = Request(Module, "eth_estimateGas", chain)
            .Add("to", Guard.Address(to, nameof(to)))
            .Add("data", Guard.OptionalHexData(data, nameof(data)))
            .Add("value", value is null ? null : WireFormat.Hex(value.Value))
            .Add("gasPrice", gasPrice is null ? null : WireFormat.Hex(gasPrice.Value))
            .Add("gas", gas is null ? null : WireFormat.Hex(gas.Value));
        return Transport.SendRpcRequiredAsync(request, Json.BigInteger, cancellationToken);
    }
}
