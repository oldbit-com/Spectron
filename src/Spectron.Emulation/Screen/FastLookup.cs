namespace OldBit.Spectron.Emulation.Screen;

internal record AttributeColor(Color Paper, Color Ink, bool IsFlashOn);

internal record ScreenRenderEvent(Word BitmapAddress, Word AttributeAddress, int Ticks, int FrameBufferIndex);

internal static class FastLookup
{
    private static readonly Dictionary<ComputerType, ScreenRenderEvent[]> ScreenRenderEvents = new();

    /// <summary>
    /// Bit masks for each bit in a byte.
    /// </summary>
    internal static readonly byte[] BitMasks = [0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01];

    /// <summary>
    /// A lookup table that provides all precomputed mappings of screen attribute value
    /// to their corresponding paper, ink colors, and flash state.
    /// </summary>
    internal static readonly AttributeColor[] AttributeData = BuildAttributeColorLookupTable();

    /// <summary>
    /// A lookup table that maps attribute memory addresses to the corresponding
    /// starting screen memory address of their associated 8-pixel-high line.
    /// This table is used to efficiently determine the area of the screen affected
    /// by attribute changes.
    /// </summary>
    internal static readonly Word[] LineAddressForAttrAddress = BuildScreenAddressLookupTable();

    /// <summary>
    /// List of events used to render the screen. Each event occurs at 8 ticks intervals and updates 2
    /// bytes of the screen.
    /// </summary>
    /// <param name="hardware">The hardware settings.</param>
    /// <returns>A list of <see cref="ScreenRenderEvent"/> events.</returns>
    internal static ScreenRenderEvent[] GetScreenRenderEvents(HardwareSettings hardware)
    {
        if (ScreenRenderEvents.TryGetValue(hardware.ComputerType, out var events))
        {
            return events;
        }

        ScreenRenderEvents[hardware.ComputerType] = BuildScreenEventsTable(hardware);

        return ScreenRenderEvents[hardware.ComputerType];
    }

    private static AttributeColor[] BuildAttributeColorLookupTable()
    {
        var colors = new List<AttributeColor>();

        for (var i = 0; i <= byte.MaxValue; i++)
        {
            var isFlashOn = (i & 0x80) != 0;

            var ink = SpectrumPalette.GetInkColor(i);
            var paper = SpectrumPalette.GetPaperColor(i);

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

    private static ScreenRenderEvent[] BuildScreenEventsTable(HardwareSettings hardware)
    {
        var screenEvents = new List<ScreenRenderEvent>();

        for (var y = 0; y < ScreenSize.ContentHeight; y++)
        {
            var rowTime = hardware.FirstPixelTicks + hardware.TicksPerLine * y;
            var bufferLineIndex = FrameBuffer.GetLineIndex(y, hardware.BorderTop);

            for (var x = 0; x < 16; x++)
            {
                var bitmapAddress = ScreenAddress.Calculate(x * 2, y) - 0x4000;
                var attributeAddress = 0x1800 + 32 * (y / 8) + x * 2;

                screenEvents.Add(new ScreenRenderEvent(
                    BitmapAddress: (Word)bitmapAddress,
                    AttributeAddress: (Word)attributeAddress,
                    Ticks: rowTime + 8 * x,
                    FrameBufferIndex: bufferLineIndex + 8 * x * 2));
            }
        }

        return screenEvents.ToArray();
    }
}