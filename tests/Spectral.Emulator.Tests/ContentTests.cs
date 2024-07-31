using FluentAssertions;
using OldBit.Spectral.Emulator.Screen;

namespace OldBit.ZXSpectrum.Emulator.UnitTests;

public class ContentTests
{
    [Theory]
    [InlineData(0, 14335, 0x4000, 0x5800)]
    [InlineData(1, 14343, 0x4002, 0x5802)]
    [InlineData(15, 14455, 0x401E, 0x581E)]
    [InlineData(16, 14559, 0x4100, 0x5800)]
    [InlineData(3056, 57119, 0x57E0, 0x5AE0)]
    [InlineData(3071, 57239, 0x57FE, 0x5AFE)]
    public void ContentRenderer_ShouldBuildScreenEventsTable(int index, int expectedTicks, Word expectedBitmapAddress, Word expectedAttributeAddress)
    {
        var eventsTable = FastLookup.BuildScreenEventsTable();

        eventsTable[index].Ticks.Should().Be(expectedTicks);
        eventsTable[index].BitmapAddress.Should().Be(expectedBitmapAddress);
        eventsTable[index].AttributeAddress.Should().Be(expectedAttributeAddress);
    }
}