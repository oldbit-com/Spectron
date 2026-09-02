using OldBit.Spectron.Emulation.Screen;
using OldBit.Spectron.Emulation.State;
using OldBit.Spectron.Emulation.State.Components;

namespace OldBit.Spectron.Emulation.Extensions;

public static class EmulatorStateExtensions
{
    private static readonly int[] Buffer = new int[ScreenSize.ContentHeight * ScreenSize.ContentWidth];

    extension(StateSnapshot snapshot)
    {
        public int[] GetScreenshot(bool isFlashOnFrame = false)
        {
            var screenMemory = snapshot.ComputerType == ComputerType.Spectrum128K ?
                snapshot.Memory.Banks[5] :
                snapshot.Memory.Banks[0];

            var isUlaPlus = snapshot.UlaPlus is { IsEnabled: true, IsActive: true };

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

                        if (isUlaPlus)
                        {
                            color = (bitmap & FastLookup.BitMasks[bit]) != 0 ?
                                snapshot.UlaPlus!.GetInkColor(attribute) :
                                snapshot.UlaPlus!.GetPaperColor(attribute);
                        }
                        else
                        {
                            color = (bitmap & FastLookup.BitMasks[bit]) != 0 ^ isFlashOn ?
                                attributeData.Ink :
                                attributeData.Paper;
                        }

                        Buffer[bufferIndex + bit] = (int)color.Abgr;
                    }
                }
            }

            return Buffer;
        }

        public Color GetBorderColor()
        {
            if (snapshot.UlaPlus is { IsEnabled: true, IsActive: true } ulaPlus)
            {
                return ulaPlus.GetBorderColor(snapshot.Border);
            }

            return SpectrumPalette.GetBorderColor(snapshot.Border);
        }
    }

    extension(UlaPlusState ulaPlusState)
    {
        private Color GetInkColor(byte attribute)
        {
            var paletteIndex = attribute >> 6;
            var colorIndex = attribute & 0x07;

            return ulaPlusState.PaletteColors[paletteIndex][colorIndex];
        }

        private Color GetPaperColor(byte attribute)
        {
            var paletteIndex = attribute >> 6;
            var colorIndex = ((attribute >> 3) & 0x07) | 8;

            return ulaPlusState.PaletteColors[paletteIndex][colorIndex];
        }

        private Color GetBorderColor(byte borderIndex) =>
            ulaPlusState.PaletteColors[0][(borderIndex & 0x07) | 8];
    }
}