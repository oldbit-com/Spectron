using OldBit.Spectron.Emulation.Screen;

namespace OldBit.Spectron.Screen;

internal static class BorderSizes
{
    internal static readonly Border None = new(Top: 0, Left: 0, Right: 0, Bottom: 0);
    internal static readonly Border Small = new(Top: 15, Left: 15, Right: 15, Bottom: 15);
    internal static readonly Border Medium = new(Top: 25, Left: 25, Right: 25, Bottom: 25);
    internal static readonly Border Large = new(Top: 40, Left: 40, Right: 40, Bottom: 40);
    internal static readonly Border Full = new(ScreenSize.BorderTop - 1, ScreenSize.BorderLeft, ScreenSize.BorderRight, ScreenSize.BorderBottom - 1);
    internal static readonly Border Max = new(ScreenSize.BorderTop, ScreenSize.BorderLeft, ScreenSize.BorderRight, ScreenSize.BorderBottom);

    internal static Border GetBorder(BorderSize borderSize) => borderSize switch
    {
        BorderSize.None => None,
        BorderSize.Small => Small,
        BorderSize.Medium => Medium,
        BorderSize.Large => Large,
        _ => Full,
    };
}