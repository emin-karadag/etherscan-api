using System.Numerics;
using EtherscanApi.Models;

namespace EtherscanApi;

/// <summary>API usage endpoint (<c>getapilimit</c>).</summary>
public interface IUsageModule
{
    /// <summary>Gets your API credit usage and limit for the current interval (<c>getapilimit</c>).</summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/getapilimit"/>
    Task<ApiUsage> GetApiUsageAsync(CancellationToken cancellationToken = default);
}
