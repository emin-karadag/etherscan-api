using System.Numerics;
using EtherscanApi.Models;

namespace EtherscanApi;

/// <summary>
/// Contract endpoints (<c>module=contract</c>): ABI, verified source, creation info, and source verification.
/// </summary>
public interface IContractsModule
{
    /// <summary>Gets the ABI of a verified contract as a JSON string (<c>getabi</c>).</summary>
    /// <remarks>Available on every plan and chain, including the Free tier.</remarks>
    /// <param name="address">The contract address.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <exception cref="EtherscanApiException">The contract is not verified.</exception>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/getabi"/>
    Task<string> GetAbiAsync(string address, ChainId? chain = null, CancellationToken cancellationToken = default);

    /// <summary>Gets the verified source code, ABI and compiler settings of a contract (<c>getsourcecode</c>).</summary>
    /// <remarks>
    /// Available on every plan and chain. For unverified contracts the call succeeds with an empty
    /// <see cref="ContractSourceCode.SourceCode"/>; check <see cref="ContractSourceCode.IsVerified"/>.
    /// </remarks>
    /// <param name="address">The contract address.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/getsourcecode"/>
    Task<ContractSourceCode> GetSourceCodeAsync(string address, ChainId? chain = null, CancellationToken cancellationToken = default);

    /// <summary>Gets the deployer and creation transaction of up to 5 contracts (<c>getcontractcreation</c>).</summary>
    /// <param name="contractAddresses">1 to 5 contract addresses.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/getcontractcreation"/>
    Task<IReadOnlyList<ContractCreation>> GetContractCreationAsync(
        IEnumerable<string> contractAddresses,
        ChainId? chain = null,
        CancellationToken cancellationToken = default);

    /// <summary>Submits Solidity source code for verification (<c>verifysourcecode</c>).</summary>
    /// <remarks>
    /// This is a state-changing submission: it is sent once and never retried automatically.
    /// The returned GUID is a request id, not a confirmation; poll <see cref="CheckVerificationStatusAsync"/>
    /// or use <see cref="WaitForVerificationAsync"/>. Verify on each chain separately.
    /// </remarks>
    /// <param name="request">The submission.</param>
    /// <param name="chain">The chain the contract is deployed on.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The verification GUID.</returns>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/verifysourcecode"/>
    Task<string> VerifySolidityAsync(SolidityVerificationRequest request, ChainId? chain = null, CancellationToken cancellationToken = default);

    /// <summary>Submits a Vyper contract for verification (<c>verifysourcecode</c>, <c>codeformat=vyper-json</c>).</summary>
    /// <remarks>Never retried automatically. Poll <see cref="CheckVerificationStatusAsync"/> with the returned GUID.</remarks>
    /// <param name="request">The submission.</param>
    /// <param name="chain">The chain the contract is deployed on.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The verification GUID.</returns>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/verifyvyper"/>
    Task<string> VerifyVyperAsync(VyperVerificationRequest request, ChainId? chain = null, CancellationToken cancellationToken = default);

    /// <summary>Submits an Arbitrum Stylus project for verification (<c>verifysourcecode</c>, <c>codeformat=stylus</c>).</summary>
    /// <remarks>Never retried automatically. Poll <see cref="CheckVerificationStatusAsync"/> with the returned GUID.</remarks>
    /// <param name="request">The submission.</param>
    /// <param name="chain">The chain the contract is deployed on.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The verification GUID.</returns>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/verifystylus"/>
    Task<string> VerifyStylusAsync(StylusVerificationRequest request, ChainId? chain = null, CancellationToken cancellationToken = default);

    /// <summary>Submits zkSync source code for verification (<c>verifysourcecode</c> with <c>zksolcVersion</c>).</summary>
    /// <remarks>Never retried automatically. Poll <see cref="CheckVerificationStatusAsync"/> with the returned GUID.</remarks>
    /// <param name="request">The submission.</param>
    /// <param name="chain">The zkSync chain the contract is deployed on.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The verification GUID.</returns>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/verifyzksyncsourcecode"/>
    Task<string> VerifyZkSyncAsync(ZkSyncVerificationRequest request, ChainId? chain = null, CancellationToken cancellationToken = default);

    /// <summary>Checks the status of a source code verification (<c>checkverifystatus</c>).</summary>
    /// <param name="guid">The GUID returned by a verify call.</param>
    /// <param name="chain">The chain of the submission.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/checkverifystatus"/>
    Task<VerificationStatus> CheckVerificationStatusAsync(string guid, ChainId? chain = null, CancellationToken cancellationToken = default);

    /// <summary>Submits a proxy contract for verification (<c>verifyproxycontract</c>).</summary>
    /// <remarks>
    /// Never retried automatically. Poll <see cref="CheckProxyVerificationAsync"/> with the returned GUID.
    /// </remarks>
    /// <param name="proxyAddress">The proxy contract address.</param>
    /// <param name="expectedImplementation">Optional implementation address to enforce.</param>
    /// <param name="chain">The chain the proxy is deployed on.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The verification GUID.</returns>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/verifyproxycontract"/>
    Task<string> VerifyProxyAsync(
        string proxyAddress,
        string? expectedImplementation = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default);

    /// <summary>Checks the status of a proxy verification (<c>checkproxyverification</c>).</summary>
    /// <param name="guid">The GUID returned by <see cref="VerifyProxyAsync"/>.</param>
    /// <param name="chain">The chain of the submission.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/checkproxyverification"/>
    Task<VerificationStatus> CheckProxyVerificationAsync(string guid, ChainId? chain = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Polls <see cref="CheckVerificationStatusAsync"/> until the verification leaves the pending state.
    /// </summary>
    /// <param name="guid">The GUID returned by a verify call.</param>
    /// <param name="pollInterval">Delay between checks. Defaults to 5 seconds.</param>
    /// <param name="timeout">Maximum time to wait. Defaults to 5 minutes.</param>
    /// <param name="chain">The chain of the submission.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The final status; still pending if <paramref name="timeout"/> elapsed.</returns>
    Task<VerificationStatus> WaitForVerificationAsync(
        string guid,
        TimeSpan? pollInterval = null,
        TimeSpan? timeout = null,
        ChainId? chain = null,
        CancellationToken cancellationToken = default);
}
