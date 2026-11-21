namespace EtherscanApi;

/// <summary>Sort order for list endpoints (<c>sort</c> parameter).</summary>
public enum SortOrder
{
    /// <summary>Oldest first (<c>asc</c>).</summary>
    Ascending,

    /// <summary>Latest first (<c>desc</c>).</summary>
    Descending,
}

/// <summary>Logical operator for combining filters (<c>fromto_opr</c>, <c>topicX_Y_opr</c>).</summary>
public enum FilterOperator
{
    /// <summary>Both conditions must match (<c>and</c>).</summary>
    And,

    /// <summary>Either condition can match (<c>or</c>).</summary>
    Or,
}

/// <summary>Which block to pick when a timestamp falls between blocks (<c>closest</c>).</summary>
public enum ClosestBlock
{
    /// <summary>The closest block before the timestamp (<c>before</c>).</summary>
    Before,

    /// <summary>The closest block after the timestamp (<c>after</c>).</summary>
    After,
}

/// <summary>Block type for validated-block queries (<c>blocktype</c>).</summary>
public enum MinedBlockType
{
    /// <summary>Full blocks (<c>blocks</c>).</summary>
    Blocks,

    /// <summary>Uncle blocks, pre-Merge only (<c>uncles</c>).</summary>
    Uncles,
}

/// <summary>Node client for chain size statistics (<c>clienttype</c>).</summary>
public enum NodeClientType
{
    /// <summary>Geth (<c>geth</c>).</summary>
    Geth,

    /// <summary>Parity (<c>parity</c>).</summary>
    Parity,
}

/// <summary>Node sync mode for chain size statistics (<c>syncmode</c>).</summary>
public enum NodeSyncMode
{
    /// <summary>Default node (<c>default</c>).</summary>
    Default,

    /// <summary>Archive node (<c>archive</c>).</summary>
    Archive,
}

/// <summary>Export file format for address tag exports (<c>format</c>).</summary>
public enum ExportFormat
{
    /// <summary>CSV file (<c>csv</c>).</summary>
    Csv,

    /// <summary>ZIP archive (<c>zip</c>). Only supported by the multichain (V2) export.</summary>
    Zip,
}

/// <summary>
/// Etherscan API plan tier. Selects the client-side rate-limit preset.
/// </summary>
/// <remarks>See <see href="https://docs.etherscan.io/rate-limits">Rate Limits</see>.</remarks>
public enum EtherscanPlan
{
    /// <summary>Free: 3 calls/second, 100,000 calls/day, selected chains only, no PRO endpoints.</summary>
    Free,

    /// <summary>Lite: 5 calls/second, 100,000 calls/day, no PRO endpoints.</summary>
    Lite,

    /// <summary>Standard: 10 calls/second, 200,000 calls/day, PRO endpoints.</summary>
    Standard,

    /// <summary>Advanced: 20 calls/second, 500,000 calls/day, PRO endpoints.</summary>
    Advanced,

    /// <summary>Professional: 30 calls/second, 1,000,000 calls/day, PRO endpoints.</summary>
    Professional,

    /// <summary>Pro Plus: 30 calls/second, 1,500,000 calls/day, PRO and PRO+ endpoints.</summary>
    ProPlus,

    /// <summary>Dedicated or custom plan. Set <see cref="EtherscanClientOptions.RequestsPerSecond"/> explicitly.</summary>
    Custom,
}
