using OldBit.Spectron.Emulation.Screen;

namespace OldBit.Spectron.Extensions;

public static class ColorExtensions
{
    public static bool TryParseHex(string? text, out Avalonia.Media.Color color)
    {
        color = default;

        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        text = text.Trim();

        if (text[0] != '#')
        {
            text = "#" + text;
        }

        if (!Avalonia.Media.Color.TryParse(text, out var parsed))
        {
            return false;
        }

        color = Avalonia.Media.Color.FromRgb(parsed.R, parsed.G, parsed.B);

        return true;
    }

    extension(Color color)
    {
        public string ToHex() => $"#{color.Red:X2}{color.Green:X2}{color.Blue:X2}";

        public static Color FromHex(string? hex) =>
            TryParseHex(hex, out var parsed) ? new Color(parsed.R, parsed.G, parsed.B) : new Color(0, 0, 0);
    }
}
