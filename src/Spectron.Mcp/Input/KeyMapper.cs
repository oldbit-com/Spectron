using OldBit.Spectron.Emulation.Devices.Keyboard;
using static OldBit.Spectron.Emulation.Devices.Keyboard.SpectrumKey;

namespace OldBit.Spectron.Mcp.Input;

/// <summary>
/// Translates text and key names into Spectrum key combinations.
/// </summary>
internal static class KeyMapper
{
    private static readonly Dictionary<char, SpectrumKey[]> SymbolKeys = new()
    {
        ['!'] = [SymbolShift, D1], ['@'] = [SymbolShift, D2], ['#'] = [SymbolShift, D3],
        ['$'] = [SymbolShift, D4], ['%'] = [SymbolShift, D5], ['&'] = [SymbolShift, D6],
        ['\''] = [SymbolShift, D7], ['('] = [SymbolShift, D8], [')'] = [SymbolShift, D9],
        ['_'] = [SymbolShift, D0], ['<'] = [SymbolShift, R], ['>'] = [SymbolShift, T],
        [';'] = [SymbolShift, O], ['"'] = [SymbolShift, P], ['^'] = [SymbolShift, H],
        ['-'] = [SymbolShift, J], ['+'] = [SymbolShift, K], ['='] = [SymbolShift, L],
        [':'] = [SymbolShift, Z], ['£'] = [SymbolShift, X], ['?'] = [SymbolShift, C],
        ['/'] = [SymbolShift, V], ['*'] = [SymbolShift, B], [','] = [SymbolShift, N],
        ['.'] = [SymbolShift, M],
        [' '] = [Space], ['\n'] = [Enter], ['\b'] = [CapsShift, D0],
    };

    private static readonly Dictionary<string, SpectrumKey[]> NamedKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ENTER"] = [Enter],
        ["SPACE"] = [Space],
        ["CAPS"] = [CapsShift],
        ["CAPSSHIFT"] = [CapsShift],
        ["SHIFT"] = [CapsShift],
        ["SYM"] = [SymbolShift],
        ["SYMBOL"] = [SymbolShift],
        ["SYMBOLSHIFT"] = [SymbolShift],
        ["BREAK"] = [CapsShift, Space],
        ["DELETE"] = [CapsShift, D0],
        ["EDIT"] = [CapsShift, D1],
        ["CAPSLOCK"] = [CapsShift, D2],
        ["TRUEVIDEO"] = [CapsShift, D3],
        ["INVVIDEO"] = [CapsShift, D4],
        ["LEFT"] = [CapsShift, D5],
        ["DOWN"] = [CapsShift, D6],
        ["UP"] = [CapsShift, D7],
        ["RIGHT"] = [CapsShift, D8],
        ["GRAPHICS"] = [CapsShift, D9],
        ["EXTEND"] = [CapsShift, SymbolShift],
    };

    /// <summary>
    /// Converts text to a sequence of key combinations, one per character.
    /// Letters, digits, space, newline (ENTER), backspace (DELETE) and the symbols printed
    /// in red on the 48K keyboard are supported.
    /// </summary>
    internal static List<SpectrumKey[]> FromText(string text)
    {
        var result = new List<SpectrumKey[]>(text.Length);

        foreach (var c in text.Replace("\r\n", "\n"))
        {
            result.Add(FromChar(c) ?? throw new ArgumentException($"Character '{c}' cannot be typed on the Spectrum keyboard."));
        }

        return result;
    }

    /// <summary>
    /// Parses a key combination such as "ENTER", "A", "CAPS+5", "SYM+P" or "BREAK".
    /// </summary>
    internal static SpectrumKey[] ParseCombination(string combination)
    {
        var keys = new List<SpectrumKey>();

        foreach (var part in combination.Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            if (NamedKeys.TryGetValue(part, out var named))
            {
                keys.AddRange(named);
            }
            else if (part.Length == 1 && char.IsAsciiLetterOrDigit(part[0]))
            {
                keys.Add(FromLetterOrDigit(char.ToUpperInvariant(part[0])));
            }
            else
            {
                throw new ArgumentException($"Unknown key '{part}' in '{combination}'.");
            }
        }

        if (keys.Count == 0)
        {
            throw new ArgumentException("Key combination is empty.");
        }

        return keys.Distinct().ToArray();
    }

    private static SpectrumKey[]? FromChar(char c)
    {
        if (char.IsAsciiLetterLower(c) || char.IsAsciiDigit(c))
        {
            return [FromLetterOrDigit(char.ToUpperInvariant(c))];
        }

        if (char.IsAsciiLetterUpper(c))
        {
            return [CapsShift, FromLetterOrDigit(c)];
        }

        return SymbolKeys.GetValueOrDefault(c);
    }

    private static SpectrumKey FromLetterOrDigit(char c) => char.IsAsciiDigit(c)
        ? D0 + (c - '0')
        : Enum.Parse<SpectrumKey>(c.ToString());
}
