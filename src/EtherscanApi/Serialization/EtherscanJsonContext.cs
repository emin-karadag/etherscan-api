using System.Globalization;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using EtherscanApi.Models;

namespace EtherscanApi.Serialization;

/// <summary>Reads a chain id sent as a string or number.</summary>
internal sealed class ChainIdJsonConverter : JsonConverter<ChainId>
{
    public override ChainId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var text = EtherscanNumber.ReadText(ref reader);
        return ulong.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var value)
            ? new ChainId(value)
            : throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, ChainId value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}

/// <summary>
/// Source-generated serialization metadata for every Etherscan result type. Keeps the library
/// trimming- and Native AOT-compatible (no reflection fallback).
/// </summary>
[JsonSourceGenerationOptions(
    // The client only deserializes, so the serialization fast-path code is not generated (smaller assembly).
    GenerationMode = JsonSourceGenerationMode.Metadata,
    PropertyNameCaseInsensitive = true,
    NumberHandling = JsonNumberHandling.AllowReadingFromString,
    Converters =
    [
        typeof(FlexibleInt64Converter),
        typeof(FlexibleNullableInt64Converter),
        typeof(FlexibleInt32Converter),
        typeof(FlexibleNullableInt32Converter),
        typeof(FlexibleBigIntegerConverter),
        typeof(FlexibleNullableBigIntegerConverter),
        typeof(FlexibleDecimalConverter),
        typeof(FlexibleNullableDecimalConverter),
        typeof(FlexibleBooleanConverter),
        typeof(FlexibleNullableBooleanConverter),
        typeof(UnixTimestampConverter),
        typeof(NullableUnixTimestampConverter),
        typeof(FlexibleDateOnlyConverter),
        typeof(FlexibleStringConverter),
        typeof(ChainIdJsonConverter),
    ])]
// Scalars
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(long))]
[JsonSerializable(typeof(BigInteger))]
// Accounts
[JsonSerializable(typeof(List<AddressBalance>))]
[JsonSerializable(typeof(List<NormalTransaction>))]
[JsonSerializable(typeof(List<InternalTransaction>))]
[JsonSerializable(typeof(List<Erc20Transfer>))]
[JsonSerializable(typeof(List<Erc721Transfer>))]
[JsonSerializable(typeof(List<Erc1155Transfer>))]
[JsonSerializable(typeof(List<MinedBlock>))]
[JsonSerializable(typeof(List<BeaconWithdrawal>))]
[JsonSerializable(typeof(FundingSource))]
// Tokens
[JsonSerializable(typeof(List<TopTokenHolder>))]
[JsonSerializable(typeof(List<TokenHolder>))]
[JsonSerializable(typeof(List<TokenInfo>))]
[JsonSerializable(typeof(List<AddressTokenBalance>))]
[JsonSerializable(typeof(List<AddressNftBalance>))]
[JsonSerializable(typeof(List<NftInventoryItem>))]
// Blocks
[JsonSerializable(typeof(BlockReward))]
[JsonSerializable(typeof(BlockTransactionCounts))]
[JsonSerializable(typeof(BlockCountdown))]
[JsonSerializable(typeof(List<DailyAverageBlockSize>))]
[JsonSerializable(typeof(List<DailyBlockCount>))]
[JsonSerializable(typeof(List<DailyBlockRewards>))]
[JsonSerializable(typeof(List<DailyAverageBlockTime>))]
[JsonSerializable(typeof(List<DailyUncleBlockCount>))]
// Contracts
[JsonSerializable(typeof(List<ContractSourceCode>))]
[JsonSerializable(typeof(List<ContractCreation>))]
// Gas tracker and stats
[JsonSerializable(typeof(GasOracle))]
[JsonSerializable(typeof(List<DailyAverageGasLimit>))]
[JsonSerializable(typeof(List<DailyGasUsed>))]
[JsonSerializable(typeof(List<DailyAverageGasPrice>))]
[JsonSerializable(typeof(EtherSupplyDetails))]
[JsonSerializable(typeof(NativeTokenPrice))]
[JsonSerializable(typeof(List<ChainSizeEntry>))]
[JsonSerializable(typeof(NodeCount))]
[JsonSerializable(typeof(List<NodeCount>))]
[JsonSerializable(typeof(List<DailyTransactionFee>))]
[JsonSerializable(typeof(List<DailyNewAddressCount>))]
[JsonSerializable(typeof(List<DailyEnsRegistrationCount>))]
[JsonSerializable(typeof(List<DailyNetworkUtilization>))]
[JsonSerializable(typeof(List<DailyAverageHashRate>))]
[JsonSerializable(typeof(List<DailyTransactionCount>))]
[JsonSerializable(typeof(List<DailyAverageNetworkDifficulty>))]
[JsonSerializable(typeof(List<DailyNativeTokenPrice>))]
// Transactions
[JsonSerializable(typeof(ContractExecutionStatus))]
[JsonSerializable(typeof(TransactionReceiptStatus))]
// Proxy and logs
[JsonSerializable(typeof(RpcBlock))]
[JsonSerializable(typeof(RpcBlockWithTransactions))]
[JsonSerializable(typeof(RpcTransaction))]
[JsonSerializable(typeof(RpcTransactionReceipt))]
[JsonSerializable(typeof(List<EventLog>))]
// L2
[JsonSerializable(typeof(List<PlasmaDeposit>))]
[JsonSerializable(typeof(List<L2DepositTransaction>))]
[JsonSerializable(typeof(List<L2WithdrawalTransaction>))]
// Nametags and metadata
[JsonSerializable(typeof(List<AddressMetadata>))]
[JsonSerializable(typeof(List<LabelInfo>))]
// Usage and chains
[JsonSerializable(typeof(ApiUsage))]
[JsonSerializable(typeof(List<ChainInfo>))]
internal sealed partial class EtherscanJsonContext : JsonSerializerContext;
