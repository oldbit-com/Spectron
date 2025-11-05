using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices.Memory;
using Shouldly;

namespace OldBit.Spectron.Emulator.Tests.Devices.Memory;

public class ContentionProvider48KTests
{
    private readonly ContentionProvider _contention = new(
        Hardware.Spectrum48K.ContentionStartTicks,
        Hardware.Spectrum48K.TicksPerLine);

    [Theory]
    [InlineData(16384, 14336, 6)]
    [InlineData(16384, 14337, 5)]
    [InlineData(16384, 14338, 4)]
    [InlineData(16384, 14339, 3)]
    [InlineData(16384, 14340, 2)]
    [InlineData(16384, 14341, 1)]
    [InlineData(16384, 14342, 0)]
    [InlineData(16384, 14343, 0)]
    [InlineData(16384, 14560, 6)]
    [InlineData(16384, 14561, 5)]
    [InlineData(16384, 14562, 4)]
    [InlineData(16384, 14563, 3)]
    [InlineData(16384, 14564, 2)]
    [InlineData(16384, 14565, 1)]
    [InlineData(16384, 14566, 0)]
    [InlineData(16384, 14567, 0)]
    [InlineData(32767, 14344, 6)]
    [InlineData(32767, 14345, 5)]
    [InlineData(32767, 14346, 4)]
    [InlineData(32767, 14347, 3)]
    [InlineData(32767, 14348, 2)]
    [InlineData(32767, 14349, 1)]
    [InlineData(32767, 14350, 0)]
    [InlineData(32767, 14351, 0)]
    [InlineData(16384, 57120, 6)]
    [InlineData(16384, 57121, 5)]
    [InlineData(16384, 57122, 4)]
    [InlineData(16384, 57123, 3)]
    [InlineData(16384, 57124, 2)]
    [InlineData(16384, 57125, 1)]
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