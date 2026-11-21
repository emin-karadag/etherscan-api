using System.Numerics;
using System.Text.Json.Serialization;

namespace EtherscanApi.Models;

/// <summary>The native balance of one address (<c>balancemulti</c>).</summary>
public sealed record AddressBalance
{
    /// <summary>The address.</summary>
    [JsonPropertyName("account")]
    public string Account { get; init; } = "";

    /// <summary>The balance in wei.</summary>
    [JsonPropertyName("balance")]
    public BigInteger Balance { get; init; }
}

/// <summary>A normal (external) transaction (<c>txlist</c>).</summary>
public sealed record NormalTransaction
{
    /// <summary>Block number.</summary>
    [JsonPropertyName("blockNumber")]
    public long BlockNumber { get; init; }

    /// <summary>Block hash.</summary>
    [JsonPropertyName("blockHash")]
    public string? BlockHash { get; init; }

    /// <summary>Block timestamp.</summary>
    [JsonPropertyName("timeStamp")]
    public DateTimeOffset TimeStamp { get; init; }

    /// <summary>Transaction hash.</summary>
    [JsonPropertyName("hash")]
    public string Hash { get; init; } = "";

    /// <summary>Sender nonce.</summary>
    [JsonPropertyName("nonce")]
    public long Nonce { get; init; }

    /// <summary>Position of the transaction in the block.</summary>
    [JsonPropertyName("transactionIndex")]
    public long TransactionIndex { get; init; }

    /// <summary>Sender address.</summary>
    [JsonPropertyName("from")]
    public string From { get; init; } = "";

    /// <summary>Recipient address; empty for contract creations.</summary>
    [JsonPropertyName("to")]
    public string? To { get; init; }

    /// <summary>Value transferred, in wei.</summary>
    [JsonPropertyName("value")]
    public BigInteger Value { get; init; }

    /// <summary>Gas limit provided by the sender.</summary>
    [JsonPropertyName("gas")]
    public long Gas { get; init; }

    /// <summary>Gas price, in wei.</summary>
    [JsonPropertyName("gasPrice")]
    public BigInteger GasPrice { get; init; }

    /// <summary>Call data.</summary>
    [JsonPropertyName("input")]
    public string? Input { get; init; }

    /// <summary>Function selector of the call.</summary>
    [JsonPropertyName("methodId")]
    public string? MethodId { get; init; }

    /// <summary>Decoded function signature, when known.</summary>
    [JsonPropertyName("functionName")]
    public string? FunctionName { get; init; }

    /// <summary>Created contract address, for contract creations.</summary>
    [JsonPropertyName("contractAddress")]
    public string? ContractAddress { get; init; }

    /// <summary>Cumulative gas used in the block up to this transaction.</summary>
    [JsonPropertyName("cumulativeGasUsed")]
    public long CumulativeGasUsed { get; init; }

    /// <summary>Receipt status: <see langword="true"/> for success, <see langword="null"/> before Byzantium.</summary>
    [JsonPropertyName("txreceipt_status")]
    public bool? ReceiptStatus { get; init; }

    /// <summary>Gas used by this transaction.</summary>
    [JsonPropertyName("gasUsed")]
    public long GasUsed { get; init; }

    /// <summary>Number of confirmations.</summary>
    [JsonPropertyName("confirmations")]
    public long Confirmations { get; init; }

    /// <summary>Whether the transaction failed.</summary>
    [JsonPropertyName("isError")]
    public bool IsError { get; init; }
}

/// <summary>An internal transaction (<c>txlistinternal</c>).</summary>
public sealed record InternalTransaction
{
    /// <summary>Block number.</summary>
    [JsonPropertyName("blockNumber")]
    public long BlockNumber { get; init; }

    /// <summary>Block timestamp.</summary>
    [JsonPropertyName("timeStamp")]
    public DateTimeOffset TimeStamp { get; init; }

    /// <summary>Parent transaction hash. Not returned by the by-hash variant.</summary>
    [JsonPropertyName("hash")]
    public string? Hash { get; init; }

