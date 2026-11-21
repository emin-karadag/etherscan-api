using System.Numerics;
using EtherscanApi.Models;

namespace EtherscanApi;

/// <summary>
/// Account endpoints (<c>module=account</c>): balances, transaction history, token transfers,
/// validated blocks, beacon withdrawals and funding sources.
/// </summary>
/// <remarks>See <see href="https://docs.etherscan.io/endpoint-overview">Endpoint Overview</see>.</remarks>
public interface IAccountsModule
{
    /// <summary>Gets the native token balance of an address, in wei (<c>balance</c>).</summary>
    /// <param name="address">The address.</param>
    /// <param name="tag">
    /// <see cref="BlockTag.Latest"/> (default) or a block within the last 128 blocks.
    /// For older blocks use <see cref="GetHistoricalBalanceAsync"/>.
    /// </param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/balance"/>
    Task<BigInteger> GetBalanceAsync(
        string address,
        BlockTag? tag = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the native balances of up to 20 addresses in one call (<c>balancemulti</c>).</summary>
    /// <param name="addresses">1 to 20 addresses.</param>
    /// <param name="tag"><see cref="BlockTag.Latest"/> (default) or a block within the last 128 blocks.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/balancemulti"/>
    Task<IReadOnlyList<AddressBalance>> GetBalancesAsync(
        IEnumerable<string> addresses,
        BlockTag? tag = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the native balance of an address at a block height, in wei (<c>balancehistory</c>).</summary>
    /// <remarks><b>PRO</b> endpoint (Standard plan and above). Throttled to 2 calls/second.</remarks>
    /// <param name="address">The address.</param>
    /// <param name="blockNumber">The block height.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/balancehistory"/>
    Task<BigInteger> GetHistoricalBalanceAsync(
        string address,
        long blockNumber,
        ChainId? chain = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the normal transactions of an address (<c>txlist</c>).</summary>
    /// <param name="address">The address.</param>
    /// <param name="query">Block range, page and sort.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The transactions, or an empty list when there are none.</returns>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/txlist"/>
    Task<IReadOnlyList<NormalTransaction>> GetTransactionsAsync(
        string address,
        BlockRangeQuery? query = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default);

    /// <summary>Streams each normal transaction of an address, fetching pages on demand (<c>txlist</c>). Stops with an <see cref="EtherscanApiException"/> if Etherscan's <c>page × offset</c> cap is reached; narrow the block range for long histories.</summary>
    /// <param name="address">The address.</param>
    /// <param name="query">Block range and sort. <see cref="BlockRangeQuery.Page"/> sets the first page; <see cref="BlockRangeQuery.Offset"/> is replaced by <paramref name="pageSize"/>.</param>
    /// <param name="pageSize">Records per request, 1 to 10,000. Defaults to 1,000.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    IAsyncEnumerable<NormalTransaction> EnumerateTransactionsAsync(
        string address,
        BlockRangeQuery? query = null,
        int? pageSize = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets normal transactions filtered by sender and/or recipient (<c>txlist</c>, advanced filter).</summary>
    /// <param name="filter">Sender/recipient filter.</param>
    /// <param name="query">Block range, page and sort.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/advanced-filter-txlist"/>
    Task<IReadOnlyList<NormalTransaction>> GetTransactionsAdvancedFilterAsync(
        AddressFilter filter,
        BlockRangeQuery? query = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the internal transactions of an address (<c>txlistinternal</c>).</summary>
    /// <param name="address">The address.</param>
    /// <param name="query">Block range, page and sort.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/txlistinternal"/>
    Task<IReadOnlyList<InternalTransaction>> GetInternalTransactionsAsync(
        string address,
        BlockRangeQuery? query = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default);

    /// <summary>Streams each internal transaction of an address (<c>txlistinternal</c>). Stops with an <see cref="EtherscanApiException"/> if Etherscan's <c>page × offset</c> cap is reached; narrow the block range for long histories.</summary>
    /// <param name="address">The address.</param>
    /// <param name="query">Block range and sort; <see cref="BlockRangeQuery.Page"/> sets the first page.</param>
    /// <param name="pageSize">Records per request, 1 to 10,000. Defaults to 1,000.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    IAsyncEnumerable<InternalTransaction> EnumerateInternalTransactionsAsync(
        string address,
        BlockRangeQuery? query = null,
        int? pageSize = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the internal transactions executed within a transaction (<c>txlistinternal</c> by hash).</summary>
    /// <param name="transactionHash">The transaction hash.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/txlistinternal-txhash"/>
    Task<IReadOnlyList<InternalTransaction>> GetInternalTransactionsByHashAsync(
        string transactionHash,
        ChainId? chain = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets internal transactions within a block range (<c>txlistinternal</c> by block range).</summary>
    /// <remarks><b>PRO</b> endpoint (Standard plan and above).</remarks>
    /// <param name="startBlock">First block.</param>
    /// <param name="endBlock">Last block.</param>
    /// <param name="page">Page and sort.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/txlistinternal-blockrange"/>
    Task<IReadOnlyList<InternalTransaction>> GetInternalTransactionsByBlockRangeAsync(
        long startBlock,
        long endBlock,
        PageQuery? page = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets internal transactions filtered by sender and/or recipient (<c>txlistinternal</c>, advanced filter).</summary>
    /// <param name="filter">Sender/recipient filter.</param>
    /// <param name="query">Block range, page and sort.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/advanced-filter-txlistinternal"/>
    Task<IReadOnlyList<InternalTransaction>> GetInternalTransactionsAdvancedFilterAsync(
        AddressFilter filter,
        BlockRangeQuery? query = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets ERC-20 transfers of an address and/or token contract (<c>tokentx</c>).</summary>
    /// <param name="address">The address; optional when <paramref name="contractAddress"/> is set.</param>
    /// <param name="contractAddress">The token contract; optional when <paramref name="address"/> is set.</param>
    /// <param name="query">Block range, page and sort.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/tokentx"/>
    Task<IReadOnlyList<Erc20Transfer>> GetErc20TransfersAsync(
        string? address = null,
        string? contractAddress = null,
        BlockRangeQuery? query = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default);

    /// <summary>Streams each ERC-20 transfer of an address and/or token contract (<c>tokentx</c>). Stops with an <see cref="EtherscanApiException"/> if Etherscan's <c>page × offset</c> cap is reached; narrow the block range for long histories.</summary>
    /// <param name="address">The address; optional when <paramref name="contractAddress"/> is set.</param>
    /// <param name="contractAddress">The token contract; optional when <paramref name="address"/> is set.</param>
    /// <param name="query">Block range and sort; <see cref="BlockRangeQuery.Page"/> sets the first page.</param>
    /// <param name="pageSize">Records per request, 1 to 10,000. Defaults to 1,000.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    IAsyncEnumerable<Erc20Transfer> EnumerateErc20TransfersAsync(
        string? address = null,
        string? contractAddress = null,
        BlockRangeQuery? query = null,
        int? pageSize = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets ERC-20 transfers filtered by sender and/or recipient (<c>tokentx</c>, advanced filter).</summary>
    /// <param name="filter">Sender/recipient filter.</param>
    /// <param name="contractAddress">Optional token contract.</param>
    /// <param name="query">Block range, page and sort.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/advanced-filter-tokentx"/>
    Task<IReadOnlyList<Erc20Transfer>> GetErc20TransfersAdvancedFilterAsync(
        AddressFilter filter,
        string? contractAddress = null,
        BlockRangeQuery? query = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets ERC-721 transfers of an address and/or NFT contract (<c>tokennfttx</c>).</summary>
    /// <param name="address">The address; optional when <paramref name="contractAddress"/> is set.</param>
    /// <param name="contractAddress">The NFT contract; optional when <paramref name="address"/> is set.</param>
    /// <param name="query">Block range, page and sort.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/tokennfttx"/>
    Task<IReadOnlyList<Erc721Transfer>> GetErc721TransfersAsync(
        string? address = null,
        string? contractAddress = null,
        BlockRangeQuery? query = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default);

    /// <summary>Streams each ERC-721 transfer of an address and/or NFT contract (<c>tokennfttx</c>). Stops with an <see cref="EtherscanApiException"/> if Etherscan's <c>page × offset</c> cap is reached; narrow the block range for long histories.</summary>
    /// <param name="address">The address; optional when <paramref name="contractAddress"/> is set.</param>
    /// <param name="contractAddress">The NFT contract; optional when <paramref name="address"/> is set.</param>
    /// <param name="query">Block range and sort; <see cref="BlockRangeQuery.Page"/> sets the first page.</param>
    /// <param name="pageSize">Records per request, 1 to 10,000. Defaults to 1,000.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    IAsyncEnumerable<Erc721Transfer> EnumerateErc721TransfersAsync(
        string? address = null,
        string? contractAddress = null,
        BlockRangeQuery? query = null,
        int? pageSize = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets ERC-721 transfers filtered by sender and/or recipient (<c>tokennfttx</c>, advanced filter).</summary>
    /// <param name="filter">Sender/recipient filter.</param>
    /// <param name="contractAddress">Optional NFT contract.</param>
    /// <param name="query">Block range, page and sort.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/advanced-filter-tokennfttx"/>
    Task<IReadOnlyList<Erc721Transfer>> GetErc721TransfersAdvancedFilterAsync(
        AddressFilter filter,
        string? contractAddress = null,
        BlockRangeQuery? query = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets ERC-1155 transfers of an address and/or contract (<c>token1155tx</c>).</summary>
    /// <param name="address">The address; optional when <paramref name="contractAddress"/> is set.</param>
    /// <param name="contractAddress">The ERC-1155 contract; optional when <paramref name="address"/> is set.</param>
    /// <param name="query">Block range, page and sort.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/token1155tx"/>
    Task<IReadOnlyList<Erc1155Transfer>> GetErc1155TransfersAsync(
        string? address = null,
        string? contractAddress = null,
        BlockRangeQuery? query = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default);

    /// <summary>Streams each ERC-1155 transfer of an address and/or contract (<c>token1155tx</c>). Stops with an <see cref="EtherscanApiException"/> if Etherscan's <c>page × offset</c> cap is reached; narrow the block range for long histories.</summary>
    /// <param name="address">The address; optional when <paramref name="contractAddress"/> is set.</param>
    /// <param name="contractAddress">The ERC-1155 contract; optional when <paramref name="address"/> is set.</param>
    /// <param name="query">Block range and sort; <see cref="BlockRangeQuery.Page"/> sets the first page.</param>
    /// <param name="pageSize">Records per request, 1 to 10,000. Defaults to 1,000.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    IAsyncEnumerable<Erc1155Transfer> EnumerateErc1155TransfersAsync(
        string? address = null,
        string? contractAddress = null,
        BlockRangeQuery? query = null,
        int? pageSize = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets ERC-1155 transfers filtered by sender and/or recipient (<c>token1155tx</c>, advanced filter).</summary>
    /// <param name="filter">Sender/recipient filter.</param>
    /// <param name="contractAddress">Optional ERC-1155 contract.</param>
    /// <param name="query">Block range, page and sort.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/advanced-filter-token1155tx"/>
    Task<IReadOnlyList<Erc1155Transfer>> GetErc1155TransfersAdvancedFilterAsync(
        AddressFilter filter,
        string? contractAddress = null,
        BlockRangeQuery? query = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the blocks validated by an address (<c>getminedblocks</c>).</summary>
    /// <param name="address">The validator address.</param>
    /// <param name="blockType"><see cref="MinedBlockType.Blocks"/> (default) or <see cref="MinedBlockType.Uncles"/> (pre-Merge).</param>
    /// <param name="page">Pagination.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/getminedblocks"/>
    Task<IReadOnlyList<MinedBlock>> GetMinedBlocksAsync(
        string address,
        MinedBlockType? blockType = null,
        PageQuery? page = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets beacon chain withdrawals, optionally for one address (<c>txsBeaconWithdrawal</c>).</summary>
    /// <param name="address">Optional withdrawal recipient.</param>
    /// <param name="query">Block range, page and sort.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/txsbeaconwithdrawal"/>
    Task<IReadOnlyList<BeaconWithdrawal>> GetBeaconWithdrawalsAsync(
        string? address = null,
        BlockRangeQuery? query = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets the address and transaction that first funded an EOA (<c>fundedby</c>).</summary>
    /// <remarks><b>PRO</b> endpoint (Standard plan and above). Throttled to 2 calls/second. Not available for contracts.</remarks>
    /// <param name="address">An externally owned account.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/fundedby"/>
    Task<FundingSource> GetFundedByAsync(
        string address,
        ChainId? chain = null,
        CancellationToken cancellationToken = default);
}
