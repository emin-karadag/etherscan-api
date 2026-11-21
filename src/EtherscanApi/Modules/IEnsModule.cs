using System.Numerics;
using EtherscanApi.Models;

namespace EtherscanApi;

/// <summary>ENS endpoints (<c>module=ens</c>).</summary>
public interface IEnsModule
{
    /// <summary>Resolves an ENS name or onchain subdomain (for example <c>vitalik.eth</c>) to an address (<c>forwardresolve</c>).</summary>
    /// <remarks>Ethereum Mainnet. Throttled to 1 call/second on the Free tier. Results are cached for up to 5 minutes.</remarks>
    /// <param name="name">The ENS name.</param>
    /// <param name="chain">The chain; defaults to <see cref="ChainId.EthereumMainnet"/> (not the client default).</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The resolved address.</returns>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/forwardresolve"/>
    Task<string> ResolveAsync(string name, ChainId? chain = null, CancellationToken cancellationToken = default);
}
