using EtherscanApi.Internal;
using EtherscanApi.Models;

namespace EtherscanApi;

/// <summary>Event log endpoints (<c>module=logs</c>, <c>action=getLogs</c>).</summary>
internal sealed class LogsModule : EtherscanModule, ILogsModule
{
    internal LogsModule(EtherscanTransport transport)
        : base(transport)
    {
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<EventLog>> GetLogsAsync(LogQuery query, ChainId? chain = null, CancellationToken cancellationToken = default)
        => Transport.SendListAsync(BuildRequest(query, chain), Json.ListEventLog, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<EventLog>> GetLogsByAddressAsync(
        string address,
        long fromBlock,
        long toBlock,
        int? page = null,
        int? offset = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default)
        => GetLogsAsync(new LogQuery { Address = address, FromBlock = fromBlock, ToBlock = toBlock, Page = page, Offset = offset }, chain, cancellationToken);

    /// <inheritdoc />
    public IAsyncEnumerable<EventLog> EnumerateLogsAsync(
        LogQuery query,
        int? pageSize = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default)
    {
        ValidateLogQuery(query);
        var size = ResolvePageSize(pageSize, MaxLogsPageSize);
        return PaginateAsync(
            (page, ct) => GetLogsAsync(query with { Page = page, Offset = size }, chain, ct),
            query.Page ?? 1,
            size,
            cancellationToken);
    }

    /// <summary>getLogs returns at most 1,000 records per page.</summary>
    private const int MaxLogsPageSize = 1000;

    private EtherscanRequest BuildRequest(LogQuery query, ChainId? chain)
    {
        var (address, topic0, topic1, topic2, topic3) = ValidateLogQuery(query);
        return Request("logs", "getLogs", chain)
            .Add("fromBlock", query.FromBlock)
            .Add("toBlock", query.ToBlock)
            .Add("address", address)
            .Add("topic0", topic0)
            .Add("topic0_1_opr", Both(topic0, topic1) ? query.Topic0And1 : null)
            .Add("topic1", topic1)
            .Add("topic1_2_opr", Both(topic1, topic2) ? query.Topic1And2 : null)
            .Add("topic2", topic2)
            .Add("topic2_3_opr", Both(topic2, topic3) ? query.Topic2And3 : null)
            .Add("topic3", topic3)
            .Add("topic0_2_opr", Both(topic0, topic2) ? query.Topic0And2 : null)
            .Add("topic0_3_opr", Both(topic0, topic3) ? query.Topic0And3 : null)
            .Add("topic1_3_opr", Both(topic1, topic3) ? query.Topic1And3 : null)
            .Add("page", query.Page)
            .Add("offset", query.Offset);
    }

    /// <summary>Validates the filter and returns the normalized address and topics.</summary>
    private static (string? Address, string? Topic0, string? Topic1, string? Topic2, string? Topic3) ValidateLogQuery(LogQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentOutOfRangeException.ThrowIfNegative(query.FromBlock, nameof(query));
        if (query.ToBlock < query.FromBlock)
        {
            throw new ArgumentException("ToBlock must not be lower than FromBlock.", nameof(query));
        }

        var address = Guard.OptionalAddress(query.Address, nameof(query.Address));
        var topic0 = Guard.OptionalHash(query.Topic0, nameof(query.Topic0));
        var topic1 = Guard.OptionalHash(query.Topic1, nameof(query.Topic1));
        var topic2 = Guard.OptionalHash(query.Topic2, nameof(query.Topic2));
        var topic3 = Guard.OptionalHash(query.Topic3, nameof(query.Topic3));
        if (address is null && topic0 is null && topic1 is null && topic2 is null && topic3 is null)
        {
            throw new ArgumentException("Provide an address, at least one topic, or both.", nameof(query));
        }

        Guard.Page(query.Page, query.Offset);
        return (address, topic0, topic1, topic2, topic3);
    }

    private static bool Both(string? a, string? b) => a is not null && b is not null;
}
