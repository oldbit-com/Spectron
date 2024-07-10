using OldBit.ZXSpectrum.Emulator.Hardware;
using OldBit.ZXSpectrum.Emulator.Screen;

namespace OldBit.ZXSpectrum.Emulator.UnitTests;

public class ContentRendererTests
{
    [Fact]
    public void Test()
    {
        var contentRenderer = new ContentRenderer(new ScreenBuffer(Colors.White), new Memory48K());
    }
}