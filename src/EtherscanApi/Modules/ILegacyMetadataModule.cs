using System.Numerics;
using EtherscanApi.Models;

namespace EtherscanApi;

/// <summary>
/// Legacy (V1) metadata exports served from <c>api-metadata.etherscan.io</c>. Prefer <see cref="NametagsModule"/>.
/// </summary>
public interface ILegacyMetadataModule
{
    /// <summary>Gets the master list of address labels and categories (legacy <c>getlabelmasterlist</c>).</summary>
    /// <remarks>Enterprise tier.</remarks>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/getlabelmasterlist"/>
    Task<IReadOnlyList<LabelInfo>> GetLabelMasterListAsync(CancellationToken cancellationToken = default);

    /// <summary>Exports the addresses tagged with a label as CSV (legacy <c>exportaddresstags</c>). Dispose the stream when done.</summary>
    /// <remarks>Enterprise tier. Throttled to 2 calls/second and 100 calls/day.</remarks>
    /// <param name="label">A label slug, such as <c>phish-hack</c>, or <c>all</c>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/exportaddresstags"/>
    Task<Stream> ExportAddressTagsCsvAsync(string label, CancellationToken cancellationToken = default);
}
