using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Screen;

namespace OldBit.Spectron.Emulator.Tests;

public class ContentTests
{
    [Theory]
    [InlineData(0, 14336, 0x0000, 0x1800)]
    [InlineData(1, 14344, 0x0002, 0x1802)]
    [InlineData(15, 14456, 0x001E, 0x181E)]
    [InlineData(16, 14560, 0x0100, 0x1800)]
    [InlineData(3056, 57120, 0x17E0, 0x1AE0)]
    [InlineData(3071, 57240, 0x17FE, 0x1AFE)]
    public void ContentRenderer_ShouldBuildScreenEventsTableSpectrum48(int index, int expectedTicks, Word expectedBitmapAddress, Word expectedAttributeAddress)
    {
        var eventsTable = FastLookup.GetScreenRenderEvents(Hardware.Spectrum48K);

        eventsTable[index].Ticks.ShouldBe(expectedTicks);
        eventsTable[index].BitmapAddress.ShouldBe(expectedBitmapAddress);
        eventsTable[index].AttributeAddress.ShouldBe(expectedAttributeAddress);
    }

    [Theory]
    [InlineData(0, 14362, 0x0000, 0x1800)]
    [InlineData(1, 14370, 0x0002, 0x1802)]
    [InlineData(15, 14482, 0x001E, 0x181E)]
    [InlineData(16, 14590, 0x0100, 0x1800)]
    [InlineData(3056, 57910, 0x17E0, 0x1AE0)]
    [InlineData(3071, 58030, 0x17FE, 0x1AFE)]
    public void ContentRenderer_ShouldBuildScreenEventsTableSpectrum128(int index, int expectedTicks, Word expectedBitmapAddress, Word expectedAttributeAddress)
    {
        var eventsTable = FastLookup.GetScreenRenderEvents(Hardware.Spectrum128K);

        eventsTable[index].Ticks.ShouldBe(expectedTicks);
        eventsTable[index].BitmapAddress.ShouldBe(expectedBitmapAddress);
        eventsTable[index].AttributeAddress.ShouldBe(expectedAttributeAddress);
    }
}