using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices;
using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Spectron.Emulation.Screen;

namespace OldBit.Spectron.Emulator.Tests.Screen;

public class ScreenBufferTests
{
    private const int RegisterPort = 0xBF3B;
    private const int DataPort = 0xFF3B;

    private static readonly Color BrightGreen = new(0x00, 0xFF, 0x00);

    [Fact]
    public void Test()
    {
        var random = new Random(738245);
        var rom = Enumerable.Repeat(0, 16384).Select(_ => (byte)random.Next(0, 256)).ToArray();
        var memory = new Memory48K(rom);
        var ulaPlus = new UlaPlus();

        var screenBuffer = new ScreenBuffer(Hardware.Spectrum48K, memory, ulaPlus);

        screenBuffer.UpdateBorder(5, 224);
        screenBuffer.UpdateBorder(7, 448);

       // TODO: Write some tests for the frame buffer, not so easy
    }

    [Fact]
    public void UpdateBorder_ShouldUseStandardColor_WhenUlaPlusIsNotActive()
    {
        var ulaPlus = new UlaPlus { IsEnabled = true };
        var screenBuffer = CreateScreenBuffer(ulaPlus);

        SetPaletteEntry(ulaPlus, entry: 8, color: 0xE0);

        screenBuffer.UpdateBorder(2);
        screenBuffer.EndFrame(Hardware.Spectrum48K.TicksPerFrame);

        screenBuffer.FrameBuffer.Pixels[100..5000].ShouldAllBe(color => color == SpectrumPalette.Red);
    }

    [Fact]
    public void UpdateBorder_ShouldUseUlaPlusPalette_WhenUlaPlusIsActive()
    {
        var ulaPlus = new UlaPlus { IsEnabled = true };
        var screenBuffer = CreateScreenBuffer(ulaPlus);

        // BORDER 2 uses PAPER 2 of the first CLUT, which is palette entry 10
        SetPaletteEntry(ulaPlus, entry: 10, color: 0xE0);
        Activate(ulaPlus);

        screenBuffer.UpdateBorder(2);
        screenBuffer.EndFrame(Hardware.Spectrum48K.TicksPerFrame);

        screenBuffer.FrameBuffer.Pixels[100..5000].ShouldAllBe(color => color == BrightGreen);
    }

    [Fact]
    public void RefreshBorder_ShouldUpdateBorder_WhenUlaPlusPaletteChanges()
    {
        var ulaPlus = new UlaPlus { IsEnabled = true };
        var screenBuffer = CreateScreenBuffer(ulaPlus);

        Activate(ulaPlus);
        screenBuffer.UpdateBorder(2);

        SetPaletteEntry(ulaPlus, entry: 10, color: 0xE0);
        screenBuffer.RefreshBorder();
        screenBuffer.EndFrame(Hardware.Spectrum48K.TicksPerFrame);

        screenBuffer.FrameBuffer.Pixels[100..5000].ShouldAllBe(color => color == BrightGreen);
    }

    [Fact]
    public void UpdateBorder_ShouldUseSecondUlaPlusClut_WhenTimexHiResIsActive()
    {
        var ulaPlus = new UlaPlus { IsEnabled = true };
        var screenBuffer = CreateScreenBuffer(ulaPlus, Hardware.Timex2048);

        // Hi-res PAPER 5 uses palette entry 13 of the second CLUT
        SetPaletteEntry(ulaPlus, entry: 16 + 13, color: 0xE0);
        Activate(ulaPlus);

        screenBuffer.ChangeScreenMode(
            ScreenMode.TimexHiRes, SpectrumPalette.Black, SpectrumPalette.White, paperIndex: 5, frameTicks: 0);

        screenBuffer.EndFrame(Hardware.Timex2048.TicksPerFrame);

        screenBuffer.FrameBuffer.Pixels[100..5000].ShouldAllBe(color => color == BrightGreen);
    }

    private static ScreenBuffer CreateScreenBuffer(UlaPlus ulaPlus, HardwareSettings? hardware = null) =>
        new(hardware ?? Hardware.Spectrum48K, new Memory48K(new byte[16384]), ulaPlus);

    private static void SetPaletteEntry(UlaPlus ulaPlus, int entry, byte color)
    {
        ulaPlus.WritePort(RegisterPort, (byte)entry);
        ulaPlus.WritePort(DataPort, color);
    }

    private static void Activate(UlaPlus ulaPlus)
    {
        ulaPlus.WritePort(RegisterPort, 0x40);
        ulaPlus.WritePort(DataPort, 0x01);
    }
}
