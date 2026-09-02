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

    private static Color[][] CreatePalette() =>
        [new Color[16], new Color[16], new Color[16], new Color[16]];
}
