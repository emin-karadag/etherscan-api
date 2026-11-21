using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EtherscanApi.Models;

/// <summary>Fields shared by blocks returned from <c>eth_getBlockByNumber</c> and <c>eth_getUncleByBlockNumberAndIndex</c>.</summary>
public abstract record RpcBlockBase
{
    /// <summary>Block number.</summary>
    [JsonPropertyName("number")]
    public long? Number { get; init; }

    /// <summary>Block hash.</summary>
    [JsonPropertyName("hash")]
    public string? Hash { get; init; }

    /// <summary>Parent hash.</summary>
    [JsonPropertyName("parentHash")]
    public string? ParentHash { get; init; }

    /// <summary>Proof-of-work nonce (zero after the Merge).</summary>
    [JsonPropertyName("nonce")]
    public string? Nonce { get; init; }

    /// <summary>Uncles hash.</summary>
    [JsonPropertyName("sha3Uncles")]
    public string? Sha3Uncles { get; init; }

    /// <summary>Logs bloom filter.</summary>
    [JsonPropertyName("logsBloom")]
    public string? LogsBloom { get; init; }

    /// <summary>Transactions trie root.</summary>
    [JsonPropertyName("transactionsRoot")]
    public string? TransactionsRoot { get; init; }

    /// <summary>State trie root.</summary>
    [JsonPropertyName("stateRoot")]
    public string? StateRoot { get; init; }

    /// <summary>Receipts trie root.</summary>
    [JsonPropertyName("receiptsRoot")]
    public string? ReceiptsRoot { get; init; }

    /// <summary>Fee recipient.</summary>
    [JsonPropertyName("miner")]
    public string? Miner { get; init; }

    /// <summary>Mix hash (prevRandao after the Merge).</summary>
    [JsonPropertyName("mixHash")]
    public string? MixHash { get; init; }

    /// <summary>Difficulty.</summary>
    [JsonPropertyName("difficulty")]
    public BigInteger? Difficulty { get; init; }

    /// <summary>Total difficulty, when returned.</summary>
    [JsonPropertyName("totalDifficulty")]
    public BigInteger? TotalDifficulty { get; init; }

    /// <summary>Extra data.</summary>
    [JsonPropertyName("extraData")]
    public string? ExtraData { get; init; }

    /// <summary>Block size in bytes.</summary>
    [JsonPropertyName("size")]
    public long? Size { get; init; }

    /// <summary>Gas limit.</summary>
    [JsonPropertyName("gasLimit")]
    public BigInteger? GasLimit { get; init; }

    /// <summary>Gas used.</summary>
    [JsonPropertyName("gasUsed")]
    public BigInteger? GasUsed { get; init; }

    /// <summary>Block timestamp.</summary>
    [JsonPropertyName("timestamp")]
    public DateTimeOffset? Timestamp { get; init; }

    /// <summary>Base fee per gas (EIP-1559), wei.</summary>
    [JsonPropertyName("baseFeePerGas")]
    public BigInteger? BaseFeePerGas { get; init; }

    /// <summary>Withdrawals root (post-Shanghai).</summary>
    [JsonPropertyName("withdrawalsRoot")]
    public string? WithdrawalsRoot { get; init; }

    /// <summary>Blob gas used (EIP-4844).</summary>
    [JsonPropertyName("blobGasUsed")]
    public BigInteger? BlobGasUsed { get; init; }

    /// <summary>Excess blob gas (EIP-4844).</summary>
    [JsonPropertyName("excessBlobGas")]
    public BigInteger? ExcessBlobGas { get; init; }

    /// <summary>Parent beacon block root (EIP-4788).</summary>
    [JsonPropertyName("parentBeaconBlockRoot")]
    public string? ParentBeaconBlockRoot { get; init; }

    /// <summary>Uncle hashes.</summary>
    [JsonPropertyName("uncles")]
    public IReadOnlyList<string> Uncles { get; init; } = [];

    /// <summary>Beacon chain withdrawals (post-Shanghai).</summary>
    [JsonPropertyName("withdrawals")]
    public IReadOnlyList<RpcWithdrawal>? Withdrawals { get; init; }
}

/// <summary>A block whose <see cref="Transactions"/> are hashes (<c>boolean=false</c>), or an uncle.</summary>
public sealed record RpcBlock : RpcBlockBase
{
    /// <summary>Transaction hashes.</summary>
    [JsonPropertyName("transactions")]
    public IReadOnlyList<string> Transactions { get; init; } = [];
}

