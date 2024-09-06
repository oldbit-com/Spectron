using OldBit.Spectron.Emulation.Screen;
using OldBit.ZXTape.Szx;
using OldBit.ZXTape.Szx.Blocks;

namespace OldBit.Spectron.Emulation.Snapshot;

public static class SzxFileExtensions
{
    public static int[] GetScreenshot(this SzxFile szxFile, bool isFlashOnFrame = false)
    {
        var screenMemory = szxFile.RamPages.First(page => page.PageNumber == 5).Data;
        var palette = szxFile.Palette;

        var buffer = new int[ScreenSize.ContentHeight * ScreenSize.ContentWidth];

        for (var line = 0; line < ScreenSize.ContentHeight; line++)
        {
            for (var column = 0; column < 32; column++)
            {
                var bitmapAddress = ScreenAddress.Calculate(column, line) - 0x4000;
                var attributeAddress = ScreenAddress.CalculateAttribute(column, line) - 0x4000;

                var bitmap = screenMemory[bitmapAddress];
                var attribute = screenMemory[attributeAddress];

                var attributeData = FastLookup.AttributeData[attribute];
                var isFlashOn = attributeData.IsFlashOn && isFlashOnFrame;

                var bufferIndex = 256 * line + 8 * column;

                for (var bit = 0; bit < FastLookup.BitMasks.Length; bit++)
                {
                    Color color;

                    if (palette?.Flags == PaletteBlock.FlagsPaletteEnabled)
                    {
                        color = (bitmap & FastLookup.BitMasks[bit]) != 0 ?
                            palette.GetInkColor(attribute) :
                            palette.GetPaperColor(attribute);
                    }
                    else
                    {
                        color = (bitmap & FastLookup.BitMasks[bit]) != 0 ^ isFlashOn ?
                            attributeData.Ink :
                            attributeData.Paper;
                    }

                    buffer[bufferIndex + bit] = color.Abgr;
                }
            }
        }

        return buffer;
    }

    private static Color GetInkColor(this PaletteBlock palette, byte attribute)
    {
        // TODO: Implement
        return SpectrumPalette.White;
    }

    private static Color GetPaperColor(this PaletteBlock palette, byte attribute)
    {
        // TODO: Implement
        return SpectrumPalette.White;
    }
}