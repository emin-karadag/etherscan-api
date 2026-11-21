using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Web;

namespace EtherscanApi.Tests;

internal sealed record RecordedRequest(HttpMethod Method, Uri Uri, string? Body)
{
    public NameValueCollection Query { get; } = HttpUtility.ParseQueryString(Uri.Query);

    public NameValueCollection Form { get; } = HttpUtility.ParseQueryString(Body ?? "");
}

/// <summary>An HttpMessageHandler that records requests and returns canned responses.</summary>
internal sealed class FakeHttpMessageHandler(Func<RecordedRequest, int, HttpResponseMessage> responder) : HttpMessageHandler
{
    public List<RecordedRequest> Requests { get; } = [];

    public static FakeHttpMessageHandler Json(string json)
        => new((_, _) => JsonResponse(json));

    public static FakeHttpMessageHandler Sequence(params string[] jsons)
        => new((_, index) => JsonResponse(jsons[Math.Min(index, jsons.Length - 1)]));

    public static HttpResponseMessage JsonResponse(string json, HttpStatusCode status = HttpStatusCode.OK)
        => new(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
        var recorded = new RecordedRequest(request.Method, request.RequestUri!, body);
        int index;
        lock (Requests)
        {
            index = Requests.Count;
            Requests.Add(recorded);
        }

        return responder(recorded, index);
    }
}

internal static class TestClient
{
    public const string ApiKey = "TEST-SECRET-KEY-123";

    public static EtherscanClient Create(
        FakeHttpMessageHandler handler,
        Action<EtherscanClientOptions>? configure = null)
    {
        var options = new EtherscanClientOptions
        {
            ApiKey = ApiKey,
            EnableRateLimiting = false,
            RetryBaseDelay = TimeSpan.Zero,
        };
        configure?.Invoke(options);
        return new EtherscanClient(options, new HttpClient(handler));
    }

    public static string Ok(string resultJson) => $$"""{"status":"1","message":"OK","result":{{resultJson}}}""";

    public static string Rpc(string resultJson) => $$"""{"jsonrpc":"2.0","id":1,"result":{{resultJson}}}""";

    public static string NotOk(string result, string message = "NOTOK")
        => $$"""{"status":"0","message":"{{message}}","result":"{{result}}"}""";
}