/// <summary>A block with full transaction objects (<c>boolean=true</c>).</summary>
public sealed record RpcBlockWithTransactions : RpcBlockBase
{
    /// <summary>Transactions.</summary>
    [JsonPropertyName("transactions")]
    public IReadOnlyList<RpcTransaction> Transactions { get; init; } = [];
}

/// <summary>A beacon chain withdrawal inside a block.</summary>
public sealed record RpcWithdrawal
{
    /// <summary>Withdrawal index.</summary>
    [JsonPropertyName("index")]
    public long Index { get; init; }

    /// <summary>Validator index.</summary>
    [JsonPropertyName("validatorIndex")]
    public long ValidatorIndex { get; init; }

    /// <summary>Recipient.</summary>
    [JsonPropertyName("address")]
    public string? Address { get; init; }

    /// <summary>Amount, gwei.</summary>
    [JsonPropertyName("amount")]
    public BigInteger Amount { get; init; }
}

/// <summary>A transaction returned by the proxy endpoints.</summary>
public sealed record RpcTransaction
{
    /// <summary>Block hash; <see langword="null"/> while pending.</summary>
    [JsonPropertyName("blockHash")]
    public string? BlockHash { get; init; }

    /// <summary>Block number; <see langword="null"/> while pending.</summary>
    [JsonPropertyName("blockNumber")]
    public long? BlockNumber { get; init; }

    /// <summary>Sender.</summary>
    [JsonPropertyName("from")]
    public string? From { get; init; }

    /// <summary>Gas limit.</summary>
    [JsonPropertyName("gas")]
    public BigInteger Gas { get; init; }

    /// <summary>Gas price, wei.</summary>
    [JsonPropertyName("gasPrice")]
    public BigInteger? GasPrice { get; init; }

    /// <summary>Max fee per gas (EIP-1559), wei.</summary>
    [JsonPropertyName("maxFeePerGas")]
    public BigInteger? MaxFeePerGas { get; init; }

    /// <summary>Max priority fee per gas (EIP-1559), wei.</summary>
    [JsonPropertyName("maxPriorityFeePerGas")]
    public BigInteger? MaxPriorityFeePerGas { get; init; }

    /// <summary>Max fee per blob gas (EIP-4844), wei.</summary>
    [JsonPropertyName("maxFeePerBlobGas")]
    public BigInteger? MaxFeePerBlobGas { get; init; }

    /// <summary>Transaction hash.</summary>
    [JsonPropertyName("hash")]
    public string? Hash { get; init; }

    /// <summary>Call data.</summary>
    [JsonPropertyName("input")]
    public string? Input { get; init; }

    /// <summary>Sender nonce.</summary>
    [JsonPropertyName("nonce")]
    public BigInteger Nonce { get; init; }

    /// <summary>Recipient; <see langword="null"/> for contract creations.</summary>
    [JsonPropertyName("to")]
    public string? To { get; init; }

    /// <summary>Position in the block.</summary>
    [JsonPropertyName("transactionIndex")]
    public long? TransactionIndex { get; init; }

    /// <summary>Value, wei.</summary>
    [JsonPropertyName("value")]
    public BigInteger Value { get; init; }

    /// <summary>Transaction type (0 legacy, 1 access list, 2 EIP-1559, 3 blob, 4 set-code).</summary>
    [JsonPropertyName("type")]
    public int? Type { get; init; }

    /// <summary>Access list.</summary>
    [JsonPropertyName("accessList")]
    public IReadOnlyList<RpcAccessListEntry>? AccessList { get; init; }

    /// <summary>Chain id.</summary>
    [JsonPropertyName("chainId")]
    public BigInteger? ChainId { get; init; }

    /// <summary>Signature V.</summary>
    [JsonPropertyName("v")]
    public string? V { get; init; }

    /// <summary>Signature R.</summary>
    [JsonPropertyName("r")]
    public string? R { get; init; }

    /// <summary>Signature S.</summary>
    [JsonPropertyName("s")]
    public string? S { get; init; }

    /// <summary>Signature y-parity.</summary>
    [JsonPropertyName("yParity")]
    public string? YParity { get; init; }
}

/// <summary>An access list entry.</summary>
public sealed record RpcAccessListEntry
{
    /// <summary>Accessed address.</summary>
    [JsonPropertyName("address")]
    public string? Address { get; init; }

