using FluentAssertions;
using OldBit.ZXSpectrum.Emulator.Screen;

namespace OldBit.ZXSpectrum.Emulator.UnitTests;

public class ScreenAddressTests
{
    [Theory]
    [InlineData(0, 0, 0x4000)]
    [InlineData(0, 1, 0x4100)]
    [InlineData(0, 2, 0x4200)]
    [InlineData(0, 3, 0x4300)]
    [InlineData(0, 4, 0x4400)]
    [InlineData(0, 5, 0x4500)]
    [InlineData(0, 6, 0x4600)]
    [InlineData(0, 7, 0x4700)]
    [InlineData(31, 15, 0x473F)]
    [InlineData(31, 183, 0x57DF)]
    [InlineData(0, 184, 0x50E0)]
    [InlineData(0, 185, 0x51E0)]
    [InlineData(0, 186, 0x52E0)]
    [InlineData(0, 187, 0x53E0)]
    [InlineData(0, 188, 0x54E0)]
    [InlineData(0, 189, 0x55E0)]
    [InlineData(0, 190, 0x56E0)]
    [InlineData(0, 191, 0x57E0)]
    public void ScreenAddress_ShouldBeCalculatedForXY(byte x, byte y, Word expectedAddress)
    {
        var address = ScreenAddress.Calculate(x, y);

        address.Should().Be(expectedAddress);
    }
}