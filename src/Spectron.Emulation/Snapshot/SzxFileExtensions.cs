using OldBit.Spectron.Emulation.Devices;
using OldBit.Spectron.Emulation.Screen;
using OldBit.Spectron.Files.Szx;
using OldBit.Spectron.Files.Szx.Blocks;

namespace OldBit.Spectron.Emulation.Snapshot;

public static class SzxFileExtensions
{
    private static readonly int[] Buffer = new int[ScreenSize.ContentHeight * ScreenSize.ContentWidth];

    public static int[] GetScreenshot(this SzxFile szx, bool isFlashOnFrame = false)
    {
        const int screenBank = 5;

        var screenMemory = szx.RamPages.First(page => page.PageNumber == screenBank).Data;
        var palette = szx.Palette;

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

                    Buffer[bufferIndex + bit] = color.Abgr;
                }
            }
        }

        return Buffer;
    }

    private static Color GetInkColor(this PaletteBlock palette, byte attribute)
    {
        var paletteIndex = attribute >> 6;
        var colorIndex = attribute & 0x07;
        var colorValue = palette.Registers[paletteIndex * 16 + colorIndex];

        return UlaPlus.ColorFromValue(colorValue);
    }

    private static Color GetPaperColor(this PaletteBlock palette, byte attribute)
    {
        var paletteIndex = attribute >> 6;
        var colorIndex = ((attribute >> 3) & 0x07) | 8;
        var colorValue = palette.Registers[paletteIndex * 16 + colorIndex];

        return UlaPlus.ColorFromValue(colorValue);
    }
}