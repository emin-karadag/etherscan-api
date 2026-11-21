using EtherscanApi.Internal;
using EtherscanApi.Models;

namespace EtherscanApi;

/// <summary>Chain list endpoint (<c>/v2/chainlist</c>).</summary>
internal sealed class ChainsModule : EtherscanModule, IChainsModule
{
    internal ChainsModule(EtherscanTransport transport)
        : base(transport)
    {
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<ChainInfo>> GetChainListAsync(CancellationToken cancellationToken = default)
    {
        var request = new EtherscanRequest("chainlist", "chainlist", chain: null)
        {
            Path = "v2/chainlist",
            IncludeModuleAndAction = false,
        };
        return Transport.SendListAsync(request, Json.ListChainInfo, cancellationToken);
    }
}
