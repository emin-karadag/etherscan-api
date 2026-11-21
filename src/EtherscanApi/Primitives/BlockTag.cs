using System.Globalization;

namespace EtherscanApi;

/// <summary>
/// A block reference used by balance and Geth/Parity proxy endpoints: a block number,
/// or one of the <c>latest</c>, <c>earliest</c>, <c>pending</c> keywords.
/// </summary>
/// <remarks>
/// <para>
/// <c>default(BlockTag)</c> is <see cref="Latest"/>. Block numbers are sent in hexadecimal (<c>0x10d4f</c>),
/// as the proxy endpoints require.
/// </para>
/// <para>
/// An implicit conversion from <see cref="long"/> (and therefore <see cref="int"/>) lets you pass block numbers
/// directly, for example <c>GetBlockByNumberAsync(tx.BlockNumber)</c>. The conversion never throws; a negative
/// number is rejected with <see cref="ArgumentOutOfRangeException"/> when the request is built.
/// </para>
/// </remarks>
public readonly record struct BlockTag
{
    private const string LatestKeyword = "latest";

    // null keyword and null number means "latest", so default(BlockTag) == BlockTag.Latest.
    private readonly string? _keyword;

    private BlockTag(string? keyword, long? number)
    {
        _keyword = keyword;
        Number = number;
    }

    /// <summary>The most recent block. Same as <c>default(BlockTag)</c>.</summary>
    public static BlockTag Latest => default;

    /// <summary>The genesis block.</summary>
    public static BlockTag Earliest { get; } = new("earliest", null);

    /// <summary>The pending state.</summary>
    public static BlockTag Pending { get; } = new("pending", null);

    /// <summary>The block number when this tag refers to a specific block; otherwise <see langword="null"/>.</summary>
    public long? Number { get; }

    /// <summary>Whether this tag refers to a specific block number.</summary>
    public bool IsNumber => Number.HasValue;

    /// <summary>Creates a tag for a specific block number.</summary>
    /// <param name="number">The block number. Must not be negative when the request is sent.</param>
    public static BlockTag FromNumber(long number) => new(null, number);

    /// <summary>Creates a tag for a specific block number. Also accepts <see cref="int"/> values.</summary>
    public static implicit operator BlockTag(long number) => FromNumber(number);

    /// <summary>Creates a tag for a specific block number.</summary>
    public static BlockTag FromInt64(long number) => FromNumber(number);

    /// <summary>Returns the wire format: a keyword or a <c>0x</c>-prefixed hex number.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The block number is negative.</exception>
    public string ToHexString()
    {
        if (Number is not { } number)
        {
            return _keyword ?? LatestKeyword;
        }

        ArgumentOutOfRangeException.ThrowIfNegative(number, nameof(Number));
        return "0x" + number.ToString("x", CultureInfo.InvariantCulture);
    }

    /// <inheritdoc />
    public override string ToString() => Number is { } n ? n.ToString(CultureInfo.InvariantCulture) : _keyword ?? LatestKeyword;
}
