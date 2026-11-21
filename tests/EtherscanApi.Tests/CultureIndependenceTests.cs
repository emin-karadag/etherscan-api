using System.Globalization;
using System.Numerics;

namespace EtherscanApi.Tests;

/// <summary>
/// The library must parse and format identically under any current culture. Turkish (tr-TR) is the classic
/// trap: ',' is the decimal separator, '.' groups thousands, and "I".ToLower() is "ı".
/// </summary>
public sealed class CultureIndependenceTests
{
    private const string Address = "0xde0B295669a9FD93d5F28D9Ec85E40f4cb697BAe";

    [Theory]
    [InlineData("tr-TR")]
    [InlineData("de-DE")]
    [InlineData("ar-SA")]
    public async Task Parsing_and_requests_ignore_the_current_culture(string cultureName)
    {
        var original = (CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture);
        var culture = CultureInfo.GetCultureInfo(cultureName);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        try
        {
            var handler = FakeHttpMessageHandler.Sequence(
                TestClient.Ok("""{"LastBlock":"23467872","SafeGasPrice":"0.496839934","ProposeGasPrice":"0.5","FastGasPrice":"0.55","suggestBaseFee":"0.4","gasUsedRatio":"0.4,0.78"}"""),
                TestClient.Ok("""[{"UTCDate":"2019-02-01","unixTimeStamp":"1548979200","networkDifficulty":"2,408.028"}]"""),
                TestClient.Ok("""[{"blockNumber":"0xc48174","timeStamp":"0x60f9ce56","gasPrice":"0x2e90edd000","gasUsed":"0x1","logIndex":"0x","transactionIndex":"0x","address":"0x1","transactionHash":"0x2","topics":[]}]"""),
                TestClient.Ok("\"122373866217800000000000000\""));
            using var client = TestClient.Create(handler);

            var oracle = await client.GasTracker.GetGasOracleAsync();
            var difficulty = Assert.Single(await client.Stats.GetDailyAverageNetworkDifficultyAsync(new(2019, 2, 1), new(2019, 2, 1), SortOrder.Ascending));
            var log = Assert.Single(await client.Logs.GetLogsAsync(new LogQuery
            {
                FromBlock = 1_000,
                ToBlock = 2_000,
                Address = Address,
            }));
            var supply = await client.Stats.GetEtherSupplyAsync();

            Assert.Equal(0.55m, oracle.FastGasPrice);
            Assert.Equal([0.4m, 0.78m], oracle.GetGasUsedRatios());
            Assert.Equal(2408.028m, difficulty.NetworkDifficulty);
            Assert.Equal(new DateOnly(2019, 2, 1), difficulty.UtcDate);
            Assert.Equal(0xc48174, log.BlockNumber);
            Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(0x60f9ce56), log.TimeStamp);
            Assert.Equal(BigInteger.Parse("122373866217800000000000000", CultureInfo.InvariantCulture), supply);

            // Outgoing parameters use invariant formats regardless of culture.
            Assert.Equal("asc", handler.Requests[1].Query["sort"]);
            Assert.Equal("2019-02-01", handler.Requests[1].Query["startdate"]);
            Assert.Equal("1000", handler.Requests[2].Query["fromBlock"]);
            Assert.Equal("1", handler.Requests[0].Query["chainid"]);
        }
        finally
        {
            (CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture) = original;
        }
    }
}
