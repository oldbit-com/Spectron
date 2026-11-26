using System.Text;
using OldBit.Spectron.Emulation.Devices.Keyboard;
using OldBit.Spectron.Emulation.Rom;

namespace OldBit.Spectron.Mcp.Basic;

/// <summary>
/// How a 48K BASIC keyword is entered on the keyboard.
/// </summary>
internal enum KeywordEntry
{
    /// <summary>
    /// A letter key at the K cursor, e.g. P for PRINT. Only possible at the start of a statement.
    /// </summary>
    KMode,

    /// <summary>
    /// Symbol Shift and a key, e.g. SYM+F for TO. Works at the K and L cursor.
    /// </summary>
    SymbolShift,

    /// <summary>
    /// EXTEND, then a key, e.g. EXTEND, Q for SIN.
    /// </summary>
    Extended,

    /// <summary>
    /// EXTEND, then Symbol Shift and a key, e.g. EXTEND, SYM+H for CIRCLE.
    /// </summary>
    ExtendedSymbolShift,

    /// <summary>
    /// A 128 BASIC keyword (SPECTRUM, PLAY). It has no key and can only be spelled out in the 128 editor.
    /// </summary>
    Spelled,
}

/// <summary>
/// A 48K BASIC keyword: its token, name and the key that enters it.
/// </summary>
internal sealed record BasicKeyword(byte Token, string Name, KeywordEntry Entry, SpectrumKey Key)
{
    internal bool IsExtended => Entry is KeywordEntry.Extended or KeywordEntry.ExtendedSymbolShift;

    /// <summary>
    /// The key combination for the keyword. For extended keywords, it is pressed after EXTEND.
    /// </summary>
    internal SpectrumKey[] Keys => Entry is KeywordEntry.KMode or KeywordEntry.Extended
        ? [Key]
        : [SpectrumKey.SymbolShift, Key];

    public override string ToString() => Name;
}

/// <summary>
/// The 48K BASIC keywords, read from the 48K ROM: names from the token table, keys by inverting the
/// keyboard decoding tables used by K-DECODE. The same tables are used by the 128K (48 BASIC) and TC2048 ROMs.
/// </summary>
internal static class BasicKeywords
{
    internal const byte FirstToken = 0xA5;
    internal const byte RemToken = 0xEA;
    internal const byte Spectrum128Token = 0xA3;
    internal const byte Play128Token = 0xA4;

    internal const int Count = 0x100 - FirstToken;

    // ROM addresses of the tables, already offset so they can be indexed with the key character
    private const int TokenTable = 0x0095;
    private const int ExtendedUnshifted = 0x022C;   // E-UNSHIFT, A-Z
    private const int ExtendedShifted = 0x0246;     // EXT-SHIFT, A-Z
    private const int SymbolCodes = 0x026A;         // SYM-CODES, A-Z
    private const int ExtendedDigits = 0x0284;      // E-DIGITS, 0-9

    private static readonly Lazy<KeywordTables> Tables = new(() => Build(RomReader.ReadRom(RomType.Original48)));

    /// <summary>
    /// All 91 keywords, ordered by token.
    /// </summary>
    internal static IReadOnlyList<BasicKeyword> All => Tables.Value.Keywords;

    /// <summary>
    /// The keywords that only 128 BASIC knows, SPECTRUM and PLAY.
    /// </summary>
    internal static IReadOnlyList<BasicKeyword> Basic128 { get; } =
    [
        new(Spectrum128Token, "SPECTRUM", KeywordEntry.Spelled, default),
        new(Play128Token, "PLAY", KeywordEntry.Spelled, default),
    ];

    /// <summary>
    /// Characters that can only be typed in extended mode with Symbol Shift, e.g. '[' is EXTEND, SYM+Y.
    /// </summary>
    internal static IReadOnlyDictionary<char, SpectrumKey> ExtendedCharacters => Tables.Value.ExtendedCharacters;

    internal static BasicKeyword Get(byte token) => token >= FirstToken ? All[token - FirstToken] : Basic128[token - Spectrum128Token];