    /// <summary>Sender address.</summary>
    [JsonPropertyName("from")]
    public string From { get; init; } = "";

    /// <summary>Recipient address.</summary>
    [JsonPropertyName("to")]
    public string? To { get; init; }

    /// <summary>Value transferred, in wei.</summary>
    [JsonPropertyName("value")]
    public BigInteger Value { get; init; }

    /// <summary>Created contract address, for create calls.</summary>
    [JsonPropertyName("contractAddress")]
    public string? ContractAddress { get; init; }

    /// <summary>Call data.</summary>
    [JsonPropertyName("input")]
    public string? Input { get; init; }

    /// <summary>Call type, such as <c>call</c>, <c>create</c> or <c>delegatecall</c>.</summary>
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    /// <summary>Gas provided.</summary>
    [JsonPropertyName("gas")]
    public long Gas { get; init; }

    /// <summary>Gas used.</summary>
    [JsonPropertyName("gasUsed")]
    public long GasUsed { get; init; }

    /// <summary>Trace position within the transaction.</summary>
    [JsonPropertyName("traceId")]
    public string? TraceId { get; init; }

    /// <summary>Whether the call failed.</summary>
    [JsonPropertyName("isError")]
    public bool IsError { get; init; }

    /// <summary>Error code, when the call failed.</summary>
    [JsonPropertyName("errCode")]
    public string? ErrorCode { get; init; }
}

/// <summary>Fields shared by ERC-20, ERC-721 and ERC-1155 transfer events.</summary>
public abstract record TokenTransferBase
{
    /// <summary>Block number.</summary>
    [JsonPropertyName("blockNumber")]
    public long BlockNumber { get; init; }

    /// <summary>Block timestamp.</summary>
    [JsonPropertyName("timeStamp")]
    public DateTimeOffset TimeStamp { get; init; }

    /// <summary>Transaction hash.</summary>
    [JsonPropertyName("hash")]
    public string Hash { get; init; } = "";

    /// <summary>Sender nonce.</summary>
    [JsonPropertyName("nonce")]
    public long Nonce { get; init; }

    /// <summary>Block hash.</summary>
    [JsonPropertyName("blockHash")]
    public string? BlockHash { get; init; }

    /// <summary>Token sender.</summary>
    [JsonPropertyName("from")]
    public string From { get; init; } = "";

    /// <summary>Token contract address.</summary>
    [JsonPropertyName("contractAddress")]
    public string ContractAddress { get; init; } = "";

    /// <summary>Token recipient.</summary>
    [JsonPropertyName("to")]
    public string? To { get; init; }

    /// <summary>Token name.</summary>
    [JsonPropertyName("tokenName")]
    public string? TokenName { get; init; }

    /// <summary>Token symbol.</summary>
    [JsonPropertyName("tokenSymbol")]
    public string? TokenSymbol { get; init; }

    /// <summary>Position of the transaction in the block.</summary>
    [JsonPropertyName("transactionIndex")]
    public long TransactionIndex { get; init; }

    /// <summary>Gas limit.</summary>
    [JsonPropertyName("gas")]
    public long Gas { get; init; }

    /// <summary>Gas price, in wei.</summary>
    [JsonPropertyName("gasPrice")]
    public BigInteger GasPrice { get; init; }

    /// <summary>Gas used.</summary>
    [JsonPropertyName("gasUsed")]
    public long GasUsed { get; init; }

    /// <summary>Cumulative gas used in the block.</summary>
    [JsonPropertyName("cumulativeGasUsed")]
    public long CumulativeGasUsed { get; init; }

    /// <summary>Call data (deprecated by Etherscan for transfer events; may be empty).</summary>
    [JsonPropertyName("input")]
    public string? Input { get; init; }

    /// <summary>Function selector.</summary>
    [JsonPropertyName("methodId")]
    public string? MethodId { get; init; }

    /// <summary>Decoded function signature.</summary>
    [JsonPropertyName("functionName")]
    public string? FunctionName { get; init; }

