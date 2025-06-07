using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices;
using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Spectron.Emulation.Screen;

namespace OldBit.Spectron.Emulator.Tests.Screen;

public class ScreenBufferTests
{
    [Fact]
    public void Test()
    {
        var random = new Random(738245);
        var rom = Enumerable.Repeat(0, 16384).Select(_ => (byte)random.Next(0, 256)).ToArray();
        var memory = new Memory48K(rom);
        var ulaPlus = new UlaPlus();

        var buffer = new ScreenBuffer(Hardware.Spectrum48K, memory, ulaPlus);

        buffer.UpdateBorder(SpectrumPalette.Cyan, 224);
        buffer.UpdateBorder(SpectrumPalette.White, 448);

       // TODO: Write some tests for the frame buffer, not so easy
    }
}