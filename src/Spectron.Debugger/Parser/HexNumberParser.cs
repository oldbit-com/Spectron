using System.Globalization;

namespace OldBit.Spectron.Debugger.Parser;

internal static class HexNumberParser
{
    internal static bool TryParse(string input, out int value)
    {
        if (int.TryParse(input, out value))
        {
            return true;
        }

        var hex = input
            .Replace("0x", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("h", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("$", string.Empty)
            .Replace("#", string.Empty);

        return int.TryParse(hex, NumberStyles.AllowHexSpecifier,
            CultureInfo.InvariantCulture, out value);
    }
}