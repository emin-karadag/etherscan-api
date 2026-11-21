using System.Text.Json.Serialization;

namespace EtherscanApi.Models;

/// <summary>Verified source code and compiler settings of a contract (<c>getsourcecode</c>).</summary>
public sealed record ContractSourceCode
{
    /// <summary>
    /// Source code. A plain Solidity file, or standard-JSON input (sometimes wrapped in an extra pair of braces).
    /// Empty when the contract is not verified.
    /// </summary>
    [JsonPropertyName("SourceCode")]
    public string SourceCode { get; init; } = "";

    /// <summary>ABI as a JSON string, or <c>Contract source code not verified</c>.</summary>
    [JsonPropertyName("ABI")]
    public string Abi { get; init; } = "";

    /// <summary>Contract name.</summary>
    [JsonPropertyName("ContractName")]
    public string? ContractName { get; init; }

    /// <summary>Compiler version, such as <c>v0.8.24+commit.e11b9ed9</c>.</summary>
    [JsonPropertyName("CompilerVersion")]
    public string? CompilerVersion { get; init; }

    /// <summary>Compiler type, such as <c>solc</c> or <c>vyper</c>.</summary>
    [JsonPropertyName("CompilerType")]
    public string? CompilerType { get; init; }

    /// <summary>Whether optimization was enabled.</summary>
    [JsonPropertyName("OptimizationUsed")]
    public bool OptimizationUsed { get; init; }

    /// <summary>Optimizer runs.</summary>
    [JsonPropertyName("Runs")]
    public int? Runs { get; init; }

    /// <summary>ABI-encoded constructor arguments (hex, without 0x).</summary>
    [JsonPropertyName("ConstructorArguments")]
    public string? ConstructorArguments { get; init; }

    /// <summary>EVM version.</summary>
    [JsonPropertyName("EVMVersion")]
    public string? EvmVersion { get; init; }

    /// <summary>Linked libraries.</summary>
    [JsonPropertyName("Library")]
    public string? Library { get; init; }

    /// <summary>Main contract file name.</summary>
    [JsonPropertyName("ContractFileName")]
    public string? ContractFileName { get; init; }

    /// <summary>License type.</summary>
    [JsonPropertyName("LicenseType")]
    public string? LicenseType { get; init; }

    /// <summary>Whether the contract is a proxy.</summary>
    [JsonPropertyName("Proxy")]
    public bool IsProxy { get; init; }

    /// <summary>Implementation address, for proxies.</summary>
    [JsonPropertyName("Implementation")]
    public string? Implementation { get; init; }

    /// <summary>Swarm source.</summary>
    [JsonPropertyName("SwarmSource")]
    public string? SwarmSource { get; init; }

    /// <summary>Address of a similar verified contract the source was matched from, if any.</summary>
    [JsonPropertyName("SimilarMatch")]
    public string? SimilarMatch { get; init; }

    /// <summary>Whether the contract has verified source code.</summary>
    [JsonIgnore]
    public bool IsVerified => !string.IsNullOrEmpty(SourceCode);
}

/// <summary>Deployer and creation transaction of a contract (<c>getcontractcreation</c>).</summary>
public sealed record ContractCreation
{
    /// <summary>Contract address.</summary>
    [JsonPropertyName("contractAddress")]
    public string ContractAddress { get; init; } = "";

    /// <summary>Deployer address.</summary>
    [JsonPropertyName("contractCreator")]
    public string ContractCreator { get; init; } = "";

    /// <summary>Creation transaction hash.</summary>
    [JsonPropertyName("txHash")]
    public string TransactionHash { get; init; } = "";

    /// <summary>Creation block number.</summary>
    [JsonPropertyName("blockNumber")]
    public long? BlockNumber { get; init; }

    /// <summary>Creation timestamp.</summary>
    [JsonPropertyName("timestamp")]
    public DateTimeOffset? Timestamp { get; init; }

    /// <summary>Factory contract that deployed it, if any.</summary>
    [JsonPropertyName("contractFactory")]
    public string? ContractFactory { get; init; }

    /// <summary>Creation bytecode.</summary>
    [JsonPropertyName("creationBytecode")]
    public string? CreationBytecode { get; init; }
}

/// <summary>Source format for Solidity and zkSync verification (<c>codeformat</c>).</summary>
public enum SolidityCodeFormat
{
    /// <summary>A single flattened file (<c>solidity-single-file</c>). Requires optimization settings.</summary>
    SingleFile,

    /// <summary>Standard JSON input (<c>solidity-standard-json-input</c>).</summary>
    StandardJsonInput,
}

/// <summary>Submission for Solidity source code verification (<c>verifysourcecode</c>).</summary>
public sealed record SolidityVerificationRequest
{
    /// <summary>Address where the contract is deployed.</summary>
    public required string ContractAddress { get; init; }

    /// <summary>Flattened source, or standard-JSON input.</summary>
    public required string SourceCode { get; init; }

