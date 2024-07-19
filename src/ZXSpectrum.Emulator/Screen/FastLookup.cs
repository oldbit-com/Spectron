namespace OldBit.ZXSpectrum.Emulator.Screen;

internal record AttributeColor(Color Paper, Color Ink, bool IsFlashOn);

internal record ScreenRenderEvent(Word BitmapAddress, Word AttributeAddress, int Ticks, int FrameBufferIndex);

internal static class FastLookup
{
    /// <summary>
    /// Bit masks for each bit in a byte.
    /// </summary>
    internal static readonly byte[] BitMasks = [0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01];

    /// <summary>
    /// Attribute colors indexed by attribute value (0-255).
    /// </summary>
    internal static readonly AttributeColor[] AttributeData = BuildAttributeColorLookupTable();

    /// <summary>
    /// Address of the first screen byte for each attribute address.
    /// </summary>
    internal static readonly Word[] LineAddressForAttrAddress = BuildScreenAddressLookupTable();

    /// <summary>
    /// List of events used to render the screen. Each event occurs at 8 ticks intervals and updates 2 bytes of the screen.
    /// </summary>
    internal static ScreenRenderEvent[] ScreenRenderEvents { get; } = BuildScreenEventsTable();

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

    public static ScreenRenderEvent[] BuildScreenEventsTable()
    {
        var screenEvents = new List<ScreenRenderEvent>();

        for (var y = 0; y < DefaultSizes.ContentHeight; y++)
        {
            var rowTime = DefaultTimings.FirstPixelTick + DefaultTimings.LineTicks * y;
            var bufferLineIndex = FrameBuffer.GetLineIndex(y);

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