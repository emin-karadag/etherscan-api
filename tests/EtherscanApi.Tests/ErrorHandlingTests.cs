using System.Net;
using EtherscanApi.Models;

namespace EtherscanApi.Tests;

public sealed class ErrorHandlingTests
{
    private const string Address = "0xde0B295669a9FD93d5F28D9Ec85E40f4cb697BAe";

    public static TheoryData<string, Type> ErrorMessages => new()
    {
        { "Max rate limit reached, please use API Key for higher rate limit", typeof(EtherscanRateLimitException) },
        { "Invalid API Key", typeof(EtherscanAuthenticationException) },
        { "Too many invalid api key attempts, please try again later", typeof(EtherscanAuthenticationException) },
        { "Missing or unsupported chainid parameter (required for v2 api), please see chainlist for the list of supported chainids", typeof(EtherscanUnsupportedChainException) },
        { "Free API access is not supported for this chain. Please upgrade your api plan for full chain coverage.", typeof(EtherscanPlanRequiredException) },
        { "Error! Missing Or invalid Action name", typeof(EtherscanPlanRequiredException) },
        { "Query Timeout occured. Please select a smaller result dataset", typeof(EtherscanQueryTimeoutException) },
        { "Unexpected err, timeout occurred or server too busy. Please try again later", typeof(EtherscanQueryTimeoutException) },
        { "Community Free API limit reached. Resets 2026-06-24 12:00:00 UTC. For higher, uninterrupted limits, please upgrade to API Pro: https://etherscan.io/apis", typeof(EtherscanQuotaExceededException) },
        { "Error! Invalid address format", typeof(EtherscanApiException) },
    };

    [Theory]
    [MemberData(nameof(ErrorMessages))]
    public async Task Documented_error_messages_map_to_typed_exceptions(string result, Type expected)
    {
        using var client = TestClient.Create(FakeHttpMessageHandler.Json(TestClient.NotOk(result)), o => o.MaxRetryAttempts = 0);

        var ex = await Assert.ThrowsAnyAsync<EtherscanApiException>(() => client.Accounts.GetBalanceAsync(Address));

        Assert.Equal(expected, ex.GetType());
        Assert.Equal(result, ex.ApiResult);
        Assert.Equal("account", ex.Module);
        Assert.Equal("balance", ex.Action);
        Assert.Equal(ChainId.EthereumMainnet, ex.Chain);
    }

    [Fact]
    public async Task Quota_exception_parses_reset_time()
    {
        using var client = TestClient.Create(FakeHttpMessageHandler.Json(TestClient.NotOk(
            "Community Free API limit reached. Resets 2026-06-24 12:00:00 UTC. For higher limits upgrade.")));

        var ex = await Assert.ThrowsAsync<EtherscanQuotaExceededException>(() => client.Accounts.GetBalanceAsync(Address));

        Assert.Equal(new DateTimeOffset(2026, 6, 24, 12, 0, 0, TimeSpan.Zero), ex.ResetsAt);
    }

