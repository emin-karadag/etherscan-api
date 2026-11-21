using System.Numerics;
using EtherscanApi.Models;

namespace EtherscanApi;

/// <summary>Name tag and metadata export endpoints (<c>module=nametag</c>, multichain V2).</summary>
public interface INametagsModule
{
    /// <summary>Gets name tags, labels and reputation for an address (<c>getaddresstag</c>).</summary>
    /// <remarks><b>PRO+</b> endpoint (Pro Plus plan only). Throttled to 2 calls/second.</remarks>
    /// <param name="address">The address.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The metadata, or <see langword="null"/> when the address has none.</returns>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/getaddresstag"/>
    Task<AddressMetadata?> GetAddressMetadataAsync(string address, ChainId? chain = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a temporary (5-minute) download URL for the latest label master list JSON (<c>getlabelmasterlist</c>).
    /// </summary>
    /// <remarks>Enterprise plan only. Download it with <see cref="OpenExportAsync"/>.</remarks>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/getlabelmasterlist-v2"/>
    Task<Uri> GetLabelMasterListUrlAsync(ChainId? chain = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a temporary (5-minute) download URL for the addresses tagged with a label (<c>exportaddresstags</c>).
    /// </summary>
    /// <remarks>Enterprise plan only. Throttled to 2 calls/second and 100 calls/day. Download it with <see cref="OpenExportAsync"/>.</remarks>
    /// <param name="label">A label slug from the label master list, such as <c>phish-hack</c>.</param>
    /// <param name="format">CSV (default) or ZIP.</param>
    /// <param name="chain">The chain; defaults to <see cref="EtherscanClientOptions.DefaultChain"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <seealso href="https://docs.etherscan.io/api-reference/endpoint/exportaddresstags-v2"/>
    Task<Uri> GetAddressTagsExportUrlAsync(
        string label,
        ExportFormat format = ExportFormat.Csv,
        ChainId? chain = null,
        CancellationToken cancellationToken = default);

    /// <summary>Opens a download stream for a temporary export URL. Dispose the stream when done.</summary>
    /// <param name="exportUrl">A URL returned by <see cref="GetLabelMasterListUrlAsync"/> or <see cref="GetAddressTagsExportUrlAsync"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    Task<Stream> OpenExportAsync(Uri exportUrl, CancellationToken cancellationToken = default);
}