    /// <summary>
    /// Converts a Spectrum character code to a .NET character, for the codes that differ from ASCII.
    /// </summary>
    internal static char ToChar(byte code) => code switch
    {
        0x60 => '£',
        0x7F => '©',
        _ => (char)code
    };

    /// <summary>
    /// Converts a character to its Spectrum character code.
    /// </summary>
    internal static byte ToCode(char c) => c switch
    {
        '£' => 0x60,
        '©' => 0x7F,
        _ => (byte)c
    };

    /// <summary>
    /// Renders edit line or program bytes as text, keywords spelled out.
    /// </summary>
    internal static string Detokenize(IEnumerable<byte> bytes)
    {
        var result = new StringBuilder();

        foreach (var b in bytes)
        {
            if (b >= FirstToken)
            {
                result.Append(' ').Append(Get(b).Name).Append(' ');
            }
            else if (b is >= 0x20 and < 0x80)
            {
                result.Append(ToChar(b));
            }
            else
            {
                result.Append($"[{b:X2}]");
            }
        }

        return string.Join(' ', result.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    private static KeywordTables Build(byte[] rom)
    {
        var names = ReadTokenNames(rom);
        var entries = new Dictionary<byte, (KeywordEntry Entry, SpectrumKey Key)>();
        var extendedCharacters = new Dictionary<char, SpectrumKey>();

        for (var i = 0; i < 26; i++)
        {
            var letterKey = Enum.Parse<SpectrumKey>(((char)('A' + i)).ToString());

            // K-TOKENS: a letter at the K cursor gives token $A5 + letter index, PRINT for P and so on
            Add((byte)(0xE6 + i), KeywordEntry.KMode, letterKey);

            if (rom[SymbolCodes + i] >= FirstToken)
            {
                Add(rom[SymbolCodes + i], KeywordEntry.SymbolShift, letterKey);
            }

            if (rom[ExtendedUnshifted + i] >= FirstToken)
            {
                Add(rom[ExtendedUnshifted + i], KeywordEntry.Extended, letterKey);
            }

            var shifted = rom[ExtendedShifted + i];

            if (shifted >= FirstToken)
            {
                Add(shifted, KeywordEntry.ExtendedSymbolShift, letterKey);
            }
            else
            {
                extendedCharacters.Add(ToChar(shifted), letterKey);
            }
        }

        for (var i = 0; i < 10; i++)
        {
            Add(rom[ExtendedDigits + i], KeywordEntry.ExtendedSymbolShift, SpectrumKey.D0 + i);
        }

        var keywords = new List<BasicKeyword>(Count);

        for (int token = FirstToken; token <= 0xFF; token++)
        {
            if (!entries.TryGetValue((byte)token, out var entry))
            {
                throw new InvalidOperationException($"No key found for BASIC token ${token:X2}.");
            }

            keywords.Add(new BasicKeyword((byte)token, names[token - FirstToken], entry.Entry, entry.Key));
        }

        return new KeywordTables(keywords, extendedCharacters);

        void Add(byte token, KeywordEntry entry, SpectrumKey key)
        {
            if (!entries.TryAdd(token, (entry, key)))
            {
                throw new InvalidOperationException($"BASIC token ${token:X2} has more than one key.");
            }
        }
    }

    private static List<string> ReadTokenNames(byte[] rom)
    {
        // The table starts with '?' (entry 0), followed by RND ($A5) to COPY ($FF). Bit 7 marks the last character.
        var names = new List<string>(Count);
        var name = new StringBuilder();
        var address = TokenTable;
        var isFirst = true;

        while (names.Count < Count)
        {
            var b = rom[address++];
            name.Append((char)(b & 0x7F));

            if ((b & 0x80) == 0)
            {
                continue;
            }

            if (!isFirst)
            {
                names.Add(name.ToString());
            }

            isFirst = false;
            name.Clear();
        }

        return names;
    }

    private sealed record KeywordTables(
        IReadOnlyList<BasicKeyword> Keywords,
        IReadOnlyDictionary<char, SpectrumKey> ExtendedCharacters);
}
