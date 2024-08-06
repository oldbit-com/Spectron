using FluentAssertions;
using OldBit.Spectral.Emulation.Screen;

namespace OldBit.ZXSpectrum.Emulator.Tests;

public class ContentTests
{
    [Theory]
    [InlineData(0, 14336, 0x0000, 0x1800)]
    [InlineData(1, 14344, 0x0002, 0x1802)]
    [InlineData(15, 14456, 0x001E, 0x181E)]
    [InlineData(16, 14560, 0x0100, 0x1800)]
    [InlineData(3056, 57120, 0x17E0, 0x1AE0)]
    [InlineData(3071, 57240, 0x17FE, 0x1AFE)]
    public void ContentRenderer_ShouldBuildScreenEventsTable(int index, int expectedTicks, Word expectedBitmapAddress, Word expectedAttributeAddress)
    {
        var eventsTable = FastLookup.ScreenRenderEvents;

        eventsTable[index].Ticks.Should().Be(expectedTicks);
        eventsTable[index].BitmapAddress.Should().Be(expectedBitmapAddress);
        eventsTable[index].AttributeAddress.Should().Be(expectedAttributeAddress);
    }
}