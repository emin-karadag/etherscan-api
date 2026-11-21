using EtherscanApi.Internal;
using EtherscanApi.Models;

namespace EtherscanApi;

/// <summary>
/// Contract endpoints (<c>module=contract</c>): ABI, verified source, creation info, and source verification.
/// </summary>
internal sealed class ContractsModule : EtherscanModule, IContractsModule
{
    private const string Module = "contract";

    internal ContractsModule(EtherscanTransport transport)
        : base(transport)
    {
    }

    /// <inheritdoc />
    public Task<string> GetAbiAsync(string address, ChainId? chain = null, CancellationToken cancellationToken = default)
    {
        var request = Request(Module, "getabi", chain).Add("address", Guard.Address(address, nameof(address)));
        return Transport.SendAsync(request, Json.String, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ContractSourceCode> GetSourceCodeAsync(string address, ChainId? chain = null, CancellationToken cancellationToken = default)
    {
        var request = Request(Module, "getsourcecode", chain).Add("address", Guard.Address(address, nameof(address)));
        var list = await Transport.SendListAsync(request, Json.ListContractSourceCode, cancellationToken).ConfigureAwait(false);
        return list.Count > 0 ? list[0] : new ContractSourceCode();
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<ContractCreation>> GetContractCreationAsync(
        IEnumerable<string> contractAddresses,
        ChainId? chain = null,
        CancellationToken cancellationToken = default)
    {
        var request = Request(Module, "getcontractcreation", chain)
            .Add("contractaddresses", Guard.Addresses(contractAddresses, 5, nameof(contractAddresses)));
        return Transport.SendListAsync(request, Json.ListContractCreation, cancellationToken);
    }

    /// <inheritdoc />
    public Task<string> VerifySolidityAsync(SolidityVerificationRequest request, ChainId? chain = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.CodeFormat == SolidityCodeFormat.SingleFile && request.OptimizationUsed is null)
        {
            throw new ArgumentException("OptimizationUsed is required for single-file verification.", nameof(request));
        }

        var post = SourceVerificationPost(request.ContractAddress, request.SourceCode, request.ContractName, request.CompilerVersion, chain)
            .AddForm("codeformat", CodeFormat(request.CodeFormat))
            .AddForm("optimizationUsed", Flag(request.OptimizationUsed))
            .AddForm("runs", request.Runs)
            .AddForm("constructorArguments", StripHexPrefix(request.ConstructorArguments))
            .AddForm("evmVersion", request.EvmVersion)
            .AddForm("licenseType", request.LicenseType);
        return Transport.SendAsync(post, Json.String, cancellationToken);
    }

    /// <inheritdoc />
    public Task<string> VerifyVyperAsync(VyperVerificationRequest request, ChainId? chain = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var post = SourceVerificationPost(request.ContractAddress, request.SourceCode, request.ContractName, request.CompilerVersion, chain)
            .AddForm("codeformat", "vyper-json")
            .AddForm("optimizationUsed", Flag(request.OptimizationUsed))
            .AddForm("constructorArguments", StripHexPrefix(request.ConstructorArguments));
        return Transport.SendAsync(post, Json.String, cancellationToken);
    }

    /// <inheritdoc />
    public Task<string> VerifyStylusAsync(StylusVerificationRequest request, ChainId? chain = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var post = SourceVerificationPost(request.ContractAddress, request.SourceCode, request.ContractName, request.CompilerVersion, chain)
            .AddForm("codeformat", "stylus")
            .AddForm("licenseType", request.LicenseType);
        return Transport.SendAsync(post, Json.String, cancellationToken);
    }

    /// <inheritdoc />
    public Task<string> VerifyZkSyncAsync(ZkSyncVerificationRequest request, ChainId? chain = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.CodeFormat == SolidityCodeFormat.SingleFile && request.OptimizationUsed is null)
        {
            throw new ArgumentException("OptimizationUsed is required for single-file verification.", nameof(request));
        }

        var post = SourceVerificationPost(request.ContractAddress, request.SourceCode, request.ContractName, request.CompilerVersion, chain)
            .AddForm("zksolcVersion", Required(request.ZksolcVersion, nameof(request.ZksolcVersion)))
            .AddForm("codeformat", CodeFormat(request.CodeFormat))
            .AddForm("optimizationUsed", Flag(request.OptimizationUsed))
            .AddForm("constructorArguments", StripHexPrefix(request.ConstructorArguments));
        return Transport.SendAsync(post, Json.String, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<VerificationStatus> CheckVerificationStatusAsync(string guid, ChainId? chain = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(guid);
        var request = Request(Module, "checkverifystatus", chain).Add("guid", guid);
        var envelope = await Transport.SendEnvelopeAsync(request, cancellationToken).ConfigureAwait(false);
        return ToStatus(envelope);
    }

    /// <inheritdoc />
    public Task<string> VerifyProxyAsync(
        string proxyAddress,
        string? expectedImplementation = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default)
    {
        var post = VerificationPost("verifyproxycontract", chain)
            .AddForm("address", Guard.Address(proxyAddress, nameof(proxyAddress)))
            .AddForm("expectedimplementation", Guard.OptionalAddress(expectedImplementation, nameof(expectedImplementation)));
        return Transport.SendAsync(post, Json.String, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<VerificationStatus> CheckProxyVerificationAsync(string guid, ChainId? chain = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(guid);
        var request = Request(Module, "checkproxyverification", chain).Add("guid", guid);
        var envelope = await Transport.SendEnvelopeAsync(request, cancellationToken).ConfigureAwait(false);
        return ToStatus(envelope);
    }

    /// <inheritdoc />
    public async Task<VerificationStatus> WaitForVerificationAsync(
        string guid,
        TimeSpan? pollInterval = null,
        TimeSpan? timeout = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(guid);
        var interval = pollInterval ?? TimeSpan.FromSeconds(5);
        var limit = timeout ?? TimeSpan.FromMinutes(5);
        ArgumentOutOfRangeException.ThrowIfLessThan(interval, TimeSpan.FromSeconds(1), nameof(pollInterval));
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(limit, TimeSpan.Zero, nameof(timeout));

        var time = Transport.TimeProvider;
        var started = time.GetTimestamp();
        while (true)
        {
            var status = await CheckVerificationStatusAsync(guid, chain, cancellationToken).ConfigureAwait(false);
            if (!status.IsPending || time.GetElapsedTime(started) + interval > limit)
            {
                return status;
            }

            await Task.Delay(interval, time, cancellationToken).ConfigureAwait(false);
        }
    }

    private EtherscanRequest VerificationPost(string action, ChainId? chain)
    {
        // Sent as a form POST with the API key in the body, as documented. Never retried.
        return new EtherscanRequest(Module, action, Transport.Resolve(chain)) { IsIdempotent = false }.AsFormPost();
    }

    /// <summary>Starts a <c>verifysourcecode</c> submission with the fields every source format requires.</summary>
    private EtherscanRequest SourceVerificationPost(string contractAddress, string sourceCode, string contractName, string compilerVersion, ChainId? chain)
        => VerificationPost("verifysourcecode", chain)
            .AddForm("contractaddress", Guard.Address(contractAddress, nameof(contractAddress)))
            .AddForm("sourceCode", Required(sourceCode, nameof(sourceCode)))
            .AddForm("contractname", Required(contractName, nameof(contractName)))
            .AddForm("compilerversion", Required(compilerVersion, nameof(compilerVersion)));

    private static VerificationStatus ToStatus(EnvelopeText envelope)
    {
        var result = envelope.Result ?? envelope.Message ?? "";
        if (envelope.IsOk)
        {
            return new VerificationStatus(VerificationState.Success, result);
        }

        return result.Contains("pending", StringComparison.OrdinalIgnoreCase)
               || result.Contains("in queue", StringComparison.OrdinalIgnoreCase)
            ? new VerificationStatus(VerificationState.Pending, result)
            : new VerificationStatus(VerificationState.Failure, result);
    }

    private static string CodeFormat(SolidityCodeFormat format) => format switch
    {
        SolidityCodeFormat.SingleFile => "solidity-single-file",
        SolidityCodeFormat.StandardJsonInput => "solidity-standard-json-input",
        _ => throw new ArgumentOutOfRangeException(nameof(format)),
    };

    private static string? Flag(bool? value) => value is null ? null : value.Value ? "1" : "0";

    private static string? StripHexPrefix(string? value)
        => value is not null && value.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? value[2..] : value;

    private static string Required(string? value, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, name);
        return value;
    }
}
