using OldBit.Spectron.Emulation.Screen;
using OldBit.Spectron.Emulation.Screen.Palettes;

namespace OldBit.Spectron.Emulator.Tests.Screen.Palettes;

public class SpectrumPaletteTests
{
    private static readonly Color[] Normal = SpectrumPalette.StandardColors.ToArray();
    private static readonly Color[] Bright = SpectrumPalette.StandardBrightColors.ToArray();

    [Fact]
    public void GetInkColor_MapsEveryAttribute()
    {
        for (var attr = 0; attr <= byte.MaxValue; attr++)
        {
            var expected = (attr & 0x40) != 0 ? Bright[attr & 0x07] : Normal[attr & 0x07];

            SpectrumPalette.GetInkColor(attr).ShouldBe(expected, $"attribute 0x{attr:X2}");
        }
    }

    [Fact]
    public void GetPaperColor_MapsEveryAttribute()
    {
        for (var attr = 0; attr <= byte.MaxValue; attr++)
        {
            var index = (attr >> 3) & 0x07;
            var expected = (attr & 0x40) != 0 ? Bright[index] : Normal[index];

            SpectrumPalette.GetPaperColor(attr).ShouldBe(expected, $"attribute 0x{attr:X2}");
        }
    }

    [Fact]
    public void GetBorderColor_UsesLowestThreeBits()
    {
        for (var value = 0; value <= byte.MaxValue; value++)
        {
            SpectrumPalette.GetBorderColor(value).ShouldBe(Normal[value & 0x07], $"value 0x{value:X2}");
        }
    }

    [Fact]
    public void DefaultPalette_UsesStandardColors()
    {
        var palette = new Palette();

        palette.Ink.ShouldBe(SpectrumPalette.StandardColors);
        palette.Paper.ShouldBe(SpectrumPalette.StandardColors);
        palette.Border.ShouldBe(SpectrumPalette.StandardColors);
        palette.Bright.Ink.ShouldBe(SpectrumPalette.StandardBrightColors);
        palette.Bright.Paper.ShouldBe(SpectrumPalette.StandardBrightColors);
    }
}
