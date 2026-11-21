using System.Text.RegularExpressions;

namespace EtherscanApi.Internal;

internal static partial class Guard
{
    [GeneratedRegex("^0x[0-9a-fA-F]{40}$", RegexOptions.CultureInvariant)]
    private static partial Regex AddressRegex();

    [GeneratedRegex("^0x[0-9a-fA-F]{64}$", RegexOptions.CultureInvariant)]
    private static partial Regex HashRegex();

    [GeneratedRegex("^0x[0-9a-fA-F]*$", RegexOptions.CultureInvariant)]
    private static partial Regex HexRegex();

    public static string Address(string? address, string paramName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(address, paramName);
        address = address.Trim();
        if (!AddressRegex().IsMatch(address))
        {
            throw new ArgumentException($"'{address}' is not a valid 0x-prefixed 20-byte hex address.", paramName);
        }

        return address;
    }

    public static string? OptionalAddress(string? address, string paramName)
        => string.IsNullOrWhiteSpace(address) ? null : Address(address, paramName);

    public static string Addresses(IEnumerable<string> addresses, int max, string paramName)
    {
        ArgumentNullException.ThrowIfNull(addresses, paramName);
        var list = addresses.Select(a => Address(a, paramName)).ToList();
        if (list.Count == 0)
        {
            throw new ArgumentException("At least one address is required.", paramName);
        }

        if (list.Count > max)
        {
            throw new ArgumentException($"At most {max} addresses are allowed per call.", paramName);
        }

        return string.Join(',', list);
    }

    public static string Hash(string? hash, string paramName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hash, paramName);
        hash = hash.Trim();
        if (!HashRegex().IsMatch(hash))
        {
            throw new ArgumentException($"'{hash}' is not a valid 0x-prefixed 32-byte hex hash.", paramName);
        }

        return hash;
    }

    public static string? OptionalHash(string? hash, string paramName)
        => string.IsNullOrWhiteSpace(hash) ? null : Hash(hash, paramName);

    public static string HexData(string? data, string paramName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(data, paramName);
        data = data.Trim();
        if (!HexRegex().IsMatch(data))
        {
            throw new ArgumentException("Value must be 0x-prefixed hex data.", paramName);
        }

        return data;
    }

    public static string? OptionalHexData(string? data, string paramName)
        => string.IsNullOrWhiteSpace(data) ? null : HexData(data, paramName);

    public static void Page(int? page, int? offset)
    {
        if (page is < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(page), page, "Page numbers start at 1.");
        }

        if (offset is < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), offset, "Offset must be at least 1.");
        }
    }

    public static void DateRange(DateOnly start, DateOnly end)
    {
        if (end < start)
        {
            throw new ArgumentException("The end date must not be earlier than the start date.", nameof(end));
        }
    }
}
