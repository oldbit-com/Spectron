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
    [InlineData(16384, 14361, 6)]
    [InlineData(16384, 14362, 5)]
    [InlineData(16384, 14363, 4)]
    [InlineData(16384, 14364, 3)]
    [InlineData(16384, 14365, 2)]
    [InlineData(16384, 14366, 1)]
    [InlineData(16384, 14367, 0)]
    [InlineData(16384, 14368, 0)]
    [InlineData(16384, 14589, 6)]
    [InlineData(16384, 14590, 5)]
    [InlineData(16384, 14591, 4)]
    [InlineData(16384, 14592, 3)]
    [InlineData(16384, 14593, 2)]
    [InlineData(16384, 14594, 1)]
    [InlineData(16384, 14595, 0)]
    [InlineData(16384, 14596, 0)]
    [InlineData(32767, 14369, 6)]
    [InlineData(32767, 14370, 5)]
    [InlineData(32767, 14371, 4)]
    [InlineData(32767, 14372, 3)]
    [InlineData(32767, 14373, 2)]
    [InlineData(32767, 14374, 1)]
    [InlineData(32767, 14375, 0)]
    [InlineData(32767, 14376, 0)]
    [InlineData(16384, 57909, 6)]
    [InlineData(16384, 57910, 5)]
    [InlineData(16384, 57911, 4)]
    [InlineData(16384, 57912, 3)]
    [InlineData(16384, 57913, 2)]
    [InlineData(16384, 57914, 1)]
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