using OldBit.ZXSpectrum.Models;

namespace OldBit.ZXSpectrum.Extensions;

public record struct Border(int Top, int Left, int Right, int Bottom)
{
    public Border(): this(0, 0, 0, 0)
    {
    }
}

public static class BorderSizeExtensions
{
    private static readonly Border BorderNone = new(0, 0, 0, 0);
    private static readonly Border BorderSmall = new(15, 15, 15, 15);
    private static readonly Border BorderMedium = new(25, 25, 25, 25);
    private static readonly Border BorderLarge = new(40, 40, 40, 40);
    private static readonly Border BorderFull = new(64, 48, 48, 56);

    public static Border ToBorder(this BorderSize borderSize)
    {
        return borderSize switch
        {
            BorderSize.None => BorderNone,
            BorderSize.Small => BorderSmall,
            BorderSize.Medium => BorderMedium,
            BorderSize.Large => BorderLarge,
            _ => BorderFull,
        };
    }
}