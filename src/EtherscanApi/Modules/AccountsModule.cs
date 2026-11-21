using System.Numerics;
using EtherscanApi.Internal;
using EtherscanApi.Models;

namespace EtherscanApi;

/// <summary>
/// Account endpoints (<c>module=account</c>): balances, transaction history, token transfers,
/// validated blocks, beacon withdrawals and funding sources.
/// </summary>
/// <remarks>See <see href="https://docs.etherscan.io/endpoint-overview">Endpoint Overview</see>.</remarks>
internal sealed class AccountsModule : EtherscanModule, IAccountsModule
{
    private const string Module = "account";

    internal AccountsModule(EtherscanTransport transport)
        : base(transport)
    {
    }

    /// <inheritdoc />
    public Task<BigInteger> GetBalanceAsync(
        string address,
        BlockTag? tag = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default)
    {
        var request = Request(Module, "balance", chain)
            .Add("address", Guard.Address(address, nameof(address)))
            .Add("tag", (tag ?? BlockTag.Latest).ToHexString());
        return Transport.SendAsync(request, Json.BigInteger, cancellationToken);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<AddressBalance>> GetBalancesAsync(
        IEnumerable<string> addresses,
        BlockTag? tag = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default)
    {
        var request = Request(Module, "balancemulti", chain)
            .Add("address", Guard.Addresses(addresses, 20, nameof(addresses)))
            .Add("tag", (tag ?? BlockTag.Latest).ToHexString());
        return Transport.SendListAsync(request, Json.ListAddressBalance, cancellationToken);
    }

    /// <inheritdoc />
    public Task<BigInteger> GetHistoricalBalanceAsync(
        string address,
        long blockNumber,
        ChainId? chain = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(blockNumber);
        var request = Request(Module, "balancehistory", chain, endpointRequestsPerSecond: 2)
            .Add("address", Guard.Address(address, nameof(address)))
            .Add("blockno", blockNumber);
        return Transport.SendAsync(request, Json.BigInteger, cancellationToken);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<NormalTransaction>> GetTransactionsAsync(
        string address,
        BlockRangeQuery? query = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default)
    {
        var request = Request(Module, "txlist", chain).Add("address", Guard.Address(address, nameof(address)));
        AddRange(request, query);
        return Transport.SendListAsync(request, Json.ListNormalTransaction, cancellationToken);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<NormalTransaction> EnumerateTransactionsAsync(
        string address,
        BlockRangeQuery? query = null,
        int? pageSize = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default)
    {
        Guard.Address(address, nameof(address));
        return EnumerateRange(query, pageSize, (q, ct) => GetTransactionsAsync(address, q, chain, ct), cancellationToken);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<NormalTransaction>> GetTransactionsAdvancedFilterAsync(
        AddressFilter filter,
        BlockRangeQuery? query = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default)
    {
        var request = Request(Module, "txlist", chain);
        AddFilter(request, filter);
        AddRange(request, query);
        return Transport.SendListAsync(request, Json.ListNormalTransaction, cancellationToken);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<InternalTransaction>> GetInternalTransactionsAsync(
        string address,
        BlockRangeQuery? query = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default)
    {
        var request = Request(Module, "txlistinternal", chain).Add("address", Guard.Address(address, nameof(address)));
        AddRange(request, query);
        return Transport.SendListAsync(request, Json.ListInternalTransaction, cancellationToken);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<InternalTransaction> EnumerateInternalTransactionsAsync(
        string address,
        BlockRangeQuery? query = null,
        int? pageSize = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default)
    {
        Guard.Address(address, nameof(address));
        return EnumerateRange(query, pageSize, (q, ct) => GetInternalTransactionsAsync(address, q, chain, ct), cancellationToken);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<InternalTransaction>> GetInternalTransactionsByHashAsync(
        string transactionHash,
        ChainId? chain = null,
        CancellationToken cancellationToken = default)
    {
        var request = Request(Module, "txlistinternal", chain).Add("txhash", Guard.Hash(transactionHash, nameof(transactionHash)));
        return Transport.SendListAsync(request, Json.ListInternalTransaction, cancellationToken);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<InternalTransaction>> GetInternalTransactionsByBlockRangeAsync(
        long startBlock,
        long endBlock,
        PageQuery? page = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(startBlock);
        ArgumentOutOfRangeException.ThrowIfLessThan(endBlock, startBlock);
        var request = Request(Module, "txlistinternal", chain).Add("startblock", startBlock).Add("endblock", endBlock);
        AddPage(request, page, includeSort: true);
        return Transport.SendListAsync(request, Json.ListInternalTransaction, cancellationToken);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<InternalTransaction>> GetInternalTransactionsAdvancedFilterAsync(
        AddressFilter filter,
        BlockRangeQuery? query = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default)
    {
        var request = Request(Module, "txlistinternal", chain);
        AddFilter(request, filter);
        AddRange(request, query);
        return Transport.SendListAsync(request, Json.ListInternalTransaction, cancellationToken);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<Erc20Transfer>> GetErc20TransfersAsync(
        string? address = null,
        string? contractAddress = null,
        BlockRangeQuery? query = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default)
        => Transport.SendListAsync(TransferRequest("tokentx", address, contractAddress, query, chain), Json.ListErc20Transfer, cancellationToken);

    /// <inheritdoc />
    public IAsyncEnumerable<Erc20Transfer> EnumerateErc20TransfersAsync(
        string? address = null,
        string? contractAddress = null,
        BlockRangeQuery? query = null,
        int? pageSize = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default)
    {
        ValidateTransferTarget(address, contractAddress);
        return EnumerateRange(query, pageSize, (q, ct) => GetErc20TransfersAsync(address, contractAddress, q, chain, ct), cancellationToken);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<Erc20Transfer>> GetErc20TransfersAdvancedFilterAsync(
        AddressFilter filter,
        string? contractAddress = null,
        BlockRangeQuery? query = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default)
        => Transport.SendListAsync(FilterTransferRequest("tokentx", filter, contractAddress, query, chain), Json.ListErc20Transfer, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<Erc721Transfer>> GetErc721TransfersAsync(
        string? address = null,
        string? contractAddress = null,
        BlockRangeQuery? query = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default)
        => Transport.SendListAsync(TransferRequest("tokennfttx", address, contractAddress, query, chain), Json.ListErc721Transfer, cancellationToken);

    /// <inheritdoc />
    public IAsyncEnumerable<Erc721Transfer> EnumerateErc721TransfersAsync(
        string? address = null,
        string? contractAddress = null,
        BlockRangeQuery? query = null,
        int? pageSize = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default)
    {
        ValidateTransferTarget(address, contractAddress);
        return EnumerateRange(query, pageSize, (q, ct) => GetErc721TransfersAsync(address, contractAddress, q, chain, ct), cancellationToken);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<Erc721Transfer>> GetErc721TransfersAdvancedFilterAsync(
        AddressFilter filter,
        string? contractAddress = null,
        BlockRangeQuery? query = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default)
        => Transport.SendListAsync(FilterTransferRequest("tokennfttx", filter, contractAddress, query, chain), Json.ListErc721Transfer, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<Erc1155Transfer>> GetErc1155TransfersAsync(
        string? address = null,
        string? contractAddress = null,
        BlockRangeQuery? query = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default)
        => Transport.SendListAsync(TransferRequest("token1155tx", address, contractAddress, query, chain), Json.ListErc1155Transfer, cancellationToken);

    /// <inheritdoc />
    public IAsyncEnumerable<Erc1155Transfer> EnumerateErc1155TransfersAsync(
        string? address = null,
        string? contractAddress = null,
        BlockRangeQuery? query = null,
        int? pageSize = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default)
    {
        ValidateTransferTarget(address, contractAddress);
        return EnumerateRange(query, pageSize, (q, ct) => GetErc1155TransfersAsync(address, contractAddress, q, chain, ct), cancellationToken);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<Erc1155Transfer>> GetErc1155TransfersAdvancedFilterAsync(
        AddressFilter filter,
        string? contractAddress = null,
        BlockRangeQuery? query = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default)
        => Transport.SendListAsync(FilterTransferRequest("token1155tx", filter, contractAddress, query, chain), Json.ListErc1155Transfer, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<MinedBlock>> GetMinedBlocksAsync(
        string address,
        MinedBlockType? blockType = null,
        PageQuery? page = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default)
    {
        var request = Request(Module, "getminedblocks", chain)
            .Add("address", Guard.Address(address, nameof(address)))
            .Add("blocktype", blockType is null ? null : WireFormat.BlockType(blockType.Value));
        AddPage(request, page, includeSort: false);
        return Transport.SendListAsync(request, Json.ListMinedBlock, cancellationToken);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<BeaconWithdrawal>> GetBeaconWithdrawalsAsync(
        string? address = null,
        BlockRangeQuery? query = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default)
    {
        var request = Request(Module, "txsBeaconWithdrawal", chain).Add("address", Guard.OptionalAddress(address, nameof(address)));
        AddRange(request, query);
        return Transport.SendListAsync(request, Json.ListBeaconWithdrawal, cancellationToken);
    }

    /// <inheritdoc />
    public Task<FundingSource> GetFundedByAsync(
        string address,
        ChainId? chain = null,
        CancellationToken cancellationToken = default)
    {
        var request = Request(Module, "fundedby", chain, endpointRequestsPerSecond: 2)
            .Add("address", Guard.Address(address, nameof(address)));
        return Transport.SendAsync(request, Json.FundingSource, cancellationToken);
    }

    private EtherscanRequest TransferRequest(string action, string? address, string? contractAddress, BlockRangeQuery? query, ChainId? chain)
    {
        var (addr, contract) = ValidateTransferTarget(address, contractAddress);
        var request = Request(Module, action, chain).Add("contractaddress", contract).Add("address", addr);
        AddRange(request, query);
        return request;
    }

    /// <summary>Transfer endpoints need an address, a token contract, or both.</summary>
    private static (string? Address, string? Contract) ValidateTransferTarget(string? address, string? contractAddress)
    {
        var addr = Guard.OptionalAddress(address, nameof(address));
        var contract = Guard.OptionalAddress(contractAddress, nameof(contractAddress));
        if (addr is null && contract is null)
        {
            throw new ArgumentException("Provide an address, a contract address, or both.", nameof(address));
        }

        return (addr, contract);
    }

    private EtherscanRequest FilterTransferRequest(string action, AddressFilter filter, string? contractAddress, BlockRangeQuery? query, ChainId? chain)
    {
        var request = Request(Module, action, chain).Add("contractaddress", Guard.OptionalAddress(contractAddress, nameof(contractAddress)));
        AddFilter(request, filter);
        AddRange(request, query);
        return request;
    }
}
