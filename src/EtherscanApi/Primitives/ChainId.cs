using System.Globalization;

namespace EtherscanApi;

/// <summary>
/// Identifies an EVM chain supported by the Etherscan API V2 (the <c>chainid</c> query parameter).
/// </summary>
/// <remarks>
/// <para>
/// This is an open value type rather than a closed enum: Etherscan adds and deprecates chains regularly
/// (see the <see href="https://docs.etherscan.io/changelog">changelog</see>). Use the well-known constants
/// for convenience, construct any other id with <see cref="ChainId(ulong)"/>, and call
/// <c>client.Chains.GetChainListAsync()</c> for the live list.
/// </para>
/// <para>
/// Free API keys only cover a subset of chains. See
/// <see href="https://docs.etherscan.io/supported-chains">Supported Chains</see>.
/// </para>
/// </remarks>
/// <param name="Value">The numeric chain id, e.g. <c>1</c> for Ethereum Mainnet.</param>
public readonly record struct ChainId(ulong Value) : ISpanFormattable, IParsable<ChainId>
{
    // Well-known chains from the Etherscan V2 chain list. Plan availability changes over time; see the supported-chains page.

    /// <summary>Ethereum Mainnet (1).</summary>
    public static ChainId EthereumMainnet { get; } = new(1);
    /// <summary>Sepolia Testnet (11155111).</summary>
    public static ChainId Sepolia { get; } = new(11155111);
    /// <summary>Hoodi Testnet (560048).</summary>
    public static ChainId Hoodi { get; } = new(560048);
    /// <summary>BNB Smart Chain Mainnet (56).</summary>
    public static ChainId BnbSmartChain { get; } = new(56);
    /// <summary>BNB Smart Chain Testnet (97).</summary>
    public static ChainId BnbSmartChainTestnet { get; } = new(97);
    /// <summary>Polygon Mainnet (137).</summary>
    public static ChainId Polygon { get; } = new(137);
    /// <summary>Polygon Amoy Testnet (80002).</summary>
    public static ChainId PolygonAmoy { get; } = new(80002);
    /// <summary>Base Mainnet (8453).</summary>
    public static ChainId Base { get; } = new(8453);
    /// <summary>Base Sepolia Testnet (84532).</summary>
    public static ChainId BaseSepolia { get; } = new(84532);
    /// <summary>Arbitrum One Mainnet (42161).</summary>
    public static ChainId ArbitrumOne { get; } = new(42161);
    /// <summary>Arbitrum Sepolia Testnet (421614).</summary>
    public static ChainId ArbitrumSepolia { get; } = new(421614);
    /// <summary>Linea Mainnet (59144).</summary>
    public static ChainId Linea { get; } = new(59144);
    /// <summary>Linea Sepolia Testnet (59141).</summary>
    public static ChainId LineaSepolia { get; } = new(59141);
    /// <summary>OP Mainnet (10).</summary>
    public static ChainId Optimism { get; } = new(10);
    /// <summary>OP Sepolia Testnet (11155420).</summary>
    public static ChainId OptimismSepolia { get; } = new(11155420);
    /// <summary>Avalanche C-Chain (43114).</summary>
    public static ChainId AvalancheCChain { get; } = new(43114);
    /// <summary>BitTorrent Chain Mainnet (199).</summary>
    public static ChainId BitTorrentChain { get; } = new(199);
    /// <summary>BitTorrent Chain Testnet (1029).</summary>
    public static ChainId BitTorrentChainTestnet { get; } = new(1029);
    /// <summary>Celo Mainnet (42220).</summary>
    public static ChainId Celo { get; } = new(42220);
    /// <summary>Celo Sepolia Testnet (11142220).</summary>
    public static ChainId CeloSepolia { get; } = new(11142220);
    /// <summary>Fraxtal Mainnet (252).</summary>
    public static ChainId Fraxtal { get; } = new(252);
    /// <summary>Fraxtal Hoodi Testnet (2523).</summary>
    public static ChainId FraxtalHoodi { get; } = new(2523);
    /// <summary>Gnosis (100).</summary>
    public static ChainId Gnosis { get; } = new(100);
    /// <summary>Mantle Mainnet (5000).</summary>
    public static ChainId Mantle { get; } = new(5000);
    /// <summary>Mantle Sepolia Testnet (5003).</summary>
    public static ChainId MantleSepolia { get; } = new(5003);
    /// <summary>Memecore Mainnet (4352).</summary>
    public static ChainId Memecore { get; } = new(4352);
    /// <summary>Memecore Insectarium Testnet (43522).</summary>
    public static ChainId MemecoreInsectarium { get; } = new(43522);
    /// <summary>Taiko Mainnet (167000).</summary>
    public static ChainId Taiko { get; } = new(167000);
    /// <summary>Taiko Hoodi (167013).</summary>
    public static ChainId TaikoHoodi { get; } = new(167013);
    /// <summary>XDC Mainnet (50).</summary>
    public static ChainId Xdc { get; } = new(50);
    /// <summary>XDC Apothem Testnet (51).</summary>
    public static ChainId XdcApothem { get; } = new(51);
    /// <summary>World Mainnet (480).</summary>
    public static ChainId World { get; } = new(480);
    /// <summary>World Sepolia Testnet (4801).</summary>
    public static ChainId WorldSepolia { get; } = new(4801);
    /// <summary>Sonic Mainnet (146).</summary>
    public static ChainId Sonic { get; } = new(146);
    /// <summary>Sonic Testnet (14601).</summary>
    public static ChainId SonicTestnet { get; } = new(14601);
    /// <summary>Unichain Mainnet (130).</summary>
    public static ChainId Unichain { get; } = new(130);
    /// <summary>Unichain Sepolia Testnet (1301).</summary>
    public static ChainId UnichainSepolia { get; } = new(1301);
    /// <summary>Berachain Mainnet (80094).</summary>
    public static ChainId Berachain { get; } = new(80094);
    /// <summary>Berachain Bepolia Testnet (80069).</summary>
    public static ChainId BerachainBepolia { get; } = new(80069);
    /// <summary>Monad Mainnet (143).</summary>
    public static ChainId Monad { get; } = new(143);
    /// <summary>Monad Testnet (10143).</summary>
    public static ChainId MonadTestnet { get; } = new(10143);
    /// <summary>HyperEVM Mainnet (999).</summary>
    public static ChainId HyperEvm { get; } = new(999);
    /// <summary>Katana Mainnet (747474).</summary>
    public static ChainId Katana { get; } = new(747474);
    /// <summary>Katana Bokuto (737373).</summary>
    public static ChainId KatanaBokuto { get; } = new(737373);
    /// <summary>Sei Mainnet (1329).</summary>
    public static ChainId Sei { get; } = new(1329);
    /// <summary>Sei Testnet (1328).</summary>
    public static ChainId SeiTestnet { get; } = new(1328);
    /// <summary>Stable Mainnet (988).</summary>
    public static ChainId Stable { get; } = new(988);
    /// <summary>Stable Testnet (2201).</summary>
    public static ChainId StableTestnet { get; } = new(2201);
    /// <summary>Plasma Mainnet (9745).</summary>
    public static ChainId Plasma { get; } = new(9745);
    /// <summary>Plasma Testnet (9746).</summary>
    public static ChainId PlasmaTestnet { get; } = new(9746);
    /// <summary>MegaETH Mainnet (4326).</summary>
    public static ChainId MegaEth { get; } = new(4326);
    /// <summary>MegaETH Testnet (6343).</summary>
    public static ChainId MegaEthTestnet { get; } = new(6343);
    /// <summary>Robinhood Chain (4663).</summary>
    public static ChainId RobinhoodChain { get; } = new(4663);
    /// <summary>Arc Mainnet (5042).</summary>
    public static ChainId Arc { get; } = new(5042);

    /// <summary>Converts a numeric chain id.</summary>
    public static implicit operator ChainId(ulong value) => new(value);

    /// <summary>Returns the numeric chain id. Explicit so a chain id cannot silently flow into any <see cref="ulong"/>.</summary>
    public static explicit operator ulong(ChainId chainId) => chainId.Value;

    /// <summary>Creates a <see cref="ChainId"/> from a numeric value.</summary>
    public static ChainId FromUInt64(ulong value) => new(value);

    /// <summary>Returns the numeric chain id.</summary>
    public ulong ToUInt64() => Value;

    /// <inheritdoc />
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);

    /// <inheritdoc />
    public string ToString(string? format, IFormatProvider? formatProvider) => Value.ToString(format, formatProvider);

    /// <inheritdoc />
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        => Value.TryFormat(destination, out charsWritten, format, provider);

    /// <inheritdoc />
    public static ChainId Parse(string s, IFormatProvider? provider)
        => new(ulong.Parse(s, NumberStyles.None, provider ?? CultureInfo.InvariantCulture));

    /// <inheritdoc />
    public static bool TryParse(string? s, IFormatProvider? provider, out ChainId result)
    {
        if (ulong.TryParse(s, NumberStyles.None, provider ?? CultureInfo.InvariantCulture, out var value))
        {
            result = new ChainId(value);
            return true;
        }

        result = default;
        return false;
    }
}
