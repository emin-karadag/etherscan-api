using EtherscanApi.Internal;
using EtherscanApi.Models;

namespace EtherscanApi;

/// <summary>ENS endpoints (<c>module=ens</c>).</summary>
internal sealed class EnsModule : EtherscanModule, IEnsModule
{
    internal EnsModule(EtherscanTransport transport)
        : base(transport)
    {
    }

    /// <inheritdoc />
    public Task<string> ResolveAsync(string name, ChainId? chain = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var limit = Transport.Options.Plan == EtherscanPlan.Free ? 1 : (int?)null;
        var request = Request("ens", "forwardresolve", chain ?? ChainId.EthereumMainnet, limit).Add("name", name.Trim());
        return Transport.SendAsync(request, Json.String, cancellationToken);
    }
}