    /// <summary>Number of confirmations.</summary>
    [JsonPropertyName("confirmations")]
    public long Confirmations { get; init; }
}

/// <summary>An ERC-20 token transfer (<c>tokentx</c>).</summary>
public sealed record Erc20Transfer : TokenTransferBase
{
    /// <summary>Amount in the token's smallest unit. Divide by 10^<see cref="TokenDecimal"/> for display.</summary>
    [JsonPropertyName("value")]
    public BigInteger Value { get; init; }

    /// <summary>Token decimals.</summary>
    [JsonPropertyName("tokenDecimal")]
    public int? TokenDecimal { get; init; }

    /// <summary>Etherscan token reputation flag, when present.</summary>
    [JsonPropertyName("statusRep")]
    public string? StatusRep { get; init; }
}

/// <summary>An ERC-721 token transfer (<c>tokennfttx</c>).</summary>
public sealed record Erc721Transfer : TokenTransferBase
{
    /// <summary>Token id.</summary>
    [JsonPropertyName("tokenID")]
    public BigInteger TokenId { get; init; }

    /// <summary>Token decimals (normally 0).</summary>
    [JsonPropertyName("tokenDecimal")]
    public int? TokenDecimal { get; init; }
}

/// <summary>An ERC-1155 token transfer (<c>token1155tx</c>).</summary>
public sealed record Erc1155Transfer : TokenTransferBase
{
    /// <summary>Token id.</summary>
    [JsonPropertyName("tokenID")]
    public BigInteger TokenId { get; init; }

    /// <summary>Number of tokens transferred.</summary>
    [JsonPropertyName("tokenValue")]
    public BigInteger TokenValue { get; init; }
}

/// <summary>A block validated by an address (<c>getminedblocks</c>).</summary>
public sealed record MinedBlock
{
    /// <summary>Block number.</summary>
    [JsonPropertyName("blockNumber")]
    public long BlockNumber { get; init; }

    /// <summary>Block timestamp.</summary>
    [JsonPropertyName("timeStamp")]
    public DateTimeOffset TimeStamp { get; init; }

    /// <summary>Block reward, in wei.</summary>
    [JsonPropertyName("blockReward")]
    public BigInteger BlockReward { get; init; }
}

/// <summary>A beacon chain withdrawal (<c>txsBeaconWithdrawal</c>).</summary>
public sealed record BeaconWithdrawal
{
    /// <summary>Withdrawal index.</summary>
    [JsonPropertyName("withdrawalIndex")]
    public long WithdrawalIndex { get; init; }

    /// <summary>Validator index.</summary>
    [JsonPropertyName("validatorIndex")]
    public long ValidatorIndex { get; init; }

    /// <summary>Withdrawal recipient.</summary>
    [JsonPropertyName("address")]
    public string Address { get; init; } = "";

    /// <summary>Amount withdrawn, in gwei.</summary>
    [JsonPropertyName("amount")]
    public BigInteger Amount { get; init; }

    /// <summary>Block number.</summary>
    [JsonPropertyName("blockNumber")]
    public long BlockNumber { get; init; }

    /// <summary>Block timestamp.</summary>
    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; init; }
}

/// <summary>The address and transaction that first funded an EOA (<c>fundedby</c>).</summary>
public sealed record FundingSource
{
    /// <summary>Block number of the funding transaction.</summary>
    [JsonPropertyName("block")]
    public long Block { get; init; }

    /// <summary>Timestamp of the funding transaction.</summary>
    [JsonPropertyName("timeStamp")]
    public DateTimeOffset TimeStamp { get; init; }

    /// <summary>The funding address.</summary>
    [JsonPropertyName("fundingAddress")]
    public string FundingAddress { get; init; } = "";

    /// <summary>The funding transaction hash.</summary>
    [JsonPropertyName("fundingTxn")]
    public string FundingTransaction { get; init; } = "";

    /// <summary>Value received, in wei.</summary>
    [JsonPropertyName("value")]
    public BigInteger Value { get; init; }
}
