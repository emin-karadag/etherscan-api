using System.Buffers;
using System.Globalization;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EtherscanApi.Serialization;

/// <summary>
/// Number parsing shared by the converters. Etherscan sends numbers as decimal strings (<c>"23467053"</c>),
/// hex quantities (<c>"0xc48174"</c>, <c>"0x"</c>), formatted decimals (<c>"2,408.028"</c>), or plain JSON numbers.
/// </summary>
internal static class EtherscanNumber
{
    public static bool TryParseBigInteger(ReadOnlySpan<char> text, out BigInteger value)
    {
        text = text.Trim();
        if (text.IsEmpty)
        {
            value = BigInteger.Zero;
            return false;
        }

        if (text.Length >= 2 && text[0] == '0' && (text[1] == 'x' || text[1] == 'X'))
        {
            var hex = text[2..];
            if (hex.IsEmpty)
            {
                // "0x" is the empty quantity.
                value = BigInteger.Zero;
                return true;
            }

            // Prefix a zero so the parser never reads the value as a negative two's complement number.
            Span<char> buffer = hex.Length < 256 ? stackalloc char[hex.Length + 1] : new char[hex.Length + 1];
            buffer[0] = '0';
            hex.CopyTo(buffer[1..]);
            return BigInteger.TryParse(buffer, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out value);
        }

        if (BigInteger.TryParse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value))
        {
            return true;
        }

        // Scientific notation or integral decimals such as "1e18" or "5.0".
        if (decimal.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var d)
            && decimal.Truncate(d) == d)
        {
            value = new BigInteger(d);
            return true;
        }

        return false;
    }

    public static bool TryParseInt64(ReadOnlySpan<char> text, out long value)
    {
        // Fast path: plain decimal integers, the common case (block numbers, gas, nonces, timestamps).
        if (long.TryParse(text, NumberStyles.AllowLeadingSign | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, CultureInfo.InvariantCulture, out value))
        {
            return true;
        }

        // Hex quantities ("0xc48174", "0x"), scientific or integral decimals.
        if (TryParseBigInteger(text, out var big) && big >= long.MinValue && big <= long.MaxValue)
        {
            value = (long)big;
            return true;
        }

        value = 0;
        return false;
    }

    public static bool TryParseDecimal(ReadOnlySpan<char> text, out decimal value)
    {
        text = text.Trim();
        if (text.IsEmpty)
        {
            value = 0;
            return false;
        }

        if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            if (TryParseBigInteger(text, out var big) && big <= new BigInteger(decimal.MaxValue))
            {
                value = (decimal)big;
                return true;
            }

            value = 0;
            return false;
        }

        return decimal.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out value);
    }

    public static bool TryParseBoolean(ReadOnlySpan<char> text, out bool value)
    {
        text = text.Trim();
        if (text.Equals("1", StringComparison.Ordinal) || text.Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            value = true;
            return true;
        }

        if (text.Equals("0", StringComparison.Ordinal) || text.Equals("false", StringComparison.OrdinalIgnoreCase))
        {
            value = false;
            return true;
        }

        // Hex quantities such as the receipt status "0x1".
        if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase) && TryParseBigInteger(text, out var big))
        {
            value = !big.IsZero;
            return true;
        }

        value = false;
        return false;
    }

    public static bool TryParseTimestamp(ReadOnlySpan<char> text, out DateTimeOffset value)
    {
        text = text.Trim();
        if (TryParseInt64(text, out var seconds))
        {
            try
            {
                value = DateTimeOffset.FromUnixTimeSeconds(seconds);
                return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                value = default;
                return false;
            }
        }

        return DateTimeOffset.TryParse(
            text,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
            out value);
    }

    /// <summary>
    /// Parses the current scalar token without allocating a string in the common case: the UTF-8 bytes of
    /// unescaped strings and numbers are widened into a stack buffer (Etherscan numbers are ASCII).
    /// Returns <see langword="null"/> for JSON null and blank strings.
    /// </summary>
    public static bool? TryParseToken<T>(ref Utf8JsonReader reader, SpanParser<T> parser, out T value)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                value = default!;
                return null;
            case JsonTokenType.True:
                return parser("true", out value);
            case JsonTokenType.False:
                return parser("false", out value);
            case JsonTokenType.String or JsonTokenType.Number
                when !reader.HasValueSequence && !reader.ValueIsEscaped && Ascii.IsValid(reader.ValueSpan):
            {
                var utf8 = reader.ValueSpan;
                if (utf8.Length <= 128)
                {
                    Span<char> chars = stackalloc char[utf8.Length];
                    for (var i = 0; i < utf8.Length; i++)
                    {
                        chars[i] = (char)utf8[i];
                    }

                    return ParseChars(chars, parser, out value);
                }

                return ParseChars(Encoding.UTF8.GetString(utf8), parser, out value);
            }

            case JsonTokenType.String or JsonTokenType.Number:
                return ParseChars(ReadText(ref reader), parser, out value);
            default:
                throw new JsonException();
        }
    }

    private static bool? ParseChars<T>(ReadOnlySpan<char> text, SpanParser<T> parser, out T value)
    {
        if (text.IsWhiteSpace())
        {
            value = default!;
            return null;
        }

        return parser(text, out value);
    }

    /// <summary>Reads the current token as text: strings are unescaped, numbers and literals use their raw text.</summary>
    public static string? ReadText(ref Utf8JsonReader reader) => reader.TokenType switch
    {
        JsonTokenType.String => reader.GetString(),
        JsonTokenType.Number => reader.HasValueSequence
            ? Encoding.UTF8.GetString(reader.ValueSequence.ToArray())
            : Encoding.UTF8.GetString(reader.ValueSpan),
        JsonTokenType.True => "true",
        JsonTokenType.False => "false",
        JsonTokenType.Null => null,
        _ => throw new JsonException(),
    };
}

