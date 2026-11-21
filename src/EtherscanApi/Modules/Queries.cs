namespace EtherscanApi;

/// <summary>
/// Block range, pagination and sort options for list endpoints.
/// </summary>
/// <remarks>
/// Etherscan recommends requesting only what you need: a small <see cref="Offset"/> and a known block range
/// are much faster (see <see href="https://docs.etherscan.io/resources/best-practices">Best Practices</see>).
/// Etherscan caps <c>page × offset</c> for history endpoints; for long histories, narrow the block range.
/// </remarks>
public sealed record BlockRangeQuery
{
    /// <summary>First block to include (<c>startblock</c>).</summary>
    public long? StartBlock { get; init; }

    /// <summary>Last block to include (<c>endblock</c>).</summary>
    public long? EndBlock { get; init; }

    /// <summary>1-based page number (<c>page</c>).</summary>
    public int? Page { get; init; }

    /// <summary>Records per page (<c>offset</c>).</summary>
    public int? Offset { get; init; }

    /// <summary>Sort order (<c>sort</c>).</summary>
    public SortOrder? Sort { get; init; }
}

/// <summary>Pagination options.</summary>
public sealed record PageQuery
{
    /// <summary>1-based page number (<c>page</c>).</summary>
    public int? Page { get; init; }

    /// <summary>Records per page (<c>offset</c>).</summary>
    public int? Offset { get; init; }

    /// <summary>Sort order (<c>sort</c>), for endpoints that support it.</summary>
    public SortOrder? Sort { get; init; }
}

/// <summary>
/// Sender/recipient filter for the "advanced filter" variants of the transaction and transfer endpoints
/// (<c>from</c>, <c>to</c>, <c>fromto_opr</c>).
/// </summary>
/// <param name="From">Sender address to match.</param>
/// <param name="To">Recipient address to match.</param>
/// <param name="Operator">
/// <see cref="FilterOperator.And"/> when both addresses must match, <see cref="FilterOperator.Or"/> when either may.
/// </param>
public sealed record AddressFilter(string? From = null, string? To = null, FilterOperator Operator = FilterOperator.And);

/// <summary>
/// Filter for <c>getLogs</c>: an emitting address and/or up to four topics, combined with per-pair operators.
/// </summary>
/// <remarks>Keep the block range narrow to avoid query timeouts and large responses.</remarks>
public sealed record LogQuery
{
    /// <summary>First block (<c>fromBlock</c>). Required.</summary>
    public required long FromBlock { get; init; }

    /// <summary>Last block (<c>toBlock</c>). Required.</summary>
    public required long ToBlock { get; init; }

    /// <summary>Emitting contract address (<c>address</c>).</summary>
    public string? Address { get; init; }

    /// <summary>First topic, usually the event signature hash (<c>topic0</c>).</summary>
    public string? Topic0 { get; init; }

    /// <summary>Second topic (<c>topic1</c>).</summary>
    public string? Topic1 { get; init; }

    /// <summary>Third topic (<c>topic2</c>).</summary>
    public string? Topic2 { get; init; }

    /// <summary>Fourth topic (<c>topic3</c>).</summary>
    public string? Topic3 { get; init; }

    /// <summary>Operator between topic0 and topic1 (<c>topic0_1_opr</c>).</summary>
    public FilterOperator? Topic0And1 { get; init; }

    /// <summary>Operator between topic1 and topic2 (<c>topic1_2_opr</c>).</summary>
    public FilterOperator? Topic1And2 { get; init; }

    /// <summary>Operator between topic2 and topic3 (<c>topic2_3_opr</c>).</summary>
    public FilterOperator? Topic2And3 { get; init; }

    /// <summary>Operator between topic0 and topic2 (<c>topic0_2_opr</c>).</summary>
    public FilterOperator? Topic0And2 { get; init; }

    /// <summary>Operator between topic0 and topic3 (<c>topic0_3_opr</c>).</summary>
    public FilterOperator? Topic0And3 { get; init; }

    /// <summary>Operator between topic1 and topic3 (<c>topic1_3_opr</c>).</summary>
    public FilterOperator? Topic1And3 { get; init; }

    /// <summary>1-based page number (<c>page</c>).</summary>
    public int? Page { get; init; }

    /// <summary>Records per page (<c>offset</c>), up to 1000.</summary>
    public int? Offset { get; init; }
}
