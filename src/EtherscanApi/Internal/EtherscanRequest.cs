using System.Globalization;
using System.Numerics;
using System.Text;

namespace EtherscanApi.Internal;

internal enum EtherscanHost
{
    Api,
    Metadata,
}

/// <summary>
/// Describes one Etherscan call: route, module/action, parameters, and retry/throttle traits.
/// Parameter names are sent exactly as documented (for example <c>fromBlock</c>, <c>topic0_1_opr</c>).
/// </summary>
internal sealed class EtherscanRequest
{
    private readonly List<KeyValuePair<string, string>> _query = [];
    private List<KeyValuePair<string, string>>? _form;

    public EtherscanRequest(string module, string action, ChainId? chain)
    {
        Module = module;
        Action = action;
        Chain = chain;
    }

    public string Module { get; }

    public string Action { get; }

    /// <summary>The chain, or <see langword="null"/> for endpoints that take no <c>chainid</c>.</summary>
    public ChainId? Chain { get; }

    public EtherscanHost Host { get; init; } = EtherscanHost.Api;

    /// <summary>Path relative to the host. V2 endpoints use <c>v2/api</c>.</summary>
    public string Path { get; init; } = "v2/api";

    /// <summary>Whether <c>module</c> and <c>action</c> are sent (the chain list route takes neither).</summary>
    public bool IncludeModuleAndAction { get; init; } = true;

    /// <summary>Whether the request can safely be retried. False for submissions and broadcasts.</summary>
    public bool IsIdempotent { get; init; } = true;

    /// <summary>A documented per-endpoint limit in calls per second, applied on top of the plan limit.</summary>
    public int? EndpointRequestsPerSecond { get; init; }

    public bool IsPost => _form is not null;

    public EtherscanRequest Add(string name, string? value)
    {
        if (value is not null)
        {
            _query.Add(new(name, value));
        }

        return this;
    }

    public EtherscanRequest Add(string name, long? value)
        => Add(name, value?.ToString(CultureInfo.InvariantCulture));

    public EtherscanRequest Add(string name, int? value)
        => Add(name, value?.ToString(CultureInfo.InvariantCulture));

    public EtherscanRequest Add(string name, BigInteger? value)
        => Add(name, value?.ToString(CultureInfo.InvariantCulture));

    public EtherscanRequest Add(string name, SortOrder? value)
        => Add(name, value is null ? null : WireFormat.Sort(value.Value));

    public EtherscanRequest Add(string name, FilterOperator? value)
        => Add(name, value is null ? null : WireFormat.Operator(value.Value));

    /// <summary>Marks the request as a form-encoded POST.</summary>
    public EtherscanRequest AsFormPost()
    {
        _form ??= [];
        return this;
    }

    public EtherscanRequest AddForm(string name, string? value)
    {
        _form ??= [];
        if (value is not null)
        {
            _form.Add(new(name, value));
        }

        return this;
    }

    public EtherscanRequest AddForm(string name, int? value)
        => AddForm(name, value?.ToString(CultureInfo.InvariantCulture));

    /// <summary>Builds the relative request URI. The API key is added last, and only for GET requests.</summary>
    public string BuildRelativeUri(string? apiKey)
        => IsPost || string.IsNullOrEmpty(apiKey) ? BuildUriWithoutKey() : BuildUriWithoutKey() + Separator + "apikey=" + Uri.EscapeDataString(apiKey);

    /// <summary>Builds the relative request URI with the API key masked, safe for logs and exceptions.</summary>
    public string BuildRedactedUri() => IsPost ? BuildUriWithoutKey() : BuildUriWithoutKey() + Separator + "apikey=***";

    // Every Etherscan route carries at least one query parameter except the chain list, which takes none.
    private char Separator => Chain is not null || IncludeModuleAndAction || _query.Count > 0 ? '&' : '?';

    /// <summary>Builds the form body. For POST requests the API key travels in the body, as documented.</summary>
    public FormUrlEncodedContent BuildFormContent(string? apiKey)
    {
        var fields = new List<KeyValuePair<string, string>>((_form?.Count ?? 0) + 1);
        if (!string.IsNullOrEmpty(apiKey))
        {
            fields.Add(new("apikey", apiKey));
        }

        if (_form is not null)
        {
            fields.AddRange(_form);
        }

        return new FormUrlEncodedContent(fields);
    }

    private string BuildUriWithoutKey()
    {
        var sb = new StringBuilder(Path);
        var first = true;

        void Append(string name, string value)
        {
            sb.Append(first ? '?' : '&');
            first = false;
            sb.Append(Uri.EscapeDataString(name)).Append('=').Append(Uri.EscapeDataString(value));
        }

        if (Chain is { } chain)
        {
            Append("chainid", chain.ToString());
        }

        if (IncludeModuleAndAction)
        {
            Append("module", Module);
            Append("action", Action);
        }

        foreach (var (name, value) in _query)
        {
            Append(name, value);
        }

        return sb.ToString();
    }
}

internal static class WireFormat
{
    public static string Sort(SortOrder value) => value switch
    {
        SortOrder.Ascending => "asc",
        SortOrder.Descending => "desc",
        _ => throw new ArgumentOutOfRangeException(nameof(value)),
    };

    public static string Operator(FilterOperator value) => value switch
    {
        FilterOperator.And => "and",
        FilterOperator.Or => "or",
        _ => throw new ArgumentOutOfRangeException(nameof(value)),
    };

    public static string Closest(ClosestBlock value) => value switch
    {
        ClosestBlock.Before => "before",
        ClosestBlock.After => "after",
        _ => throw new ArgumentOutOfRangeException(nameof(value)),
    };

    public static string BlockType(MinedBlockType value) => value switch
    {
        MinedBlockType.Blocks => "blocks",
        MinedBlockType.Uncles => "uncles",
        _ => throw new ArgumentOutOfRangeException(nameof(value)),
    };

    public static string ClientType(NodeClientType value) => value switch
    {
        NodeClientType.Geth => "geth",
        NodeClientType.Parity => "parity",
        _ => throw new ArgumentOutOfRangeException(nameof(value)),
    };

    public static string SyncMode(NodeSyncMode value) => value switch
    {
        NodeSyncMode.Default => "default",
        NodeSyncMode.Archive => "archive",
        _ => throw new ArgumentOutOfRangeException(nameof(value)),
    };

    public static string Format(ExportFormat value) => value switch
    {
        ExportFormat.Csv => "csv",
        ExportFormat.Zip => "zip",
        _ => throw new ArgumentOutOfRangeException(nameof(value)),
    };

    public static string Date(DateOnly value) => value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    public static string Hex(ulong value) => "0x" + value.ToString("x", CultureInfo.InvariantCulture);

    public static string Hex(BigInteger value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value);
        if (value.IsZero)
        {
            return "0x0";
        }

        // BigInteger hex output may start with a sign nibble "0"; trim leading zeros for a canonical quantity.
        return "0x" + value.ToString("x", CultureInfo.InvariantCulture).TrimStart('0');
    }
}
