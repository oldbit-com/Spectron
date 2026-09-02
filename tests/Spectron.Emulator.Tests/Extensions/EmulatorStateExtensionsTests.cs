using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Extensions;
using OldBit.Spectron.Emulation.Screen;
using OldBit.Spectron.Emulation.State;
using OldBit.Spectron.Emulation.State.Components;

namespace OldBit.Spectron.Emulator.Tests.Extensions;

public class EmulatorStateExtensionsTests
{
    [Fact]
    public void GetBorderColor_WhenUlaPlusDisabled_ReturnsSpectrumPaletteColor()
    {
        var snapshot = new StateSnapshot { Border = 2 };

        snapshot.GetBorderColor().ShouldBe(SpectrumPalette.GetBorderColor(2));
    }

    [Fact]
    public void GetBorderColor_WhenUlaPlusEnabledButInactive_ReturnsSpectrumPaletteColor()
    {
        var snapshot = new StateSnapshot
        {
            Border = 2,
            UlaPlus = new UlaPlusState
            {
                IsEnabled = true,
                IsActive = false,
                PaletteColors = CreatePalette(),
            },
        };

        snapshot.GetBorderColor().ShouldBe(SpectrumPalette.GetBorderColor(2));
    }

    [Fact]
    public void GetBorderColor_WhenUlaPlusEnabledAndActive_ReturnsUlaPlusPaletteColor()
    {
        var expected = new Color(0x12, 0x34, 0x56);
        var palette = CreatePalette();
        palette[0][(2 & 0x07) | 8] = expected;

        var snapshot = new StateSnapshot
        {
            Border = 2,
            UlaPlus = new UlaPlusState
            {
                IsEnabled = true,
                IsActive = true,
                PaletteColors = palette,
            },
        };

        snapshot.GetBorderColor().ShouldBe(expected);
    }

    [Fact]
    public void GetScreenshot_WhenUlaPlusActive_UsesUlaPlusPaletteRegardlessOfPaletteGroup()
    {
        // Bitmap all zeros -> every pixel is paper. Attribute 0 -> palette 0, paper colour index 8.
        // PaletteGroup is 0 here, proving the renderer keys off IsActive, not PaletteGroup.
        var expected = new Color(0x11, 0x22, 0x33);
        var palette = CreatePalette();
        palette[0][8] = expected;

        var snapshot = new StateSnapshot
        {
            ComputerType = ComputerType.Spectrum48K,
            UlaPlus = new UlaPlusState
            {
                IsEnabled = true,
                IsActive = true,
                PaletteGroup = 0,
                PaletteColors = palette,
            },
        };
        snapshot.Memory.SetBank(new byte[0xC000], pageNumber: 0);

        var screenshot = snapshot.GetScreenshot();

        screenshot[0].ShouldBe((int)expected.Abgr);
    }

    [Fact]
    public void GetScreenshot_WhenUlaPlusInactive_UsesStandardPalette()
    {
        // Attribute 0b0000_1000 -> blue paper in the standard palette; bitmap all zeros -> all paper.
        var memory = new byte[0xC000];
        memory[0x1800] = 0b0000_1000;

        var snapshot = new StateSnapshot
        {
            ComputerType = ComputerType.Spectrum48K,
            UlaPlus = new UlaPlusState
            {
                IsEnabled = true,
                IsActive = false,
                PaletteGroup = 0x3F,
                PaletteColors = CreatePalette(),
            },
        };
        snapshot.Memory.SetBank(memory, pageNumber: 0);

        var screenshot = snapshot.GetScreenshot();

        screenshot[0].ShouldBe((int)SpectrumPalette.Blue.Abgr);
    }

    private static Color[][] CreatePalette() =>
        [new Color[16], new Color[16], new Color[16], new Color[16]];
}
