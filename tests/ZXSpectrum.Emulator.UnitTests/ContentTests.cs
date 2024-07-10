using FluentAssertions;
using OldBit.ZXSpectrum.Emulator.Screen;

namespace OldBit.ZXSpectrum.Emulator.UnitTests;

public class ContentTests
{
    [Fact]
    public void ContentRenderer_TicksTableShouldHave6144Elements()
    {
        FastLookup.ContentTicks.Length.Should().Be(6144);
    }

    [Theory]
    [InlineData(0, 14338)]
    [InlineData(1, 14342)]
    [InlineData(31, 14462)]
    [InlineData(32, 14562)]
    [InlineData(6112, 57122)]
    [InlineData(6113, 57126)]
    [InlineData(6143, 57246)]
    public void ContentRenderer_ShouldBuildTicksTable(int index, int expectedTick)
    {
        FastLookup.ContentTicks[index].Should().Be(expectedTick);
    }
}