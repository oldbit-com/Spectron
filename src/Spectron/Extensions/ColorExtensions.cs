using System.Globalization;
using OldBit.Spectron.Emulation.Screen;

namespace OldBit.Spectron.Extensions;

public static class ColorExtensions
{
    extension(Color color)
    {
        public string ToHex() => $"#{color.Alpha:X2}{color.Red:X2}{color.Green:X2}{color.Blue:X2}";

        public static Color FromHex(string hex)
        {
            if (Avalonia.Media.Color.TryParse(hex, out var parsed))
            {
                return new Color(parsed.R, parsed.G, parsed.B);
            }

            hex = hex.TrimStart('#');

            return !uint.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var rgb) ?
                new Color() :
                new Color((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
        }
    }
}