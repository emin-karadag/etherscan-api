using EtherscanApi.Internal;
using EtherscanApi.Models;

namespace EtherscanApi;

/// <summary>
/// Legacy (V1) metadata exports served from <c>api-metadata.etherscan.io</c>. Prefer <see cref="NametagsModule"/>.
/// </summary>
internal sealed class LegacyMetadataModule : EtherscanModule, ILegacyMetadataModule
{
    internal LegacyMetadataModule(EtherscanTransport transport)
        : base(transport)
    {
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<LabelInfo>> GetLabelMasterListAsync(CancellationToken cancellationToken = default)
    {
        var request = new EtherscanRequest("nametag", "getlabelmasterlist", chain: null)
        {
            Host = EtherscanHost.Metadata,
            Path = "v1/api.ashx",
        };
        return Transport.SendListAsync(request, Json.ListLabelInfo, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Stream> ExportAddressTagsCsvAsync(string label, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(label);
        var request = new EtherscanRequest("nametag", "exportaddresstags", chain: null)
        {
            Host = EtherscanHost.Metadata,
            Path = "v1/api.ashx",
            EndpointRequestsPerSecond = 2,
            IsIdempotent = false,
        }
            .Add("label", label.Trim())
            .Add("format", "csv");
        return Transport.SendForStreamAsync(request, cancellationToken);
    }
}
