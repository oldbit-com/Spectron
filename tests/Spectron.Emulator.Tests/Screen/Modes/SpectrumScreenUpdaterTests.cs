using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices;
using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Spectron.Emulation.Screen;
using static OldBit.Spectron.Emulation.Screen.SpectrumPalette;

namespace OldBit.Spectron.Emulator.Tests.Screen.Modes;

public class SpectrumScreenUpdaterTests
{
    private const byte InkMagenta = 0b00000011;
    private const byte PaperCyan = 0b00101000;
    private const byte InkRed = 0b00000010;
    private const byte PaperGreen = 0b00100000;
    private const byte Flash = 0b10000000;

    private readonly FrameBuffer _frameBuffer = new();
    private readonly Memory128K _memory;
    private readonly Content _content;

    public SpectrumScreenUpdaterTests()
    {
        _memory = new Memory128K(
            RomReader.ReadRom(RomType.Original128Bank0),
            RomReader.ReadRom(RomType.Original128Bank1));

        _content = new Content(Hardware.Spectrum128K, _frameBuffer, _memory, new UlaPlus());
        _content.ChangeScreenMode(ScreenMode.Spectrum, White, Black);
    }

    [Fact]
    public void FrameBuffer_ShouldBeFilledWithWhite()
    {
        _frameBuffer.Pixels.ShouldAllBe(c => c == White);
    }

    [Fact]
    public void FrameBuffer_ShouldBeUpdatedWithScreenData()
    {
        _memory.Write(0x4000, 0xAA);
        _memory.Write(0x4001, 0x55);
        _memory.Write(0x5800, InkMagenta | PaperCyan);
        _memory.Write(0x5801, InkRed | PaperGreen);

        _content.UpdateFrameBuffer(14362);

        _frameBuffer.Pixels[..0x56D0].ShouldAllBe(c => c == White);
        _frameBuffer.Pixels[0x56D0..0x56D8].ShouldBe([Magenta, Cyan, Magenta, Cyan, Magenta, Cyan, Magenta, Cyan]);
        _frameBuffer.Pixels[0x56D8..0x56E0].ShouldBe([Green, Red, Green, Red, Green, Red, Green, Red]);
        _frameBuffer.Pixels[0x56E0..].ShouldAllBe(c => c == White);
    }

    [Fact]
    public void FrameBuffer_ShouldNotBeUpdatedIfScreenDataHasNotChanged()
    {
        // Write some screen data
        _memory.Write(0x4000, 0xAA);
        _memory.Write(0x5800, InkMagenta | PaperCyan);

        // Update frame buffer and verify it has been updated
        _content.UpdateFrameBuffer(14362);
        _frameBuffer.Pixels[0x56D0..0x56D8].ShouldBe([Magenta, Cyan, Magenta, Cyan, Magenta, Cyan, Magenta, Cyan]);

        // Clear frame buffer
        Array.Fill(_frameBuffer.Pixels, White);

        // New frame to update screen again, it should not be updated
        _content.NewFrame();
        _content.UpdateFrameBuffer(14362);

        _frameBuffer.Pixels.ShouldAllBe(c => c == White);
    }

    [Fact]
    public void FrameBuffer_ShouldHandleFlashAttribute()
    {
        // Write 8x8 data block
        for (var i = 0; i < 8; i++)
        {
            var address = ScreenAddress.Calculate(0, i);
            _memory.Write(address, 0xAA);
        }

        // Set flash attribute
        _memory.Write(0x5800, InkRed | PaperCyan | Flash);

        // For the first 31 frames, the flash should be off, color should be normal
        for (var i = 0; i < 31; i++)
        {
            _content.UpdateFrameBuffer(14362);
            _frameBuffer.Pixels[0x56D0..0x56D8].ShouldBe([Red, Cyan, Red, Cyan, Red, Cyan, Red, Cyan]);
            _content.NewFrame();
        }

        // Toggle the flash
        _content.UpdateFrameBuffer(Hardware.Spectrum128K.LastPixelTicks);

        var start = 0x56D0;
        var end = start + 8;

        // Whole 8x8 should be inverted
        for (var line = 0; line < 8; line++)
        {
            _frameBuffer.Pixels[start..end].ShouldBe([Cyan, Red, Cyan, Red, Cyan, Red, Cyan, Red]);

            start = 0x56D0 + (ScreenSize.BorderLeft + ScreenSize.ContentWidth + ScreenSize.BorderRight) * (line + 1);
            end = start + 8;
        }
    }

    [Fact]
    public void FrameBuffer_ShouldBeUpdatedWithShadowScreenData()
    {
        _memory.SetPagingMode(0b00000111);  // Page shadow screen

        _memory.Write(0xC000, 0xAA);
        _memory.Write(0xC001, 0x55);
        _memory.Write(0xD800, InkMagenta | PaperCyan);
        _memory.Write(0xD801, InkRed | PaperGreen);

        _memory.SetPagingMode(0b00001000);  // Make second screen active

        _content.UpdateFrameBuffer(14362);

        _frameBuffer.Pixels[..0x56D0].ShouldAllBe(c => c == White);
        _frameBuffer.Pixels[0x56D0..0x56D8].ShouldBe([Magenta, Cyan, Magenta, Cyan, Magenta, Cyan, Magenta, Cyan]);
        _frameBuffer.Pixels[0x56D8..0x56E0].ShouldBe([Green, Red, Green, Red, Green, Red, Green, Red]);
        _frameBuffer.Pixels[0x56E0..].ShouldAllBe(c => c == White);
    }
}