using System.Numerics;
using EtherscanApi.Internal;
using EtherscanApi.Models;

namespace EtherscanApi;

/// <summary>
/// Token endpoints: supply, balances, holders, token info and address holdings
/// (<c>module=token</c>, plus the token actions of <c>module=account</c> and <c>module=stats</c>).
/// </summary>
internal sealed class TokensModule : EtherscanModule, ITokensModule
{
    internal TokensModule(EtherscanTransport transport)
        : base(transport)
    {
    }

    /// <inheritdoc />
    public Task<BigInteger> GetTotalSupplyAsync(
        string contractAddress,
        ChainId? chain = null,
        CancellationToken cancellationToken = default)
    {
        var request = Request("stats", "tokensupply", chain)
            .Add("contractaddress", Guard.Address(contractAddress, nameof(contractAddress)));
        return Transport.SendAsync(request, Json.BigInteger, cancellationToken);
    }

    /// <inheritdoc />
    public Task<BigInteger> GetHistoricalTotalSupplyAsync(
        string contractAddress,
        long blockNumber,
        ChainId? chain = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(blockNumber);
        var request = Request("stats", "tokensupplyhistory", chain, endpointRequestsPerSecond: 2)
            .Add("contractaddress", Guard.Address(contractAddress, nameof(contractAddress)))
            .Add("blockno", blockNumber);
        return Transport.SendAsync(request, Json.BigInteger, cancellationToken);
    }

    /// <inheritdoc />
    public Task<BigInteger> GetBalanceAsync(
        string contractAddress,
        string address,
        BlockTag? tag = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default)
    {
        var request = Request("account", "tokenbalance", chain)
            .Add("contractaddress", Guard.Address(contractAddress, nameof(contractAddress)))
            .Add("address", Guard.Address(address, nameof(address)))
            .Add("tag", (tag ?? BlockTag.Latest).ToHexString());
        return Transport.SendAsync(request, Json.BigInteger, cancellationToken);
    }

    /// <inheritdoc />
    public Task<BigInteger> GetHistoricalBalanceAsync(
        string contractAddress,
        string address,
        long blockNumber,
        ChainId? chain = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(blockNumber);
        var request = Request("account", "tokenbalancehistory", chain, endpointRequestsPerSecond: 2)
            .Add("contractaddress", Guard.Address(contractAddress, nameof(contractAddress)))
            .Add("address", Guard.Address(address, nameof(address)))
            .Add("blockno", blockNumber);
        return Transport.SendAsync(request, Json.BigInteger, cancellationToken);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<TopTokenHolder>> GetTopHoldersAsync(
        string contractAddress,
        int? count = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default)
    {
        if (count is < 1 or > 1000)
        {
            throw new ArgumentOutOfRangeException(nameof(count), count, "Must be between 1 and 1000.");
        }

        var request = Request("token", "topholders", chain, endpointRequestsPerSecond: 2)
            .Add("contractaddress", Guard.Address(contractAddress, nameof(contractAddress)))
            .Add("offset", count);
        return Transport.SendListAsync(request, Json.ListTopTokenHolder, cancellationToken);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<TokenHolder>> GetHoldersAsync(
        string contractAddress,
        PageQuery? page = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default)
    {
        var request = Request("token", "tokenholderlist", chain)
            .Add("contractaddress", Guard.Address(contractAddress, nameof(contractAddress)));
        AddPage(request, page, includeSort: false);
        return Transport.SendListAsync(request, Json.ListTokenHolder, cancellationToken);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<TokenHolder> EnumerateHoldersAsync(
        string contractAddress,
        int? pageSize = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default)
    {
        Guard.Address(contractAddress, nameof(contractAddress));
        var size = ResolvePageSize(pageSize);
        return PaginateAsync(
            (page, ct) => GetHoldersAsync(contractAddress, new PageQuery { Page = page, Offset = size }, chain, ct),
            1,
            size,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<long> GetHolderCountAsync(
        string contractAddress,
        ChainId? chain = null,
        CancellationToken cancellationToken = default)
    {
        var request = Request("token", "tokenholdercount", chain)
            .Add("contractaddress", Guard.Address(contractAddress, nameof(contractAddress)));
        return Transport.SendAsync(request, Json.Int64, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TokenInfo?> GetTokenInfoAsync(
        string contractAddress,
        ChainId? chain = null,
        CancellationToken cancellationToken = default)
    {
        var request = Request("token", "tokeninfo", chain, endpointRequestsPerSecond: 2)
            .Add("contractaddress", Guard.Address(contractAddress, nameof(contractAddress)));
        var list = await Transport.SendListAsync(request, Json.ListTokenInfo, cancellationToken).ConfigureAwait(false);
        return list.Count > 0 ? list[0] : null;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<AddressTokenBalance>> GetAddressTokenBalancesAsync(
        string address,
        PageQuery? page = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default)
    {
        var request = Request("account", "addresstokenbalance", chain, endpointRequestsPerSecond: 2)
            .Add("address", Guard.Address(address, nameof(address)));
        AddPage(request, page, includeSort: false);
        return Transport.SendListAsync(request, Json.ListAddressTokenBalance, cancellationToken);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<AddressNftBalance>> GetAddressNftBalancesAsync(
        string address,
        PageQuery? page = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default)
    {
        var request = Request("account", "addresstokennftbalance", chain, endpointRequestsPerSecond: 2)
            .Add("address", Guard.Address(address, nameof(address)));
        AddPage(request, page, includeSort: false);
        return Transport.SendListAsync(request, Json.ListAddressNftBalance, cancellationToken);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<NftInventoryItem>> GetAddressNftInventoryAsync(
        string address,
        string contractAddress,
        PageQuery? page = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default)
    {
        var request = Request("account", "addresstokennftinventory", chain, endpointRequestsPerSecond: 2)
            .Add("address", Guard.Address(address, nameof(address)))
            .Add("contractaddress", Guard.Address(contractAddress, nameof(contractAddress)));
        AddPage(request, page, includeSort: false);
        return Transport.SendListAsync(request, Json.ListNftInventoryItem, cancellationToken);
    }
}