/// <summary>Parses text into a value.</summary>
internal delegate bool SpanParser<T>(ReadOnlySpan<char> text, out T value);

/// <summary>Base for converters of value types that Etherscan encodes as strings or numbers.</summary>
internal abstract class FlexibleConverter<T> : JsonConverter<T>
    where T : struct
{
    private readonly SpanParser<T> _parser;

    protected FlexibleConverter()
    {
        _parser = TryParse;
    }

    protected abstract bool TryParse(ReadOnlySpan<char> text, out T value);

    protected abstract string Format(T value);

    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => EtherscanNumber.TryParseToken(ref reader, _parser, out var value) switch
        {
            true => value,
            null => default, // null or blank
            false => throw new JsonException(),
        };

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        => writer.WriteStringValue(Format(value));
}

/// <summary>Base for nullable converters: <c>null</c>, <c>""</c> and unparseable placeholders map to <see langword="null"/>.</summary>
internal abstract class FlexibleNullableConverter<T> : JsonConverter<T?>
    where T : struct
{
    private readonly SpanParser<T> _parser;

    protected FlexibleNullableConverter()
    {
        _parser = TryParse;
    }

    protected abstract bool TryParse(ReadOnlySpan<char> text, out T value);

    protected abstract string Format(T value);

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => EtherscanNumber.TryParseToken(ref reader, _parser, out var value) == true ? value : null;

    public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
    {
        if (value is { } v)
        {
            writer.WriteStringValue(Format(v));
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}

internal sealed class FlexibleInt64Converter : FlexibleConverter<long>
{
    protected override bool TryParse(ReadOnlySpan<char> text, out long value) => EtherscanNumber.TryParseInt64(text, out value);

    protected override string Format(long value) => value.ToString(CultureInfo.InvariantCulture);
}

internal sealed class FlexibleNullableInt64Converter : FlexibleNullableConverter<long>
{
    protected override bool TryParse(ReadOnlySpan<char> text, out long value) => EtherscanNumber.TryParseInt64(text, out value);

    protected override string Format(long value) => value.ToString(CultureInfo.InvariantCulture);
}

internal sealed class FlexibleInt32Converter : FlexibleConverter<int>
{
    protected override bool TryParse(ReadOnlySpan<char> text, out int value)
    {
        if (EtherscanNumber.TryParseInt64(text, out var l) && l is >= int.MinValue and <= int.MaxValue)
        {
            value = (int)l;
            return true;
        }

        value = 0;
        return false;
    }

    protected override string Format(int value) => value.ToString(CultureInfo.InvariantCulture);
}

internal sealed class FlexibleNullableInt32Converter : FlexibleNullableConverter<int>
{
    protected override bool TryParse(ReadOnlySpan<char> text, out int value)
    {
        if (EtherscanNumber.TryParseInt64(text, out var l) && l is >= int.MinValue and <= int.MaxValue)
        {
            value = (int)l;
            return true;
        }

        value = 0;
        return false;
    }

    protected override string Format(int value) => value.ToString(CultureInfo.InvariantCulture);
}

internal sealed class FlexibleBigIntegerConverter : FlexibleConverter<BigInteger>
{
    protected override bool TryParse(ReadOnlySpan<char> text, out BigInteger value) => EtherscanNumber.TryParseBigInteger(text, out value);

    protected override string Format(BigInteger value) => value.ToString(CultureInfo.InvariantCulture);
}

internal sealed class FlexibleNullableBigIntegerConverter : FlexibleNullableConverter<BigInteger>
{
    protected override bool TryParse(ReadOnlySpan<char> text, out BigInteger value) => EtherscanNumber.TryParseBigInteger(text, out value);

    protected override string Format(BigInteger value) => value.ToString(CultureInfo.InvariantCulture);
}

internal sealed class FlexibleDecimalConverter : FlexibleConverter<decimal>
{
    protected override bool TryParse(ReadOnlySpan<char> text, out decimal value) => EtherscanNumber.TryParseDecimal(text, out value);

    protected override string Format(decimal value) => value.ToString(CultureInfo.InvariantCulture);
}

internal sealed class FlexibleNullableDecimalConverter : FlexibleNullableConverter<decimal>
{
    protected override bool TryParse(ReadOnlySpan<char> text, out decimal value) => EtherscanNumber.TryParseDecimal(text, out value);

    protected override string Format(decimal value) => value.ToString(CultureInfo.InvariantCulture);
}

internal sealed class FlexibleBooleanConverter : FlexibleConverter<bool>
{
    protected override bool TryParse(ReadOnlySpan<char> text, out bool value) => EtherscanNumber.TryParseBoolean(text, out value);

    protected override string Format(bool value) => value ? "1" : "0";
}

internal sealed class FlexibleNullableBooleanConverter : FlexibleNullableConverter<bool>
{
    protected override bool TryParse(ReadOnlySpan<char> text, out bool value) => EtherscanNumber.TryParseBoolean(text, out value);

    protected override string Format(bool value) => value ? "1" : "0";
}

internal sealed class UnixTimestampConverter : FlexibleConverter<DateTimeOffset>
{
    protected override bool TryParse(ReadOnlySpan<char> text, out DateTimeOffset value) => EtherscanNumber.TryParseTimestamp(text, out value);

    protected override string Format(DateTimeOffset value) => value.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);
}

internal sealed class NullableUnixTimestampConverter : FlexibleNullableConverter<DateTimeOffset>
{
    protected override bool TryParse(ReadOnlySpan<char> text, out DateTimeOffset value) => EtherscanNumber.TryParseTimestamp(text, out value);

    protected override string Format(DateTimeOffset value) => value.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);
}

/// <summary>Reads <c>yyyy-MM-dd</c> dates (Etherscan's <c>UTCDate</c>) and tolerates empty strings.</summary>
internal sealed class FlexibleDateOnlyConverter : FlexibleConverter<DateOnly>
{
    protected override bool TryParse(ReadOnlySpan<char> text, out DateOnly value)
    {
        if (DateOnly.TryParseExact(text.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out value))
        {
            return true;
        }

        if (EtherscanNumber.TryParseTimestamp(text, out var ts))
        {
            value = DateOnly.FromDateTime(ts.UtcDateTime);
            return true;
        }

        return DateOnly.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out value);
    }

    protected override string Format(DateOnly value) => value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}

/// <summary>Reads a string field that Etherscan sometimes sends as a number.</summary>
internal sealed class FlexibleStringConverter : JsonConverter<string>
{
    public override bool HandleNull => false;

    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType is JsonTokenType.StartArray or JsonTokenType.StartObject
            ? throw new JsonException()
            : EtherscanNumber.ReadText(ref reader);

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        => writer.WriteStringValue(value);
}
