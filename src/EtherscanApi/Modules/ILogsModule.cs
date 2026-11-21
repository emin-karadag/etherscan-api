using System.Numerics;
using EtherscanApi.Models;

namespace EtherscanApi;

/// <summary>Event log endpoints (<c>module=logs</c>, <c>action=getLogs</c>).</summary>
public interface ILogsModule
{
    /// <summary>Gets event logs filtered by address and/or topics (<c>getLogs</c>).</summary>
    /// <remarks>
    /// Provide <see cref="LogQuery.Address"/>, at least one topic, or both. Keep the block range narrow to
    /// avoid timeouts. Covers the "by address", "by topics" and "by address and topics" variants.
    /// </remarks>
    /// <param name="query">The filter.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/getlogs"/>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/getlogs-topics"/>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/getlogs-address-topics"/>
    Task<IReadOnlyList<EventLog>> GetLogsAsync(LogQuery query, ChainId? chain = null, CancellationToken cancellationToken = default);

    /// <summary>Gets event logs emitted by a contract (<c>getLogs</c> by address).</summary>
    /// <param name="address">The emitting contract.</param>
    /// <param name="fromBlock">First block.</param>
    /// <param name="toBlock">Last block.</param>
    /// <param name="page">Optional page.</param>
    /// <param name="offset">Optional records per page (up to 1000).</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/getlogs"/>
    Task<IReadOnlyList<EventLog>> GetLogsByAddressAsync(
        string address,
        long fromBlock,
        long toBlock,
        int? page = null,
        int? offset = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default);

    /// <summary>Streams each matching event log, fetching pages on demand (<c>getLogs</c>). Stops with an <see cref="EtherscanApiException"/> if Etherscan's <c>page × offset</c> cap is reached; narrow the block range for long histories.</summary>
    /// <param name="query">The filter; <see cref="LogQuery.Page"/> sets the first page and <see cref="LogQuery.Offset"/> is replaced by <paramref name="pageSize"/>.</param>
    /// <param name="pageSize">Records per request, 1 to 1,000. Defaults to 1,000.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    IAsyncEnumerable<EventLog> EnumerateLogsAsync(
        LogQuery query,
        int? pageSize = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default);
}
