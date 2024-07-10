using System.Collections.ObjectModel;

namespace OldBit.ZXSpectrum.Emulator.Screen;

internal record PixelTickData(int Tick, Word Address, Word AttrAddress, int BufferAddress);

internal  record AttributeColor(Color Paper, Color Ink, bool IsFlashOn);

internal static class FastLookup
{
    /// <summary>
    /// Pixel address data indexed by t-state.
    /// </summary>
    internal static readonly PixelTickData?[] PixelTicks = BuildPixelLookupTable();

    /// <summary>
    /// Bit masks for each bit in a byte.
    /// </summary>
    internal static readonly byte[] BitMasks = [0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01];

    /// <summary>
    /// Attribute colors indexed by attribute value (0-255).
    /// </summary>
    internal static readonly AttributeColor[] AttributeColors = BuildAttributeColorLookupTable();

    /// <summary>
    /// Address of the first screen byte for each attribute address.
    /// </summary>
    internal static readonly Word[] LineAddressForAttrAddress = BuildScreenAddressLookupTable();

    /// <summary>
    /// Incremental tick for rendering each screen byte.
    /// </summary>
    internal static int[] ContentTicks { get; } = BuildContentTicksTable();

    private static PixelTickData?[] BuildPixelLookupTable()
    {
        var pixels = new List<PixelTickData>();

        for (var line = 0; line < Constants.ContentHeight; line++)
        {
            for (var tick = 0; tick < Constants.ContentTicks; tick++)
            {
                var column = tick / 4;
                var pixelTick = Constants.FirstDataPixelTick + line * Constants.LineTicks + tick;

                var tickData = new PixelTickData(
                    Tick: pixelTick,
                    Address: (Word)(ScreenAddress.Calculate(column, line) - 0x4000), // addresses are 0-based
                    AttrAddress: (Word)(0x1800 + 32 * (line / 8) + column),
                    BufferAddress: ScreenBuffer.GetContentAddress(line) + column * 8
                );

                pixels.Add(tickData);
            }
        }

        var pixelTickLookup = new PixelTickData?[pixels.Last().Tick + 1];
        foreach (var pixelTick in pixels)
        {
            pixelTickLookup[pixelTick.Tick] = pixelTick;
        }

        return pixelTickLookup;
    }

    private static AttributeColor[] BuildAttributeColorLookupTable()
    {
        var colors = new List<AttributeColor>();

        for (var i = 0; i <= byte.MaxValue; i++)
        {
            var isFlashOn = (i & 0x80) != 0;

            var ink = Colors.InkColors[i & 0x47];
            var paper = Colors.PaperColors[i & 0x78];

            colors.Add(new AttributeColor(paper, ink, isFlashOn));
        }

        return colors.ToArray();
    }

    private static Word[] BuildScreenAddressLookupTable()
    {
        var screenAddressLookup = new Word[24 * 32];

        for (var column = 0; column < 32; column++)
        {
            for (var row = 0; row < 24; row++)
            {
                var attributeAddress = row * 32 + column;
                var screenAddress = ScreenAddress.Calculate(column, row * 8);

                screenAddressLookup[attributeAddress] = (Word)(screenAddress - 0x4000);
            }
        }

        return screenAddressLookup;
    }

    private static int[] BuildContentTicksTable()
    {
        var ticks = new List<int>();

        for (var line = 0; line < Constants.ContentHeight; line++)
        {
            for (var tick = 0; tick < Constants.ContentTicks / 4; tick++)
            {
                ticks.Add(2 + Constants.FirstDataPixelTick + line * Constants.LineTicks + tick * 4);
            }
        }

        return ticks.ToArray();
    }
}