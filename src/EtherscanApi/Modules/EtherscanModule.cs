using System.Runtime.CompilerServices;
using EtherscanApi.Internal;
using EtherscanApi.Serialization;

namespace EtherscanApi;

/// <summary>Base class of the endpoint groups exposed by <see cref="EtherscanClient"/>.</summary>
internal abstract class EtherscanModule
{
    /// <summary>Records per request used by the <c>Enumerate…Async</c> methods when no page size is given.</summary>
    internal const int DefaultPageSize = 1000;

    /// <summary>The largest <c>offset</c> Etherscan accepts on history endpoints.</summary>
    internal const int MaxPageSize = 10_000;

    private protected EtherscanModule(EtherscanTransport transport)
    {
        Transport = transport;
    }

    private protected EtherscanTransport Transport { get; }

    private protected static EtherscanJsonContext Json => EtherscanJsonContext.Default;

    private protected EtherscanRequest Request(string module, string action, ChainId? chain, int? endpointRequestsPerSecond = null)
        => new(module, action, Transport.Resolve(chain)) { EndpointRequestsPerSecond = endpointRequestsPerSecond };

    /// <summary>Builds a <c>module=stats</c> daily statistics request (<c>startdate</c>, <c>enddate</c>, <c>sort</c>).</summary>
    private protected EtherscanRequest DailyRequest(string action, DateOnly startDate, DateOnly endDate, SortOrder? sort, ChainId? chain)
    {
        Guard.DateRange(startDate, endDate);
        return Request("stats", action, chain)
            .Add("startdate", WireFormat.Date(startDate))
            .Add("enddate", WireFormat.Date(endDate))
            .Add("sort", sort);
    }

    private protected static void AddRange(EtherscanRequest request, BlockRangeQuery? query)
    {
        if (query is null)
        {
            return;
        }

        Guard.Page(query.Page, query.Offset);
        request.Add("startblock", query.StartBlock)
            .Add("endblock", query.EndBlock)
            .Add("page", query.Page)
            .Add("offset", query.Offset)
            .Add("sort", query.Sort);
    }

    private protected static void AddPage(EtherscanRequest request, PageQuery? query, bool includeSort)
    {
        if (query is null)
        {
            return;
        }

        Guard.Page(query.Page, query.Offset);
        request.Add("page", query.Page).Add("offset", query.Offset);
        if (includeSort)
        {
            request.Add("sort", query.Sort);
        }
    }

    private protected static void AddFilter(EtherscanRequest request, AddressFilter filter)
    {
        ArgumentNullException.ThrowIfNull(filter);
        var from = Guard.OptionalAddress(filter.From, nameof(filter.From));
        var to = Guard.OptionalAddress(filter.To, nameof(filter.To));
        if (from is null && to is null)
        {
            throw new ArgumentException("Provide From, To, or both.", nameof(filter));
        }

        request.Add("from", from).Add("to", to).Add("fromto_opr", filter.Operator);
    }

    /// <summary>Validates a page size eagerly (at the call, not on the first <c>MoveNextAsync</c>).</summary>
    private protected static int ResolvePageSize(int? pageSize, int maxPageSize = MaxPageSize)
    {
        var size = pageSize ?? Math.Min(DefaultPageSize, maxPageSize);
        ArgumentOutOfRangeException.ThrowIfLessThan(size, 1, nameof(pageSize));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(size, maxPageSize, nameof(pageSize));
        return size;
    }

    /// <summary>
    /// Streams a block-range list endpoint page by page. Arguments are validated before the first request.
    /// <see cref="BlockRangeQuery.Page"/> sets the first page; <see cref="BlockRangeQuery.Offset"/> is replaced by the page size.
    /// </summary>
    private protected static IAsyncEnumerable<T> EnumerateRange<T>(
        BlockRangeQuery? query,
        int? pageSize,
        Func<BlockRangeQuery, CancellationToken, Task<IReadOnlyList<T>>> fetchPage,
        CancellationToken cancellationToken)
    {
        var size = ResolvePageSize(pageSize);
        var baseQuery = query ?? new BlockRangeQuery();
        Guard.Page(baseQuery.Page, offset: null);
        return PaginateAsync(
            (page, ct) => fetchPage(baseQuery with { Page = page, Offset = size }, ct),
            baseQuery.Page ?? 1,
            size,
            cancellationToken);
    }

    /// <summary>
    /// Pages through a list endpoint until a page shorter than <paramref name="pageSize"/> is returned.
    /// Callers validate <paramref name="firstPage"/> and <paramref name="pageSize"/> eagerly.
    /// </summary>
    private protected static async IAsyncEnumerable<T> PaginateAsync<T>(
        Func<int, CancellationToken, Task<IReadOnlyList<T>>> fetchPage,
        int firstPage,
        int pageSize,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        for (var page = firstPage; ; page++)
        {
            var items = await fetchPage(page, cancellationToken).ConfigureAwait(false);
            foreach (var item in items)
            {
                yield return item;
            }

            if (items.Count < pageSize)
            {
                yield break;
            }
        }
    }
}
