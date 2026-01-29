using System.Globalization;
using System.Numerics;

namespace OldBit.Spectron.Debugger.Converters;

internal static class Hex
{
    internal static bool TryParse<T>(string input, out T value, bool preferDecimal = false) where T : IBinaryInteger<T>
    {
        input = input.Trim();

        if (preferDecimal && T.TryParse(input, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var result))
        {
            value = result;
            return true;
        }

        var hex = TrimHexIndicator(input);

        if (T.TryParse(hex, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out result))
        {
            value = result;
            return true;
        }

        value = T.Zero;
        return false;
    }

    private static string TrimHexIndicator(string input)
    {
        if (input.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            return input[2..];
        }

        if (input.StartsWith('$') || input.StartsWith('#'))
        {
            return input[1..];
        }

        if (input.EndsWith("h", StringComparison.OrdinalIgnoreCase))
        {
            return input[..^1];
        }

        return input;
    }
}