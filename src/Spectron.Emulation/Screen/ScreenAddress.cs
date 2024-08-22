namespace OldBit.Spectron.Emulation.Screen;

internal static class ScreenAddress
{
    /// <summary>
    /// Calculate the screen address for the specified column and row.
    /// </summary>
    /// <remarks>
    /// The format of the screen address is as follows:
    /// 0 1 0 y7 y6 y2 y1 y0 y5 y4 y3 x4 x3 x2 x1 x0
    /// </remarks>
    /// <param name="x">The column number (0-31).</param>
    /// <param name="y">The row number (0-191).</param>
    /// <returns>The screen address for the specified column and row</returns>
    internal static Word Calculate(int x, int y)
    {
        var address =
            0x4000 |
            (x & 0x1F) |
            (y & 0xC0 | (y & 0x07) << 3 | (y & 0x38) >> 3) << 5;

        return (Word)address;
    }
}