    /// <summary>Accessed storage keys.</summary>
    [JsonPropertyName("storageKeys")]
    public IReadOnlyList<string> StorageKeys { get; init; } = [];
}

/// <summary>A transaction receipt (<c>eth_getTransactionReceipt</c>).</summary>
public sealed record RpcTransactionReceipt
{
    /// <summary>Block hash.</summary>
    [JsonPropertyName("blockHash")]
    public string? BlockHash { get; init; }

    /// <summary>Block number.</summary>
    [JsonPropertyName("blockNumber")]
    public long? BlockNumber { get; init; }

    /// <summary>Created contract address, for contract creations.</summary>
    [JsonPropertyName("contractAddress")]
    public string? ContractAddress { get; init; }

    /// <summary>Cumulative gas used in the block.</summary>
    [JsonPropertyName("cumulativeGasUsed")]
    public BigInteger CumulativeGasUsed { get; init; }

    /// <summary>Effective gas price, wei.</summary>
    [JsonPropertyName("effectiveGasPrice")]
    public BigInteger? EffectiveGasPrice { get; init; }

    /// <summary>Sender.</summary>
    [JsonPropertyName("from")]
    public string? From { get; init; }

    /// <summary>Gas used.</summary>
    [JsonPropertyName("gasUsed")]
    public BigInteger GasUsed { get; init; }

    /// <summary>Logs emitted.</summary>
    [JsonPropertyName("logs")]
    public IReadOnlyList<RpcLog> Logs { get; init; } = [];

    /// <summary>Logs bloom.</summary>
    [JsonPropertyName("logsBloom")]
    public string? LogsBloom { get; init; }

    /// <summary><see langword="true"/> on success (<c>0x1</c>); <see langword="null"/> for pre-Byzantium receipts.</summary>
    [JsonPropertyName("status")]
    public bool? Status { get; init; }

    /// <summary>Recipient.</summary>
    [JsonPropertyName("to")]
    public string? To { get; init; }

    /// <summary>Transaction hash.</summary>
    [JsonPropertyName("transactionHash")]
    public string? TransactionHash { get; init; }

    /// <summary>Position in the block.</summary>
    [JsonPropertyName("transactionIndex")]
    public long? TransactionIndex { get; init; }

    /// <summary>Transaction type.</summary>
    [JsonPropertyName("type")]
    public int? Type { get; init; }
}

/// <summary>A log in a transaction receipt.</summary>
public sealed record RpcLog
{
    /// <summary>Emitting contract.</summary>
    [JsonPropertyName("address")]
    public string? Address { get; init; }

    /// <summary>Indexed topics.</summary>
    [JsonPropertyName("topics")]
    public IReadOnlyList<string> Topics { get; init; } = [];

    /// <summary>Non-indexed data.</summary>
    [JsonPropertyName("data")]
    public string? Data { get; init; }

    /// <summary>Block number.</summary>
    [JsonPropertyName("blockNumber")]
    public long? BlockNumber { get; init; }

    /// <summary>Transaction hash.</summary>
    [JsonPropertyName("transactionHash")]
    public string? TransactionHash { get; init; }

    /// <summary>Transaction index.</summary>
    [JsonPropertyName("transactionIndex")]
    public long? TransactionIndex { get; init; }

    /// <summary>Block hash.</summary>
    [JsonPropertyName("blockHash")]
    public string? BlockHash { get; init; }

    /// <summary>Log index in the block.</summary>
    [JsonPropertyName("logIndex")]
    public long? LogIndex { get; init; }

    /// <summary>Whether the log was removed by a reorg.</summary>
    [JsonPropertyName("removed")]
    public bool Removed { get; init; }
}

/// <summary>An event log (<c>getLogs</c>). Etherscan returns hex quantities here; they are decoded.</summary>
public sealed record EventLog
{
    /// <summary>Emitting contract.</summary>
    [JsonPropertyName("address")]
    public string Address { get; init; } = "";

    /// <summary>Indexed topics (topic0 is the event signature hash).</summary>
    [JsonPropertyName("topics")]
    public IReadOnlyList<string> Topics { get; init; } = [];

    /// <summary>Non-indexed data.</summary>
    [JsonPropertyName("data")]
    public string? Data { get; init; }

    /// <summary>Block number.</summary>
    [JsonPropertyName("blockNumber")]
    public long BlockNumber { get; init; }

    /// <summary>Block hash.</summary>
    [JsonPropertyName("blockHash")]
    public string? BlockHash { get; init; }

