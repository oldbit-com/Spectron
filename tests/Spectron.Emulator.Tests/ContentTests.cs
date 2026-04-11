using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Screen;

namespace OldBit.Spectron.Emulator.Tests;

public class ContentTests
{
    [Theory]
    [InlineData(0, 14336, 0x0000, 0x1800, 22576)]
    [InlineData(1, 14344, 0x0002, 0x1802, 22592)]
    [InlineData(15, 14456, 0x001E, 0x181E, 22816)]
    [InlineData(16, 14560, 0x0100, 0x1800, 22928)]
    [InlineData(3056, 57120, 0x17E0, 0x1AE0, 89808)]
    [InlineData(3071, 57240, 0x17FE, 0x1AFE, 90048)]
    public void ContentRenderer_ShouldBuildScreenEventsTableSpectrum48(int index, int expectedTicks,
        Word expectedBitmapAddress, Word expectedAttributeAddress, int expectedFrameBufferIndex)
    {
        var eventsTable = FastLookup.GetScreenRenderEvents(Hardware.Spectrum48K, new FrameBuffer());

        eventsTable[index].Ticks.ShouldBe(expectedTicks);
        eventsTable[index].BitmapAddress.ShouldBe(expectedBitmapAddress);
        eventsTable[index].AttributeAddress.ShouldBe(expectedAttributeAddress);
        eventsTable[index].FrameBufferIndex.ShouldBe(expectedFrameBufferIndex);
    }

    [Theory]
    [InlineData(0, 14362, 0x0000, 0x1800, 22224)]
    [InlineData(1, 14370, 0x0002, 0x1802, 22240)]
    [InlineData(15, 14482, 0x001E, 0x181E, 22464)]
    [InlineData(16, 14590, 0x0100, 0x1800, 22576)]
    [InlineData(3056, 57910, 0x17E0, 0x1AE0, 89456)]
    [InlineData(3071, 58030, 0x17FE, 0x1AFE, 89696)]
    public void ContentRenderer_ShouldBuildScreenEventsTableSpectrum128(int index, int expectedTicks,
        Word expectedBitmapAddress, Word expectedAttributeAddress, int expectedFrameBufferIndex)
    {
        var eventsTable = FastLookup.GetScreenRenderEvents(Hardware.Spectrum128K, new FrameBuffer());

        eventsTable[index].Ticks.ShouldBe(expectedTicks);
        eventsTable[index].BitmapAddress.ShouldBe(expectedBitmapAddress);
        eventsTable[index].AttributeAddress.ShouldBe(expectedAttributeAddress);
        eventsTable[index].FrameBufferIndex.ShouldBe(expectedFrameBufferIndex);
    }

    [Theory]
    [InlineData(0, 14336, 0x0000, 0x1800, 45152)]
    [InlineData(1, 14344, 0x0002, 0x1802, 45184)]
    [InlineData(15, 14456, 0x001E, 0x181E, 45632)]
    [InlineData(16, 14560, 0x0100, 0x1800, 45856)]
    [InlineData(3056, 57120, 0x17E0, 0x1AE0, 179616)]
    [InlineData(3071, 57240, 0x17FE, 0x1AFE, 180096)]
    public void ContentRenderer_ShouldBuildScreenEventsTableTimexHiRes(int index, int expectedTicks,
        Word expectedBitmapAddress, Word expectedAttributeAddress, int expectedFrameBufferIndex)
    {
        var frameBuffer = new FrameBuffer();
        frameBuffer.ChangeScreenMode(ScreenMode.TimexHiRes);

        var eventsTable = FastLookup.GetScreenRenderEvents(Hardware.Timex2048, frameBuffer, ScreenMode.TimexHiRes);

        eventsTable[index].Ticks.ShouldBe(expectedTicks);
        eventsTable[index].BitmapAddress.ShouldBe(expectedBitmapAddress);
        eventsTable[index].AttributeAddress.ShouldBe(expectedAttributeAddress);
        eventsTable[index].FrameBufferIndex.ShouldBe(expectedFrameBufferIndex);
    }
}