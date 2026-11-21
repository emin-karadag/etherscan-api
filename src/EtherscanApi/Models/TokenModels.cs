using System.Globalization;
using System.Numerics;
using System.Text.Json.Serialization;

namespace EtherscanApi.Models;

/// <summary>A holder in the top-holders ranking (<c>topholders</c>).</summary>
public sealed record TopTokenHolder
{
    /// <summary>Holder address.</summary>
    [JsonPropertyName("TokenHolderAddress")]
    public string Address { get; init; } = "";

    /// <summary>
    /// Quantity held, as the decimal string Etherscan returns (already divided by the token decimals).
    /// Kept as text to preserve full precision; see <see cref="TryGetQuantity"/>.
    /// </summary>
    [JsonPropertyName("TokenHolderQuantity")]
    public string Quantity { get; init; } = "0";

    /// <summary><c>C</c> for a contract, <c>A</c> for an externally owned account.</summary>
    [JsonPropertyName("TokenHolderAddressType")]
    public string? AddressType { get; init; }

    /// <summary>Whether the holder is a contract.</summary>
    [JsonIgnore]
    public bool IsContract => string.Equals(AddressType, "C", StringComparison.OrdinalIgnoreCase);

    /// <summary>Parses <see cref="Quantity"/> as a <see cref="decimal"/>; fails if it exceeds decimal range.</summary>
    public bool TryGetQuantity(out decimal quantity)
        => decimal.TryParse(Quantity, NumberStyles.Float, CultureInfo.InvariantCulture, out quantity);
}

/// <summary>A token holder and raw balance (<c>tokenholderlist</c>).</summary>
public sealed record TokenHolder
{
    /// <summary>Holder address.</summary>
    [JsonPropertyName("TokenHolderAddress")]
    public string Address { get; init; } = "";

    /// <summary>Balance in the token's smallest unit.</summary>
    [JsonPropertyName("TokenHolderQuantity")]
    public BigInteger Quantity { get; init; }
}

/// <summary>Project details and links for a token (<c>tokeninfo</c>).</summary>
public sealed record TokenInfo
{
    /// <summary>Token contract address.</summary>
    [JsonPropertyName("contractAddress")]
    public string ContractAddress { get; init; } = "";

    /// <summary>Token name.</summary>
    [JsonPropertyName("tokenName")]
    public string? TokenName { get; init; }

    /// <summary>Token symbol.</summary>
    [JsonPropertyName("symbol")]
    public string? Symbol { get; init; }

    /// <summary>Token decimals.</summary>
    [JsonPropertyName("divisor")]
    public int? Divisor { get; init; }

    /// <summary>Token standard, such as <c>ERC20</c> or <c>ERC721</c>.</summary>
    [JsonPropertyName("tokenType")]
    public string? TokenType { get; init; }

    /// <summary>Total supply in the token's smallest unit.</summary>
    [JsonPropertyName("totalSupply")]
    public BigInteger TotalSupply { get; init; }

    /// <summary>Whether the token has Etherscan's blue checkmark.</summary>
    [JsonPropertyName("blueCheckmark")]
    public bool BlueCheckmark { get; init; }

    /// <summary>Project description.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>Website URL.</summary>
    [JsonPropertyName("website")]
    public string? Website { get; init; }

    /// <summary>Contact email.</summary>
    [JsonPropertyName("email")]
    public string? Email { get; init; }

    /// <summary>Blog URL.</summary>
    [JsonPropertyName("blog")]
    public string? Blog { get; init; }

    /// <summary>Reddit URL.</summary>
    [JsonPropertyName("reddit")]
    public string? Reddit { get; init; }

    /// <summary>Slack URL.</summary>
    [JsonPropertyName("slack")]
    public string? Slack { get; init; }

    /// <summary>Facebook URL.</summary>
    [JsonPropertyName("facebook")]
    public string? Facebook { get; init; }

    /// <summary>Twitter/X URL.</summary>
    [JsonPropertyName("twitter")]
    public string? Twitter { get; init; }

    /// <summary>Bitcointalk URL.</summary>
    [JsonPropertyName("bitcointalk")]
    public string? Bitcointalk { get; init; }

    /// <summary>GitHub URL.</summary>
    [JsonPropertyName("github")]
    public string? GitHub { get; init; }

    /// <summary>Telegram URL.</summary>
    [JsonPropertyName("telegram")]
    public string? Telegram { get; init; }

    /// <summary>WeChat URL.</summary>
    [JsonPropertyName("wechat")]
    public string? WeChat { get; init; }

    /// <summary>LinkedIn URL.</summary>
    [JsonPropertyName("linkedin")]
    public string? LinkedIn { get; init; }

    /// <summary>Discord URL.</summary>
    [JsonPropertyName("discord")]
    public string? Discord { get; init; }

    /// <summary>Whitepaper URL.</summary>
    [JsonPropertyName("whitepaper")]
    public string? Whitepaper { get; init; }

    /// <summary>Price in USD, when known.</summary>
    [JsonPropertyName("tokenPriceUSD")]
    public decimal? TokenPriceUsd { get; init; }

    /// <summary>Token logo URL.</summary>
    [JsonPropertyName("image")]
    public string? Image { get; init; }
}

/// <summary>An ERC-20 token held by an address (<c>addresstokenbalance</c>).</summary>
public sealed record AddressTokenBalance
{
    /// <summary>Token contract address.</summary>
    [JsonPropertyName("TokenAddress")]
    public string TokenAddress { get; init; } = "";

    /// <summary>Token name.</summary>
    [JsonPropertyName("TokenName")]
    public string? TokenName { get; init; }

    /// <summary>Token symbol.</summary>
    [JsonPropertyName("TokenSymbol")]
    public string? TokenSymbol { get; init; }

    /// <summary>Balance in the token's smallest unit.</summary>
    [JsonPropertyName("TokenQuantity")]
    public BigInteger TokenQuantity { get; init; }

    /// <summary>Token decimals.</summary>
    [JsonPropertyName("TokenDivisor")]
    public int? TokenDivisor { get; init; }

    /// <summary>Price in USD, when known.</summary>
    [JsonPropertyName("TokenPriceUSD")]
    public decimal? TokenPriceUsd { get; init; }
}

/// <summary>An ERC-721 collection held by an address (<c>addresstokennftbalance</c>).</summary>
public sealed record AddressNftBalance
{
    /// <summary>NFT contract address.</summary>
    [JsonPropertyName("TokenAddress")]
    public string TokenAddress { get; init; } = "";

    /// <summary>Collection name.</summary>
    [JsonPropertyName("TokenName")]
    public string? TokenName { get; init; }

    /// <summary>Collection symbol.</summary>
    [JsonPropertyName("TokenSymbol")]
    public string? TokenSymbol { get; init; }

    /// <summary>Number of tokens held.</summary>
    [JsonPropertyName("TokenQuantity")]
    public BigInteger TokenQuantity { get; init; }
}

/// <summary>An ERC-721 token id held by an address (<c>addresstokennftinventory</c>).</summary>
public sealed record NftInventoryItem
{
    /// <summary>NFT contract address.</summary>
    [JsonPropertyName("TokenAddress")]
    public string TokenAddress { get; init; } = "";

    /// <summary>Token id.</summary>
    [JsonPropertyName("TokenId")]
    public BigInteger TokenId { get; init; }
}
