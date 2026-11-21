using EtherscanApi.Internal;
using EtherscanApi.Models;

namespace EtherscanApi;

/// <summary>Name tag and metadata export endpoints (<c>module=nametag</c>, multichain V2).</summary>
internal sealed class NametagsModule : EtherscanModule, INametagsModule
{
    internal NametagsModule(EtherscanTransport transport)
        : base(transport)
    {
    }

    /// <inheritdoc />
    public async Task<AddressMetadata?> GetAddressMetadataAsync(string address, ChainId? chain = null, CancellationToken cancellationToken = default)
    {
        var request = Request("nametag", "getaddresstag", chain, endpointRequestsPerSecond: 2)
            .Add("address", Guard.Address(address, nameof(address)));
        var list = await Transport.SendListAsync(request, Json.ListAddressMetadata, cancellationToken).ConfigureAwait(false);
        return list.Count > 0 ? list[0] : null;
    }

    /// <inheritdoc />
    public async Task<Uri> GetLabelMasterListUrlAsync(ChainId? chain = null, CancellationToken cancellationToken = default)
    {
        var request = Request("nametag", "getlabelmasterlist", chain);
        var url = await Transport.SendAsync(request, Json.String, cancellationToken).ConfigureAwait(false);
        return new Uri(url, UriKind.Absolute);
    }

    /// <inheritdoc />
    public async Task<Uri> GetAddressTagsExportUrlAsync(
        string label,
        ExportFormat format = ExportFormat.Csv,
        ChainId? chain = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(label);
        var request = Request("nametag", "exportaddresstags", chain, endpointRequestsPerSecond: 2)
            .Add("label", label.Trim())
            .Add("format", WireFormat.Format(format));
        var url = await Transport.SendAsync(request, Json.String, cancellationToken).ConfigureAwait(false);
        return new Uri(url, UriKind.Absolute);
    }

    /// <inheritdoc />
    public Task<Stream> OpenExportAsync(Uri exportUrl, CancellationToken cancellationToken = default)
        => Transport.OpenDownloadAsync(exportUrl, cancellationToken);
}
