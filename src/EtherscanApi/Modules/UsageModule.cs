using EtherscanApi.Internal;
using EtherscanApi.Models;

namespace EtherscanApi;

/// <summary>API usage endpoint (<c>getapilimit</c>).</summary>
internal sealed class UsageModule : EtherscanModule, IUsageModule
{
    internal UsageModule(EtherscanTransport transport)
        : base(transport)
    {
    }

    /// <inheritdoc />
    public Task<ApiUsage> GetApiUsageAsync(CancellationToken cancellationToken = default)
        => Transport.SendAsync(new EtherscanRequest("getapilimit", "getapilimit", chain: null), Json.ApiUsage, cancellationToken);
}
