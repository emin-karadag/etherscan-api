using System.Numerics;
using EtherscanApi.Models;

namespace EtherscanApi;

/// <summary>Chain list endpoint (<c>/v2/chainlist</c>).</summary>
public interface IChainsModule
{
    /// <summary>Gets every mainnet and testnet chain supported by the Etherscan API V2, with plan availability.</summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/chainlist"/>
    Task<IReadOnlyList<ChainInfo>> GetChainListAsync(CancellationToken cancellationToken = default);
}
