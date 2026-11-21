using System.Net;
using EtherscanApi.Models;

namespace EtherscanApi.Tests;

/// <summary>Timeouts, base addresses, missing results, paging limits, back-pressure and value-type semantics.</summary>
public sealed class RobustnessTests
{
    private const string Address = "0xde0B295669a9FD93d5F28D9Ec85E40f4cb697BAe";

    /// <summary>Sends the status line and headers at once, then never finishes the body.</summary>
    private sealed class StalledContent : HttpContent
    {
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
            => SerializeToStreamAsync(stream, context, CancellationToken.None);

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context, CancellationToken cancellationToken)
            => await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }
    }

    private static EtherscanClient CreateWithTimeout(HttpMessageHandler handler, TimeSpan timeout)
        => new(
            new EtherscanClientOptions { ApiKey = TestClient.ApiKey, EnableRateLimiting = false, MaxRetryAttempts = 0 },
            new HttpClient(handler) { Timeout = timeout });

    [Fact]
    public async Task Client_timeout_covers_a_stalled_response_body()
    {
        var handler = new FakeHttpMessageHandler((_, _) => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StalledContent() });
        using var client = CreateWithTimeout(handler, TimeSpan.FromMilliseconds(300));

        var ex = await Assert.ThrowsAsync<EtherscanHttpException>(() => client.Accounts.GetBalanceAsync(Address));

        Assert.Contains("timed out", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Stream_endpoints_map_client_timeouts_to_http_exception()
    {
        var handler = new DelayedHandler();
        using var client = CreateWithTimeout(handler, TimeSpan.FromMilliseconds(300));

        var ex = await Assert.ThrowsAsync<EtherscanHttpException>(() => client.LegacyMetadata.ExportAddressTagsCsvAsync("phish-hack"));

        Assert.Contains("timed out", ex.Message, StringComparison.Ordinal);
    }

    private sealed class DelayedHandler : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }

    [Fact]
    public async Task Base_address_without_trailing_slash_keeps_its_path()
    {
        var handler = FakeHttpMessageHandler.Json(TestClient.Ok("\"1\""));
        using var client = TestClient.Create(handler, o => o.BaseAddress = new Uri("https://gateway.example/etherscan"));

        await client.Accounts.GetBalanceAsync(Address);

        Assert.Equal("/etherscan/v2/api", handler.Requests[0].Uri.AbsolutePath);
    }

    [Fact]
    public async Task Null_result_on_success_raises_response_exception()
    {
        using var client = TestClient.Create(FakeHttpMessageHandler.Json("""{"status":"1","message":"OK","result":null}"""));

        await Assert.ThrowsAsync<EtherscanResponseException>(() => client.Contracts.GetAbiAsync(Address));
    }

    [Fact]
    public async Task Required_rpc_result_missing_raises_response_exception()
    {
        using var client = TestClient.Create(FakeHttpMessageHandler.Json(TestClient.Rpc("null")));

        await Assert.ThrowsAsync<EtherscanResponseException>(() => client.Proxy.GetBlockNumberAsync());
        await Assert.ThrowsAsync<EtherscanResponseException>(() => client.Proxy.GetGasPriceAsync());
        await Assert.ThrowsAsync<EtherscanResponseException>(() => client.Proxy.SendRawTransactionAsync("0xf86c"));
    }

    [Fact]
    public async Task Optional_rpc_result_missing_returns_null()
    {
        using var client = TestClient.Create(FakeHttpMessageHandler.Json(TestClient.Rpc("null")));

        Assert.Null(await client.Proxy.GetTransactionByHashAsync("0x2f1c5c2b44f771e942a8506148e256f94f1a464babc938ae0690c6e34cd79190"));
        Assert.Null(await client.Proxy.GetBlockByNumberAsync(BlockTag.Latest));
    }

    [Fact]
    public async Task Null_list_result_is_an_empty_list()
    {
        using var client = TestClient.Create(FakeHttpMessageHandler.Json("""{"status":"1","message":"OK","result":null}"""));

        Assert.Empty(await client.Accounts.GetTransactionsAsync(Address));
    }

    [Fact]
    public void Page_size_is_validated_when_the_method_is_called()
    {
        var handler = FakeHttpMessageHandler.Json(TestClient.Ok("[]"));
        using var client = TestClient.Create(handler);

        Assert.Throws<ArgumentOutOfRangeException>(() => client.Accounts.EnumerateTransactionsAsync(Address, pageSize: 10_001));
        Assert.Throws<ArgumentOutOfRangeException>(() => client.Accounts.EnumerateTransactionsAsync(Address, pageSize: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => client.Logs.EnumerateLogsAsync(
            new LogQuery { FromBlock = 1, ToBlock = 2, Address = Address }, pageSize: 1001));
        Assert.Throws<ArgumentOutOfRangeException>(() => client.Tokens.EnumerateHoldersAsync(Address, pageSize: 10_001));
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task Default_page_size_is_one_thousand()
    {
        var handler = FakeHttpMessageHandler.Json(TestClient.Ok("[]"));
        using var client = TestClient.Create(handler);

        await foreach (var _ in client.Accounts.EnumerateTransactionsAsync(Address))
        {
        }

        Assert.Equal("1000", handler.Requests[0].Query["offset"]);
        Assert.Equal("1", handler.Requests[0].Query["page"]);
    }

    [Fact]
    public async Task Full_client_side_queue_is_reported_as_client_side()
    {
        var handler = FakeHttpMessageHandler.Json(TestClient.Ok("\"1\""));
        using var client = TestClient.Create(handler, o =>
        {
            o.EnableRateLimiting = true;
            o.RequestsPerSecond = 1;
            o.MaxQueuedRequests = 0;
        });

        var calls = Enumerable.Range(0, 3).Select(_ => client.Accounts.GetBalanceAsync(Address)).ToArray();
        var outcomes = await Task.WhenAll(calls.Select(async call =>
        {
            try
            {
                await call;
                return null;
            }
            catch (EtherscanRateLimitException ex)
            {
                return ex;
            }
        }));

        var rejected = Assert.Single(outcomes, o => o is not null);
        Assert.True(rejected!.IsClientSide);
    }

    [Fact]
    public void Queue_limit_is_bounded()
    {
        Assert.Throws<InvalidOperationException>(() => new EtherscanClient(new EtherscanClientOptions
        {
            ApiKey = "k",
            MaxQueuedRequests = int.MaxValue,
        }));
    }

    [Fact]
    public async Task Block_tag_default_is_latest_and_negative_numbers_fail_when_sent()
    {
        Assert.Equal(BlockTag.Latest, default);
        Assert.Equal("latest", default(BlockTag).ToHexString());

        BlockTag negative = -1; // the implicit conversion itself never throws
        Assert.True(negative.IsNumber);

        var handler = FakeHttpMessageHandler.Json(TestClient.Rpc("null"));
        using var client = TestClient.Create(handler);
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.Proxy.GetBlockByNumberAsync(negative));
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task Wait_for_verification_rejects_non_positive_timeout()
    {
        using var client = TestClient.Create(FakeHttpMessageHandler.Json(TestClient.Ok("\"Pass - Verified\"")));

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.Contracts.WaitForVerificationAsync("guid", timeout: TimeSpan.Zero));
        var status = await client.Contracts.WaitForVerificationAsync("guid");
        Assert.Equal(VerificationState.Success, status.State);
    }
}
