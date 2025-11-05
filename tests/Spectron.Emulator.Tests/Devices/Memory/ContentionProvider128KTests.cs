using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices.Memory;
using Shouldly;

namespace OldBit.Spectron.Emulator.Tests.Devices.Memory;

public class ContentionProvider128KTests
{
    private readonly ContentionProvider _contention = new(
        Hardware.Spectrum128K.ContentionStartTicks,
        Hardware.Spectrum128K.TicksPerLine);

    [Theory]
    [InlineData(16384, 14362, 6)]
    [InlineData(16384, 14363, 5)]
    [InlineData(16384, 14364, 4)]
    [InlineData(16384, 14365, 3)]
    [InlineData(16384, 14366, 2)]
    [InlineData(16384, 14367, 1)]
    [InlineData(16384, 14368, 0)]
    [InlineData(16384, 14369, 0)]
    [InlineData(16384, 14590, 6)]
    [InlineData(16384, 14591, 5)]
    [InlineData(16384, 14592, 4)]
    [InlineData(16384, 14593, 3)]
    [InlineData(16384, 14594, 2)]
    [InlineData(16384, 14595, 1)]
    [InlineData(16384, 14596, 0)]
    [InlineData(16384, 14597, 0)]
    [InlineData(32767, 14370, 6)]
    [InlineData(32767, 14371, 5)]
    [InlineData(32767, 14372, 4)]
    [InlineData(32767, 14373, 3)]
    [InlineData(32767, 14374, 2)]
    [InlineData(32767, 14375, 1)]
    [InlineData(32767, 14376, 0)]
    [InlineData(32767, 14377, 0)]
    [InlineData(16384, 57910, 6)]
    [InlineData(16384, 57911, 5)]
    [InlineData(16384, 57912, 4)]
    [InlineData(16384, 57913, 3)]
    [InlineData(16384, 57914, 2)]
    [InlineData(16384, 57915, 1)]
    public void WhenContentedAddress_ShouldReturnExpectedContention(Word address, int state, int expectedContention)
    {
        var contention = _contention.GetMemoryContention(state, address);

        contention.ShouldBe(expectedContention);
    }

    [Theory]
    [InlineData(16383)]
    [InlineData(32768)]
    public void WhenNotContendedAddress_ShouldReturnZeroContention(Word address)
    {
        var isContended = _contention.IsAddressContended(address);

        isContended.ShouldBeFalse();
    }
}