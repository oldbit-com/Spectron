using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices;
using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Spectron.Emulation.Screen;
using static OldBit.Spectron.Emulation.Screen.SpectrumPalette;

namespace OldBit.Spectron.Emulator.Tests.Screen.Modes;

public class TimexHiColorScreenUpdaterTests
{
    private const byte InkMagenta = 0b00000011;
    private const byte PaperCyan = 0b00101000;
    private const byte InkRed = 0b00000010;
    private const byte PaperGreen = 0b00100000;
    private const byte Flash = 0b10000000;

    private readonly FrameBuffer _frameBuffer = new();
    private readonly Memory48K _memory;
    private readonly Content _content;

    public TimexHiColorScreenUpdaterTests()
    {
        _memory = new Memory48K(RomReader.ReadRom(RomType.Original48));

        _content = new Content(Hardware.Timex2048, _frameBuffer, _memory, new UlaPlus());
        _content.ChangeScreenMode(ScreenMode.TimexHiColor, White, Black);
    }

    [Fact]
    public void FrameBuffer_ShouldBeUpdatedWithScreenData()
    {
        _memory.Write(0x4000, 0xAA);
        _memory.Write(0x4001, 0x55);
        _memory.Write(0x6000, InkMagenta | PaperCyan);
        _memory.Write(0x6001, InkRed | PaperGreen);

        _content.UpdateFrameBuffer(14336);

        _frameBuffer.Pixels[..0x5830].ShouldAllBe(c => c == White);
        _frameBuffer.Pixels[0x5830..0x5838].ShouldBe([Magenta, Cyan, Magenta, Cyan, Magenta, Cyan, Magenta, Cyan]);
        _frameBuffer.Pixels[0x5838..0x5840].ShouldBe([Green, Red, Green, Red, Green, Red, Green, Red]);
        _frameBuffer.Pixels[0x5840..].ShouldAllBe(c => c == White);
    }

    [Fact]
    public void FrameBuffer_EachLineShouldBeUpdatedWithAlternatingColor()
    {
        for(var line = 0; line < 192; line++)
        {
            var lineAddress = ScreenAddress.Calculate(0, line);

            var color = line % 2 == 0 ? InkRed | PaperGreen : InkMagenta | PaperCyan;

            for (var i = 0; i < 32; i++)
            {
                _memory.Write((Word)(lineAddress + i), 0x55);
                _memory.Write((Word)(0x2000 + lineAddress + i), (byte)color);
            }
        }

        _content.UpdateFrameBuffer(57248);

        var start = 0x5830;
        var end = start + ScreenSize.ContentWidth;

        for (var line = 0; line < 192; line++)
        {
            var colors = line % 2 == 0 ? new[] { Green, Red } : new[] { Cyan, Magenta };
            var fullLine = Enumerable.Repeat(colors, 4 * 32).SelectMany(a => a).ToArray();

            _frameBuffer.Pixels[start..end].ShouldBe(fullLine);

            start = end + ScreenSize.BorderLeft + ScreenSize.BorderRight;
            end = start + ScreenSize.ContentWidth;
        }
    }

    [Fact]
    public void FrameBuffer_ShouldHandleFlashAttribute()
    {
        _memory.Write(0x4000, 0xAA);
        _memory.Write(0x6000, InkRed | PaperCyan | Flash);

        for (var i = 0; i < 31; i++)
        {
            _content.UpdateFrameBuffer(14336);
            _frameBuffer.Pixels[0x5830..0x5838].ShouldBe([Red, Cyan, Red, Cyan, Red, Cyan, Red, Cyan]);
            _content.NewFrame();
        }

        // Now the flash should be on, color should be inverted
        _content.UpdateFrameBuffer(14336);
        _frameBuffer.Pixels[0x5830..0x5838].ShouldBe([Cyan, Red, Cyan, Red, Cyan, Red, Cyan, Red]);
    }
}