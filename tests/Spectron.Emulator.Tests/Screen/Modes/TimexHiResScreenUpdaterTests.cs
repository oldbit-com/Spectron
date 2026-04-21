using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices;
using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Spectron.Emulation.Screen;
using static OldBit.Spectron.Emulation.Screen.SpectrumPalette;

namespace OldBit.Spectron.Emulator.Tests.Screen.Modes;

public class TimexHiResScreenUpdaterTests
{
    private readonly FrameBuffer _frameBuffer = new();
    private readonly Memory48K _memory;
    private readonly Content _content;

    public TimexHiResScreenUpdaterTests()
    {
        _memory = new Memory48K(RomReader.ReadRom(RomType.Original48));

        _content = new Content(Hardware.Timex2048, _frameBuffer, _memory, new UlaPlus());
        _content.ChangeScreenMode(ScreenMode.TimexHiRes, BrightBlue, BrightYellow);
    }

    [Fact]
    public void FrameBuffer_ShouldBeUpdatedWithScreenData()
    {
        _memory.Write(0x4000, 0xAA);
        _memory.Write(0x4001, 0x81);
        _memory.Write(0x6000, 0x55);
        _memory.Write(0x6001, 0x66);

        _content.UpdateFrameBuffer(14336);

        _frameBuffer.Pixels[0xB060..0xB068].ShouldBe([BrightBlue, BrightYellow, BrightBlue, BrightYellow, BrightBlue, BrightYellow, BrightBlue, BrightYellow]);
        _frameBuffer.Pixels[0xB068..0xB070].ShouldBe([BrightYellow, BrightBlue, BrightYellow, BrightBlue, BrightYellow, BrightBlue, BrightYellow, BrightBlue]);
        _frameBuffer.Pixels[0xB070..0xB078].ShouldBe([BrightBlue, BrightYellow, BrightYellow, BrightYellow, BrightYellow, BrightYellow, BrightYellow, BrightBlue]);
        _frameBuffer.Pixels[0xB078..0xB080].ShouldBe([BrightYellow, BrightBlue, BrightBlue, BrightYellow, BrightYellow, BrightBlue, BrightBlue, BrightYellow]);
    }
}