using FluentAssertions;
using OldBit.Spectral.Emulation.Devices;

namespace OldBit.ZXSpectrum.Emulator.Tests.Devices;

public class UlaPlusTests
{
    private const int RegisterPort = 0xBF3B;
    private const int DataPort = 0xFF3B;

    [Fact]
    public void WhenModeGroup_ShouldBeAbleToActiveOrDeactivateUlaPlus()
    {
        var ulaPlus = new UlaPlus();
        ulaPlus.IsActive.Should().BeFalse();

        // Set Mode group
        ulaPlus.WritePort(RegisterPort, 0b_01_000000);

        // Enable ULA+
        ulaPlus.WritePort(DataPort, 1);
        ulaPlus.IsActive.Should().BeTrue();

        // Disable ULA+
        ulaPlus.WritePort(DataPort, 0);
        ulaPlus.IsActive.Should().BeFalse();
    }

    [Theory]
    [InlineData(0b00_000_100, 0xFF00B500, 0xFF000000)]
    [InlineData(0b11_000_100, 0xFFFF00FF, 0xFFFFFFFF)]
    [InlineData(0b11_111_111, 0xFF000000, 0xFF000000)]
    [InlineData(0b11_110_011, 0xFF00FF00, 0xFFFF0000)]
    [InlineData(0b10_001_010, 0xFFB5B500, 0xFF00B5B5)]
    [InlineData(0b01_011_100, 0xFF00FF00, 0xFFFF00FF)]
    public void WhenPaletteIsDefined_ShouldGetInkAndPaper(byte attribute, uint expectedInkArgb, uint expectedPaperArgb)
    {
        var ulaPlus = new UlaPlus();
        SetupStandardPalette(ulaPlus);

        var inkColor = ulaPlus.GetInkColor(attribute);
        var paperColor = ulaPlus.GetPaperColor(attribute);

        inkColor.Abgr.Should().Be((int)expectedInkArgb);
        paperColor.Abgr.Should().Be((int)expectedPaperArgb);
    }

    [Fact]
    public void LastWrittenData_ShouldBeReturnedWhenRead()
    {
        var ulaPlus = new UlaPlus();

        ulaPlus.WritePort(DataPort, 0xAB);

        var value = ulaPlus.ReadPort(DataPort);

        value.Should().Be(0xAB);
    }

    // Standard Spectrum Palette for ULA+ (G..R..B.)
    private static readonly byte[] StandardColors = [0b00000000, 0b00000010, 0b00010100, 0b00010110, 0b10100000, 0b10100010, 0b10110100, 0b10110110];
    private static readonly byte[] BrightColors = [0b00000000, 0b00000011, 0b00011100, 0b00011111, 0b11100000, 0b11100011, 0b11111100, 0b11111111];
    private static readonly byte[] FlashColors = [0b10110110, 0b10110100, 0b10100010, 0b10100000, 0b00010110, 0b00010100, 0b00000010, 0b00000000];
    private static readonly byte[] FlashBrightColors = [0b11111111, 0b11111100, 0b11100011, 0b11100000, 0b00011111, 0b00011100, 0b00000011, 0b00000000];

    private static void SetupStandardPalette(UlaPlus ulaPlus)
    {
        for (byte i = 0; i < StandardColors.Length; i++)
        {
            ulaPlus.WritePort(RegisterPort, i);
            ulaPlus.WritePort(DataPort, StandardColors[i]);
        }

        for (byte i = 0; i < StandardColors.Length; i++)
        {
            ulaPlus.WritePort(RegisterPort, (byte)(i + 8));
            ulaPlus.WritePort(DataPort, StandardColors[i]);
        }

        for (byte i = 0; i < BrightColors.Length; i++)
        {
            ulaPlus.WritePort(RegisterPort, (byte)(i + 16));
            ulaPlus.WritePort(DataPort, BrightColors[i]);
        }

        for (byte i = 0; i < BrightColors.Length; i++)
        {
            ulaPlus.WritePort(RegisterPort, (byte)(i + 24));
            ulaPlus.WritePort(DataPort, BrightColors[i]);
        }

        for (byte i = 0; i < FlashColors.Length; i++)
        {
            ulaPlus.WritePort(RegisterPort, (byte)(i + 32));
            ulaPlus.WritePort(DataPort, FlashColors[i]);
        }

        for (byte i = 0; i < FlashColors.Length; i++)
        {
            ulaPlus.WritePort(RegisterPort, (byte)(i + 40));
            ulaPlus.WritePort(DataPort, FlashColors[i]);
        }

        for (byte i = 0; i < FlashBrightColors.Length; i++)
        {
            ulaPlus.WritePort(RegisterPort, (byte)(i + 48));
            ulaPlus.WritePort(DataPort, FlashBrightColors[i]);
        }

        for (byte i = 0; i < FlashBrightColors.Length; i++)
        {
            ulaPlus.WritePort(RegisterPort, (byte)(i + 56));
            ulaPlus.WritePort(DataPort, FlashBrightColors[i]);
        }
    }
}