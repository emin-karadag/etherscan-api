using System.Text;
using System.Text.Json;
using EtherscanApi.Internal;
using EtherscanApi.Serialization;

namespace EtherscanApi.Tests;

/// <summary>
/// Guards the allocation behaviour of response parsing: scanning the envelope and deserializing
/// <c>result</c> from the UTF-8 buffer must produce the same rows and allocate no more than building a
/// <see cref="JsonDocument"/> first (measured: about equal, since JsonDocument uses pooled buffers).
/// </summary>
public sealed class ParsingAllocationTests(ITestOutputHelper output)
{
    private static byte[] BuildTxlist(int rows)
    {
        var sb = new StringBuilder("""{"status":"1","message":"OK","result":[""");
        for (var i = 0; i < rows; i++)
        {
            if (i > 0)
            {
                sb.Append(',');
            }

            sb.Append(System.Globalization.CultureInfo.InvariantCulture, $$"""{"blockNumber":"{{23467053 + i}}","blockHash":"0xf5646226f819fbdd6b2f1cb99de6e5d17da3d876ec166e69f4e9736c8ebf7ab4","timeStamp":"1759129619","hash":"0xf9db905d77704596d3600816bc70201586cfeec13bcf576320e2f38d6ca851a0","nonce":"8","transactionIndex":"184","from":"0x2449ecef5012f0a0e153b278ef4fcc9625bc4c78","to":"0xc5102fe9359fd9a28f877a67e36b0f050d81a3cc","value":"1000000000000000000","gas":"73271","gasPrice":"238744402","input":"0x5c19a95c","methodId":"0x5c19a95c","functionName":"delegate(address to)","contractAddress":"","cumulativeGasUsed":"22498564","txreceipt_status":"1","gasUsed":"48847","confirmations":"589","isError":"0"}""");
        }

        return Encoding.UTF8.GetBytes(sb.Append("]}").ToString());
    }

    [Fact]
    public void Span_based_parsing_matches_dom_parsing_without_extra_allocations()
    {
        var body = BuildTxlist(1000);
        var typeInfo = EtherscanJsonContext.Default.ListNormalTransaction;

        // Warm up both paths (type metadata, converters).
        _ = ParseWithScanner(body, typeInfo);
        _ = ParseWithDocument(body, typeInfo);

        var before = GC.GetAllocatedBytesForCurrentThread();
        var viaScanner = ParseWithScanner(body, typeInfo);
        var scannerBytes = GC.GetAllocatedBytesForCurrentThread() - before;

        before = GC.GetAllocatedBytesForCurrentThread();
        var viaDocument = ParseWithDocument(body, typeInfo);
        var documentBytes = GC.GetAllocatedBytesForCurrentThread() - before;

        output.WriteLine($"1000 rows ({body.Length / 1024} KB): scanner {scannerBytes / 1024} KB, JsonDocument {documentBytes / 1024} KB");
        Assert.Equal(1000, viaScanner.Count);
        Assert.Equal(viaDocument.Count, viaScanner.Count);
        Assert.Equal(viaDocument[^1], viaScanner[^1]);
        Assert.True(scannerBytes <= documentBytes * 1.02, $"scanner {scannerBytes} > document {documentBytes}");
        // Per row: roughly the model object and its strings; numeric fields add no string allocations.
        Assert.True(scannerBytes / 1000 < 1200, $"{scannerBytes / 1000} bytes per row");
    }

    private static List<Models.NormalTransaction> ParseWithScanner(byte[] body, System.Text.Json.Serialization.Metadata.JsonTypeInfo<List<Models.NormalTransaction>> typeInfo)
    {
        var envelope = EnvelopeScanner.Scan(body);
        return JsonSerializer.Deserialize(envelope.Result, typeInfo)!;
    }

    private static List<Models.NormalTransaction> ParseWithDocument(byte[] body, System.Text.Json.Serialization.Metadata.JsonTypeInfo<List<Models.NormalTransaction>> typeInfo)
    {
        using var document = JsonDocument.Parse(body);
        return document.RootElement.GetProperty("result").Deserialize(typeInfo)!;
    }
}
