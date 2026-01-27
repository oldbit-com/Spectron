using System.Globalization;
using System.Numerics;

namespace OldBit.Spectron.Debugger.Parser;

internal static class HexNumberParser
{
    internal static bool TryParse<T>(string input, out T value, bool allowDecimalNumber = false) where T : IBinaryInteger<T>
    {
        if (allowDecimalNumber && T.TryParse(input, NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite,
                CultureInfo.InvariantCulture, out var result))
        {
            value = result;
            return true;
        }

        var hex = RemoveHexPrefixes(input);

        if (T.TryParse(hex, NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowHexSpecifier,
                CultureInfo.InvariantCulture, out result))
        {
            value = result;
            return true;
        }

        value = T.Zero;
        return false;
    }

    private static string RemoveHexPrefixes(string input) => input
        .Replace("0x", string.Empty, StringComparison.OrdinalIgnoreCase)
        .Replace("h", string.Empty, StringComparison.OrdinalIgnoreCase)
        .Replace("$", string.Empty)
        .Replace("#", string.Empty);
}