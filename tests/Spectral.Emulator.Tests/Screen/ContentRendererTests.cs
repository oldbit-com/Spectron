using OldBit.Spectral.Emulator.Hardware;
using OldBit.Spectral.Emulator.Rom;
using OldBit.Spectral.Emulator.Screen;

namespace OldBit.ZXSpectrum.Emulator.Tests.Screen;

public class ContentRendererTests
{
    [Fact]
    public void Test()
    {
        var rom = RomReader.ReadRom(RomType.Original48);
        var contentRenderer = new ContentRenderer(new FrameBuffer(Colors.White), new Memory48K(rom));
    }
}