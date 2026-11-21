using System.Numerics;
using EtherscanApi.Models;

namespace EtherscanApi;

/// <summary>
/// Token endpoints: supply, balances, holders, token info and address holdings
/// (<c>module=token</c>, plus the token actions of <c>module=account</c> and <c>module=stats</c>).
/// </summary>
public interface ITokensModule
{
    /// <summary>Gets the total supply of an ERC-20 token, in its smallest unit (<c>stats/tokensupply</c>).</summary>
    /// <param name="contractAddress">The token contract.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/tokensupply"/>
    Task<BigInteger> GetTotalSupplyAsync(
        string contractAddress,
        ChainId? chain = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the total supply of an ERC-20 token at a block height (<c>stats/tokensupplyhistory</c>).</summary>
    /// <remarks><b>PRO</b> endpoint (Standard plan and above). Throttled to 2 calls/second.</remarks>
    /// <param name="contractAddress">The token contract.</param>
    /// <param name="blockNumber">The block height.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/tokensupplyhistory"/>
    Task<BigInteger> GetHistoricalTotalSupplyAsync(
        string contractAddress,
        long blockNumber,
        ChainId? chain = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the ERC-20 balance of an address, in the token's smallest unit (<c>account/tokenbalance</c>).</summary>
    /// <param name="contractAddress">The token contract.</param>
    /// <param name="address">The holder address.</param>
    /// <param name="tag"><see cref="BlockTag.Latest"/> (default).</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/tokenbalance"/>
    Task<BigInteger> GetBalanceAsync(
        string contractAddress,
        string address,
        BlockTag? tag = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the ERC-20 balance of an address at a block height (<c>account/tokenbalancehistory</c>).</summary>
    /// <remarks><b>PRO</b> endpoint (Standard plan and above). Throttled to 2 calls/second.</remarks>
    /// <param name="contractAddress">The token contract.</param>
    /// <param name="address">The holder address.</param>
    /// <param name="blockNumber">The block height.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/tokenbalancehistory"/>
    Task<BigInteger> GetHistoricalBalanceAsync(
        string contractAddress,
        string address,
        long blockNumber,
        ChainId? chain = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the top holders of an ERC-20 token (<c>token/topholders</c>).</summary>
    /// <remarks><b>PRO</b> endpoint (Standard plan and above). Throttled to 2 calls/second.</remarks>
    /// <param name="contractAddress">The token contract.</param>
    /// <param name="count">Number of holders to return, up to 1000.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/topholders"/>
    Task<IReadOnlyList<TopTokenHolder>> GetTopHoldersAsync(
        string contractAddress,
        int? count = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the holders of an ERC-20 token with their raw balances (<c>token/tokenholderlist</c>).</summary>
    /// <remarks><b>PRO</b> endpoint (Standard plan and above).</remarks>
    /// <param name="contractAddress">The token contract.</param>
    /// <param name="page">Pagination.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/tokenholderlist"/>
    Task<IReadOnlyList<TokenHolder>> GetHoldersAsync(
        string contractAddress,
        PageQuery? page = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default);

    /// <summary>Streams each holder of an ERC-20 token (<c>token/tokenholderlist</c>). Stops with an <see cref="EtherscanApiException"/> if Etherscan's <c>page × offset</c> cap is reached; narrow the block range for long histories.</summary>
    /// <remarks><b>PRO</b> endpoint (Standard plan and above).</remarks>
    /// <param name="contractAddress">The token contract.</param>
    /// <param name="pageSize">Records per request, 1 to 10,000. Defaults to 1,000.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    IAsyncEnumerable<TokenHolder> EnumerateHoldersAsync(
        string contractAddress,
        int? pageSize = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the number of holders of an ERC-20 token (<c>token/tokenholdercount</c>).</summary>
    /// <remarks><b>PRO</b> endpoint (Standard plan and above).</remarks>
    /// <param name="contractAddress">The token contract.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/tokenholdercount"/>
    Task<long> GetHolderCountAsync(
        string contractAddress,
        ChainId? chain = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets project details and social links for a token (<c>token/tokeninfo</c>).</summary>
    /// <remarks><b>PRO</b> endpoint (Standard plan and above). Throttled to 2 calls/second.</remarks>
    /// <param name="contractAddress">The ERC-20, ERC-721 or ERC-1155 contract.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The token info, or <see langword="null"/> when Etherscan has none.</returns>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/tokeninfo"/>
    Task<TokenInfo?> GetTokenInfoAsync(
        string contractAddress,
        ChainId? chain = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the ERC-20 tokens and balances held by an address (<c>account/addresstokenbalance</c>).</summary>
    /// <remarks><b>PRO</b> endpoint (Standard plan and above). Throttled to 2 calls/second.</remarks>
    /// <param name="address">The holder address.</param>
    /// <param name="page">Pagination.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/addresstokenbalance"/>
    Task<IReadOnlyList<AddressTokenBalance>> GetAddressTokenBalancesAsync(
        string address,
        PageQuery? page = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the ERC-721 collections and quantities held by an address (<c>account/addresstokennftbalance</c>).</summary>
    /// <remarks><b>PRO</b> endpoint (Standard plan and above). Throttled to 2 calls/second.</remarks>
    /// <param name="address">The holder address.</param>
    /// <param name="page">Pagination.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/addresstokennftbalance"/>
    Task<IReadOnlyList<AddressNftBalance>> GetAddressNftBalancesAsync(
        string address,
        PageQuery? page = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the ERC-721 token ids an address holds in one collection (<c>account/addresstokennftinventory</c>).</summary>
    /// <remarks><b>PRO</b> endpoint (Standard plan and above). Throttled to 2 calls/second.</remarks>
    /// <param name="address">The holder address.</param>
    /// <param name="contractAddress">The NFT contract.</param>
    /// <param name="page">Pagination.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/addresstokennftinventory"/>
    Task<IReadOnlyList<NftInventoryItem>> GetAddressNftInventoryAsync(
        string address,
        string contractAddress,
        PageQuery? page = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default);
}