    /// <summary>Block timestamp.</summary>
    [JsonPropertyName("timeStamp")]
    public DateTimeOffset TimeStamp { get; init; }

    /// <summary>Gas price, wei.</summary>
    [JsonPropertyName("gasPrice")]
    public BigInteger GasPrice { get; init; }

    /// <summary>Gas used by the transaction.</summary>
    [JsonPropertyName("gasUsed")]
    public BigInteger GasUsed { get; init; }

    /// <summary>Log index in the block.</summary>
    [JsonPropertyName("logIndex")]
    public long LogIndex { get; init; }

    /// <summary>Transaction hash.</summary>
    [JsonPropertyName("transactionHash")]
    public string TransactionHash { get; init; } = "";

    /// <summary>Transaction index.</summary>
    [JsonPropertyName("transactionIndex")]
    public long TransactionIndex { get; init; }
}

/// <summary>A Plasma (Polygon PoS bridge) deposit (<c>txnbridge</c>).</summary>
public sealed record PlasmaDeposit
{
    /// <summary>Transaction hash.</summary>
    [JsonPropertyName("hash")]
    public string Hash { get; init; } = "";

    /// <summary>Block number.</summary>
    [JsonPropertyName("blockNumber")]
    public long BlockNumber { get; init; }

    /// <summary>Timestamp.</summary>
    [JsonPropertyName("timeStamp")]
    public DateTimeOffset TimeStamp { get; init; }

    /// <summary>Sender.</summary>
    [JsonPropertyName("from")]
    public string? From { get; init; }

    /// <summary>Receiving address.</summary>
    [JsonPropertyName("address")]
    public string? Address { get; init; }

    /// <summary>Amount in the token's smallest unit.</summary>
    [JsonPropertyName("amount")]
    public BigInteger Amount { get; init; }

    /// <summary>Token name.</summary>
    [JsonPropertyName("tokenName")]
    public string? TokenName { get; init; }

    /// <summary>Token symbol.</summary>
    [JsonPropertyName("symbol")]
    public string? Symbol { get; init; }

    /// <summary>Token contract.</summary>
    [JsonPropertyName("contractAddress")]
    public string? ContractAddress { get; init; }

    /// <summary>Token decimals.</summary>
    [JsonPropertyName("divisor")]
    public int? Divisor { get; init; }
}

/// <summary>Fields shared by L2 deposit and withdrawal transactions.</summary>
public abstract record L2BridgeTransactionBase
{
    /// <summary>Block number.</summary>
    [JsonPropertyName("blockNumber")]
    public long BlockNumber { get; init; }

    /// <summary>Timestamp.</summary>
    [JsonPropertyName("timeStamp")]
    public DateTimeOffset TimeStamp { get; init; }

    /// <summary>Block hash.</summary>
    [JsonPropertyName("blockHash")]
    public string? BlockHash { get; init; }

    /// <summary>Transaction hash.</summary>
    [JsonPropertyName("hash")]
    public string Hash { get; init; } = "";

    /// <summary>Nonce.</summary>
    [JsonPropertyName("nonce")]
    public long Nonce { get; init; }

    /// <summary>Sender.</summary>
    [JsonPropertyName("from")]
    public string? From { get; init; }

    /// <summary>Recipient.</summary>
    [JsonPropertyName("to")]
    public string? To { get; init; }

    /// <summary>Value, wei.</summary>
    [JsonPropertyName("value")]
    public BigInteger Value { get; init; }

    /// <summary>Gas limit.</summary>
    [JsonPropertyName("gas")]
    public long Gas { get; init; }

    /// <summary>Gas price, wei.</summary>
    [JsonPropertyName("gasPrice")]
    public BigInteger GasPrice { get; init; }

    /// <summary>Call data.</summary>
    [JsonPropertyName("input")]
    public string? Input { get; init; }

    /// <summary>Cumulative gas used.</summary>
    [JsonPropertyName("cumulativeGasUsed")]
    public long CumulativeGasUsed { get; init; }

    /// <summary>Gas used.</summary>
    [JsonPropertyName("gasUsed")]
    public long GasUsed { get; init; }

    /// <summary>Whether the transaction failed.</summary>
    [JsonPropertyName("isError")]
    public bool IsError { get; init; }

    /// <summary>Failure reason.</summary>
    [JsonPropertyName("errDescription")]
    public string? ErrorDescription { get; init; }