    /// <summary>Source format. Defaults to <see cref="SolidityCodeFormat.StandardJsonInput"/>.</summary>
    public SolidityCodeFormat CodeFormat { get; init; } = SolidityCodeFormat.StandardJsonInput;

    /// <summary>Contract name, with path for standard-JSON input (<c>contracts/Token.sol:Token</c>).</summary>
    public required string ContractName { get; init; }

    /// <summary>Compiler version, such as <c>v0.8.24+commit.e11b9ed9</c>.</summary>
    public required string CompilerVersion { get; init; }

    /// <summary>Whether optimization was enabled. Required for single-file verification.</summary>
    public bool? OptimizationUsed { get; init; }

    /// <summary>Optimizer runs.</summary>
    public int? Runs { get; init; }

    /// <summary>ABI-encoded constructor arguments (hex).</summary>
    public string? ConstructorArguments { get; init; }

    /// <summary>EVM version, or <c>default</c>.</summary>
    public string? EvmVersion { get; init; }

    /// <summary>License type id (see https://etherscan.io/contract-license-types), e.g. 3 for MIT.</summary>
    public int? LicenseType { get; init; }
}

/// <summary>Submission for Vyper verification (<c>verifysourcecode</c>, <c>codeformat=vyper-json</c>).</summary>
public sealed record VyperVerificationRequest
{
    /// <summary>Address where the contract is deployed.</summary>
    public required string ContractAddress { get; init; }

    /// <summary>The Vyper source in JSON format.</summary>
    public required string SourceCode { get; init; }

    /// <summary>Contract name, with path (<c>contracts/Token.vy:Token</c>).</summary>
    public required string ContractName { get; init; }

    /// <summary>Compiler version, such as <c>vyper:0.4.0</c>.</summary>
    public required string CompilerVersion { get; init; }

    /// <summary>Whether optimization was enabled.</summary>
    public required bool OptimizationUsed { get; init; }

    /// <summary>ABI-encoded constructor arguments (hex).</summary>
    public string? ConstructorArguments { get; init; }
}

/// <summary>Submission for Arbitrum Stylus verification (<c>verifysourcecode</c>, <c>codeformat=stylus</c>).</summary>
public sealed record StylusVerificationRequest
{
    /// <summary>Address where the contract is deployed.</summary>
    public required string ContractAddress { get; init; }

    /// <summary>Public Git repository that hosts the Stylus source.</summary>
    public required string SourceCode { get; init; }

    /// <summary>Contract name matching the Stylus deployment.</summary>
    public required string ContractName { get; init; }

    /// <summary>Compiler version, such as <c>stylus:0.5.3</c>.</summary>
    public required string CompilerVersion { get; init; }

    /// <summary>License type id.</summary>
    public int? LicenseType { get; init; }
}

/// <summary>Submission for zkSync verification (<c>verifysourcecode</c> with <c>zksolcVersion</c>).</summary>
public sealed record ZkSyncVerificationRequest
{
    /// <summary>Address where the contract is deployed.</summary>
    public required string ContractAddress { get; init; }

    /// <summary>Flattened source, or standard-JSON input.</summary>
    public required string SourceCode { get; init; }

    /// <summary>Source format. Defaults to <see cref="SolidityCodeFormat.StandardJsonInput"/>.</summary>
    public SolidityCodeFormat CodeFormat { get; init; } = SolidityCodeFormat.StandardJsonInput;

    /// <summary>Contract name, with path.</summary>
    public required string ContractName { get; init; }

    /// <summary>solc version, such as <c>v0.8.24+commit.e11b9ed9</c>.</summary>
    public required string CompilerVersion { get; init; }

    /// <summary>zksolc version with the <c>v</c> prefix, such as <c>v1.3.13</c>.</summary>
    public required string ZksolcVersion { get; init; }

    /// <summary>Whether optimization was enabled. Required for single-file verification.</summary>
    public bool? OptimizationUsed { get; init; }

    /// <summary>ABI-encoded constructor arguments (hex).</summary>
    public string? ConstructorArguments { get; init; }
}

/// <summary>Outcome of a verification status check.</summary>
/// <param name="State">Whether the verification is pending, succeeded, or failed.</param>
/// <param name="Result">Etherscan's status text, such as <c>Pass - Verified</c> or <c>Pending in queue</c>.</param>
public sealed record VerificationStatus(VerificationState State, string Result)
{
    /// <summary>Whether the verification succeeded.</summary>
    public bool IsSuccess => State == VerificationState.Success;

    /// <summary>Whether the verification is still queued or running.</summary>
    public bool IsPending => State == VerificationState.Pending;

    /// <summary>Whether the verification failed.</summary>
    public bool IsFailure => State == VerificationState.Failure;
}

/// <summary>State of a verification request.</summary>
public enum VerificationState
{
    /// <summary>Queued or in progress.</summary>
    Pending,

    /// <summary>Verified.</summary>
    Success,

    /// <summary>Verification failed; <see cref="VerificationStatus.Result"/> holds the reason.</summary>
    Failure,
}
