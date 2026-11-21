using System.Text.Json;

namespace EtherscanApi.Internal;

/// <summary>
/// The top-level fields of an Etherscan response, read in one forward pass with <see cref="Utf8JsonReader"/>.
/// The <c>result</c> value is located by byte range, so it can be deserialized straight from the response
/// buffer without building a <see cref="JsonDocument"/>.
/// </summary>
internal readonly ref struct ScannedEnvelope
{
    public ScannedEnvelope(
        ReadOnlySpan<byte> result,
        JsonTokenType resultTokenType,
        string? resultString,
        string? status,
        string? message,
        bool isJsonRpc,
        bool hasError,
        int errorCode,
        string? errorMessage)
    {
        Result = result;
        ResultTokenType = resultTokenType;
        ResultString = resultString;
        Status = status;
        Message = message;
        IsJsonRpc = isJsonRpc;
        HasError = hasError;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    /// <summary>The raw UTF-8 bytes of the <c>result</c> value; empty when absent.</summary>
    public ReadOnlySpan<byte> Result { get; }

    /// <summary>The token type of <c>result</c>; <see cref="JsonTokenType.None"/> when absent.</summary>
    public JsonTokenType ResultTokenType { get; }

    /// <summary>The <c>result</c> value when it is a JSON string.</summary>
    public string? ResultString { get; }

    public string? Status { get; }

    public string? Message { get; }

    public bool IsJsonRpc { get; }

    public bool HasError { get; }

    public int ErrorCode { get; }

    public string? ErrorMessage { get; }

    public bool HasResult => ResultTokenType != JsonTokenType.None;

    public bool ResultIsNull => ResultTokenType is JsonTokenType.None or JsonTokenType.Null;

    public bool ResultIsEmptyArray
    {
        get
        {
            if (ResultTokenType != JsonTokenType.StartArray)
            {
                return false;
            }

            // "[" whitespace* "]"
            foreach (var b in Result[1..^1])
            {
                if (b is not ((byte)' ' or (byte)'\t' or (byte)'\r' or (byte)'\n'))
                {
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>The result as text for error messages: the string value, or the raw JSON otherwise.</summary>
    public string? ResultText => ResultTokenType switch
    {
        JsonTokenType.String => ResultString,
        JsonTokenType.None or JsonTokenType.Null => null,
        _ => System.Text.Encoding.UTF8.GetString(Result),
    };
}

internal static class EnvelopeScanner
{
    /// <summary>Reads the top-level object. Throws <see cref="JsonException"/> for invalid JSON or a non-object root.</summary>
    public static ScannedEnvelope Scan(ReadOnlySpan<byte> json)
    {
        var reader = new Utf8JsonReader(json, new JsonReaderOptions { CommentHandling = JsonCommentHandling.Skip });
        if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("The response is not a JSON object.");
        }

        ReadOnlySpan<byte> result = default;
        var resultType = JsonTokenType.None;
        string? resultString = null, status = null, message = null, errorMessage = null;
        bool isJsonRpc = false, hasError = false;
        var errorCode = 0;

        while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
        {
            if (reader.ValueTextEquals("result"u8))
            {
                reader.Read();
                var start = (int)reader.TokenStartIndex;
                resultType = reader.TokenType;
                if (resultType == JsonTokenType.String)
                {
                    resultString = reader.GetString();
                }

                reader.Skip();
                result = json[start..(int)reader.BytesConsumed];
            }
            else if (reader.ValueTextEquals("status"u8))
            {
                reader.Read();
                status = ReadScalar(ref reader);
            }
            else if (reader.ValueTextEquals("message"u8))
            {
                reader.Read();
                message = ReadScalar(ref reader);
            }
            else if (reader.ValueTextEquals("jsonrpc"u8))
            {
                isJsonRpc = true;
                reader.Read();
                reader.Skip();
            }
            else if (reader.ValueTextEquals("error"u8))
            {
                reader.Read();
                if (reader.TokenType == JsonTokenType.StartObject)
                {
                    hasError = true;
                    (errorCode, errorMessage) = ReadRpcError(ref reader);
                }
                else
                {
                    reader.Skip();
                }
            }
            else
            {
                reader.Read();
                reader.Skip();
            }
        }

        return new ScannedEnvelope(result, resultType, resultString, status, message, isJsonRpc, hasError, errorCode, errorMessage);
    }

    private static string? ReadScalar(ref Utf8JsonReader reader)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                return reader.GetString();
            case JsonTokenType.Number:
                return System.Text.Encoding.UTF8.GetString(reader.ValueSpan);
            default:
                reader.Skip();
                return null;
        }
    }

    private static (int Code, string? Message) ReadRpcError(ref Utf8JsonReader reader)
    {
        var code = 0;
        string? message = null;
        while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
        {
            if (reader.ValueTextEquals("code"u8))
            {
                reader.Read();
                if (reader.TokenType != JsonTokenType.Number || !reader.TryGetInt32(out code))
                {
                    reader.Skip();
                }
            }
            else if (reader.ValueTextEquals("message"u8))
            {
                reader.Read();
                message = ReadScalar(ref reader);
            }
            else
            {
                reader.Read();
                reader.Skip();
            }
        }

        return (code, message);
    }
}