    /// <summary>Receipt status.</summary>
    [JsonPropertyName("txreceipt_status")]
    public bool? ReceiptStatus { get; init; }

    /// <summary>Matching L1 transaction hash.</summary>
    [JsonPropertyName("L1transactionhash")]
    public string? L1TransactionHash { get; init; }

    /// <summary>Bridged token contract.</summary>
    [JsonPropertyName("tokenAddress")]
    public string? TokenAddress { get; init; }

    /// <summary>Bridged token amount.</summary>
    [JsonPropertyName("tokenValue")]
    public BigInteger? TokenValue { get; init; }
}

/// <summary>An L2 deposit transaction (<c>getdeposittxs</c>).</summary>
public sealed record L2DepositTransaction : L2BridgeTransactionBase
{
    /// <summary>Queue index.</summary>
    [JsonPropertyName("queueIndex")]
    public string? QueueIndex { get; init; }

    /// <summary>L1 transaction origin.</summary>
    [JsonPropertyName("L1TxOrigin")]
    public string? L1TxOrigin { get; init; }

    /// <summary>Token sender on L1.</summary>
    [JsonPropertyName("tokenSentFrom")]
    public string? TokenSentFrom { get; init; }

    /// <summary>Token recipient on L2.</summary>
    [JsonPropertyName("tokenSentTo")]
    public string? TokenSentTo { get; init; }
}

/// <summary>An L2 withdrawal transaction (<c>getwithdrawaltxs</c>).</summary>
public sealed record L2WithdrawalTransaction : L2BridgeTransactionBase
{
    /// <summary>Withdrawal status, such as <c>Relayed</c> or <c>Ready to Prove</c>.</summary>
    [JsonPropertyName("status")]
    public string? Status { get; init; }
}

/// <summary>Name tags, labels and reputation for an address (<c>getaddresstag</c>).</summary>
public sealed record AddressMetadata
{
    /// <summary>Address.</summary>
    [JsonPropertyName("address")]
    public string Address { get; init; } = "";

    /// <summary>Public name tag.</summary>
    [JsonPropertyName("nametag")]
    public string? Nametag { get; init; }

    /// <summary>Internal name tag.</summary>
    [JsonPropertyName("internal_nametag")]
    public string? InternalNametag { get; init; }

    /// <summary>Website.</summary>
    [JsonPropertyName("url")]
    public string? Url { get; init; }

    /// <summary>Short description.</summary>
    [JsonPropertyName("shortdescription")]
    public string? ShortDescription { get; init; }

    /// <summary>Notes.</summary>
    [JsonPropertyName("notes_1")]
    public string? Notes1 { get; init; }

    /// <summary>More notes.</summary>
    [JsonPropertyName("notes_2")]
    public string? Notes2 { get; init; }

    /// <summary>Label names.</summary>
    [JsonPropertyName("labels")]
    public IReadOnlyList<string> Labels { get; init; } = [];

    /// <summary>Label slugs.</summary>
    [JsonPropertyName("labels_slug")]
    public IReadOnlyList<string> LabelSlugs { get; init; } = [];

    /// <summary>Reputation score (see the Etherscan reputation reference).</summary>
    [JsonPropertyName("reputation")]
    public int Reputation { get; init; }

    /// <summary>Other attribute codes (see the Etherscan other-attributes reference), as raw JSON.</summary>
    [JsonPropertyName("other_attributes")]
    public IReadOnlyList<JsonElement> OtherAttributes { get; init; } = [];

    /// <summary>Last update time.</summary>
    [JsonPropertyName("lastupdatedtimestamp")]
    public DateTimeOffset? LastUpdated { get; init; }
}

/// <summary>A label in the legacy label master list (<c>getlabelmasterlist</c>, v1).</summary>
public sealed record LabelInfo
{
    /// <summary>Label name.</summary>
    [JsonPropertyName("labelname")]
    public string LabelName { get; init; } = "";

    /// <summary>Label slug, used by the address tag export.</summary>
    [JsonPropertyName("labelslug")]
    public string LabelSlug { get; init; } = "";

    /// <summary>Short description.</summary>
    [JsonPropertyName("shortdescription")]
    public string? ShortDescription { get; init; }

    /// <summary>Notes.</summary>
    [JsonPropertyName("notes")]
    public string? Notes { get; init; }

    /// <summary>Last update time.</summary>
    [JsonPropertyName("lastupdatedtimestamp")]
    public DateTimeOffset? LastUpdated { get; init; }
}