    [Fact]
    public async Task Api_key_never_appears_in_exception_text()
    {
        using var client = TestClient.Create(FakeHttpMessageHandler.Json(TestClient.NotOk("Invalid API Key")));

        var ex = await Assert.ThrowsAsync<EtherscanAuthenticationException>(() => client.Accounts.GetBalanceAsync(Address));

        Assert.DoesNotContain(TestClient.ApiKey, ex.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain(TestClient.ApiKey, ex.RequestUri, StringComparison.Ordinal);
        Assert.Contains("apikey=***", ex.RequestUri, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Api_key_never_appears_in_http_exception_text()
    {
        var handler = new FakeHttpMessageHandler((_, _) => new HttpResponseMessage(HttpStatusCode.BadGateway));
        using var client = TestClient.Create(handler, o => o.MaxRetryAttempts = 0);

        var ex = await Assert.ThrowsAsync<EtherscanHttpException>(() => client.Accounts.GetBalanceAsync(Address));

        Assert.Equal(HttpStatusCode.BadGateway, ex.StatusCode);
        Assert.DoesNotContain(TestClient.ApiKey, ex.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Rpc_error_raises_rpc_exception()
    {
        using var client = TestClient.Create(FakeHttpMessageHandler.Json(
            """{"jsonrpc":"2.0","id":1,"error":{"code":-32000,"message":"rlp: value size exceeds available input length"}}"""));

        var ex = await Assert.ThrowsAsync<EtherscanRpcException>(() => client.Proxy.SendRawTransactionAsync("0xf86c"));

        Assert.Equal(-32000, ex.RpcCode);
        Assert.Equal("rlp: value size exceeds available input length", ex.RpcMessage);
    }

    [Fact]
    public async Task Proxy_throttling_in_standard_envelope_is_classified()
    {
        using var client = TestClient.Create(
            FakeHttpMessageHandler.Json(TestClient.NotOk("Max rate limit reached, please use API Key for higher rate limit")),
            o => o.MaxRetryAttempts = 0);

        await Assert.ThrowsAsync<EtherscanRateLimitException>(() => client.Proxy.GetBlockNumberAsync());
    }

    [Fact]
    public async Task Non_json_response_raises_response_exception()
    {
        var handler = new FakeHttpMessageHandler((_, _) => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("<html>Cloudflare</html>"),
        });
        using var client = TestClient.Create(handler);

        var ex = await Assert.ThrowsAsync<EtherscanResponseException>(() => client.Accounts.GetBalanceAsync(Address));

        Assert.Contains("Cloudflare", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Echoed_api_key_in_non_json_body_is_masked()
    {
        var handler = new FakeHttpMessageHandler((request, _) => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("<html>bad request " + request.Uri + "</html>"),
        });
        using var client = TestClient.Create(handler);

        var ex = await Assert.ThrowsAsync<EtherscanResponseException>(() => client.Accounts.GetBalanceAsync(Address));

        Assert.DoesNotContain(TestClient.ApiKey, ex.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Rate_limit_errors_are_retried_for_reads()
    {
        var handler = FakeHttpMessageHandler.Sequence(
            TestClient.NotOk("Max rate limit reached"),
            TestClient.NotOk("Query Timeout occured. Please select a smaller result dataset"),
            TestClient.Ok("\"42\""));
        using var client = TestClient.Create(handler, o => o.MaxRetryAttempts = 3);

        var balance = await client.Accounts.GetBalanceAsync(Address);

        Assert.Equal(42, (int)balance);
        Assert.Equal(3, handler.Requests.Count);
    }

    [Fact]
    public async Task Transient_http_errors_are_retried_for_reads()
    {
        var handler = new FakeHttpMessageHandler((_, index) => index == 0
            ? new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            : FakeHttpMessageHandler.JsonResponse(TestClient.Ok("\"1\"")));
        using var client = TestClient.Create(handler);

        await client.Accounts.GetBalanceAsync(Address);

        Assert.Equal(2, handler.Requests.Count);
    }

    [Fact]
    public async Task Retries_stop_after_max_attempts()
    {
        var handler = FakeHttpMessageHandler.Json(TestClient.NotOk("Max rate limit reached"));
        using var client = TestClient.Create(handler, o => o.MaxRetryAttempts = 2);

        await Assert.ThrowsAsync<EtherscanRateLimitException>(() => client.Accounts.GetBalanceAsync(Address));

        Assert.Equal(3, handler.Requests.Count);
    }

    [Fact]
    public async Task Authentication_errors_are_never_retried()
    {
        var handler = FakeHttpMessageHandler.Json(TestClient.NotOk("Invalid API Key"));
        using var client = TestClient.Create(handler, o => o.MaxRetryAttempts = 5);

        await Assert.ThrowsAsync<EtherscanAuthenticationException>(() => client.Accounts.GetBalanceAsync(Address));

        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task Send_raw_transaction_is_never_retried()
    {
        var handler = FakeHttpMessageHandler.Json(TestClient.NotOk("Max rate limit reached"));
        using var client = TestClient.Create(handler, o => o.MaxRetryAttempts = 5);

        await Assert.ThrowsAsync<EtherscanRateLimitException>(() => client.Proxy.SendRawTransactionAsync("0xf86c"));

        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task Verification_submissions_are_never_retried()
    {
        var handler = new FakeHttpMessageHandler((_, _) => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        using var client = TestClient.Create(handler, o => o.MaxRetryAttempts = 5);

        await Assert.ThrowsAsync<EtherscanHttpException>(() => client.Contracts.VerifyProxyAsync(Address));

        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task Pending_verification_status_is_not_an_error()
    {
        using var client = TestClient.Create(FakeHttpMessageHandler.Json(TestClient.NotOk("Pending in queue")));

        var status = await client.Contracts.CheckVerificationStatusAsync("guid");

        Assert.Equal(VerificationState.Pending, status.State);
    }

    [Fact]
    public async Task Invalid_arguments_fail_before_any_request()
    {
        var handler = FakeHttpMessageHandler.Json(TestClient.Ok("\"1\""));
        using var client = TestClient.Create(handler);

        await Assert.ThrowsAsync<ArgumentException>(() => client.Accounts.GetBalanceAsync("not-an-address"));
        await Assert.ThrowsAsync<ArgumentException>(() => client.Accounts.GetBalancesAsync(Enumerable.Repeat(Address, 21)));
        await Assert.ThrowsAsync<ArgumentException>(() => client.Contracts.GetContractCreationAsync(Enumerable.Repeat(Address, 6)));
        await Assert.ThrowsAsync<ArgumentException>(() => client.Proxy.GetTransactionByHashAsync("0x1234"));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.Tokens.GetTopHoldersAsync(Address, 1001));
        await Assert.ThrowsAsync<ArgumentException>(() => client.Accounts.GetErc20TransfersAsync());
        await Assert.ThrowsAsync<ArgumentException>(() => client.Logs.GetLogsAsync(new LogQuery { FromBlock = 1, ToBlock = 2 }));
        await Assert.ThrowsAsync<ArgumentException>(() => client.Contracts.VerifySolidityAsync(new SolidityVerificationRequest
        {
            ContractAddress = Address,
            SourceCode = "x",
            ContractName = "A",
            CompilerVersion = "v0.8.24",
            CodeFormat = SolidityCodeFormat.SingleFile,
        }));

        Assert.Empty(handler.Requests);
    }
}
