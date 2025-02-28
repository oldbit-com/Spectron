using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices.Memory;
using Shouldly;

namespace OldBit.Spectron.Emulator.Tests.Devices.Memory;

public class ContentionProvider48KTests
{
    private readonly ContentionProvider _contention = new(
        Hardware.Spectrum48K.FirstPixelTicks,
        Hardware.Spectrum48K.TicksPerLine);

    [Theory]
    [InlineData(16384, 14335, 6)]
    [InlineData(16384, 14336, 5)]
    [InlineData(16384, 14337, 4)]
    [InlineData(16384, 14338, 3)]
    [InlineData(16384, 14339, 2)]
    [InlineData(16384, 14340, 1)]
    [InlineData(16384, 14341, 0)]
    [InlineData(16384, 14342, 0)]
    [InlineData(16384, 14559, 6)]
    [InlineData(16384, 14560, 5)]
    [InlineData(16384, 14561, 4)]
    [InlineData(16384, 14562, 3)]
    [InlineData(16384, 14563, 2)]
    [InlineData(16384, 14564, 1)]
    [InlineData(16384, 14565, 0)]
    [InlineData(16384, 14566, 0)]
    [InlineData(32767, 14343, 6)]
    [InlineData(32767, 14344, 5)]
    [InlineData(32767, 14345, 4)]
    [InlineData(32767, 14346, 3)]
    [InlineData(32767, 14347, 2)]
    [InlineData(32767, 14348, 1)]
    [InlineData(32767, 14349, 0)]
    [InlineData(32767, 14350, 0)]
    [InlineData(16384, 57119, 6)]
    [InlineData(16384, 57120, 5)]
    [InlineData(16384, 57121, 4)]
    [InlineData(16384, 57122, 3)]
    [InlineData(16384, 57123, 2)]
    [InlineData(16384, 57124, 1)]
    public void WhenContentedAddress_ShouldReturnExpectedContention(Word address, int state, int expectedContention)
    {
        var contention = _contention.GetMemoryContention(state, address);

        contention.ShouldBe(expectedContention);
    }

    [Theory]
    [InlineData(16383, 14335)]
    [InlineData(32768, 14348)]
    public void WhenNotContendedAddress_ShouldReturnZeroContention(Word address, int state)
    {
        var contention = _contention.GetMemoryContention(state, address);

        contention.ShouldBe(0);
    }
}