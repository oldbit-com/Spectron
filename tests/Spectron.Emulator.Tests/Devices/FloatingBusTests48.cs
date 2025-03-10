using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices;
using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Z80Cpu;
using Shouldly;

namespace OldBit.Spectron.Emulator.Tests.Devices;

public class FloatingBusTests48
{
    private readonly FloatingBus _floatingBus;
    private readonly Memory48K _memory;
    private readonly Clock _clock;

    public FloatingBusTests48()
    {
        _memory = new Memory48K(new byte[16384]);
        _clock = new Clock();

        _floatingBus = new FloatingBus(Hardware.Spectrum48K, _memory, _clock);
    }

    [Fact]
    public void FloatingBus_ShouldBeEnabledByDefault()
    {
        _floatingBus.IsEnabled.ShouldBeTrue();
    }

    [Theory]
    [InlineData(0x02)]
    [InlineData(0xFC)]
    [InlineData(0xFE)]
    [InlineData(0xFFFE)]
    [InlineData(0xFDFE)]
    public void FloatingBus_WhenUlaPort_ShouldReturnNull(Word port)
    {
        var value = _floatingBus.ReadPort(port);

        value.ShouldBeNull();
    }

    [Theory]
    [InlineData(14336, 0x00FF, 0x4000, 0xAA)]
    [InlineData(14337, 0x00FF, 0x5800, 0xBB)]
    [InlineData(14338, 0x00FF, 0x4001, 0xCC)]
    [InlineData(14339, 0x00FF, 0x5801, 0xDD)]
    [InlineData(14560, 0x00FF, 0x4100, 0x11)]
    [InlineData(14561, 0x00FF, 0x5800, 0x22)]
    [InlineData(14562, 0x00FF, 0x4101, 0x33)]
    [InlineData(14563, 0x00FF, 0x5801, 0x44)]
    [InlineData(57240, 0x00FF, 0x57FE, 0x45)]
    [InlineData(57241, 0x00FF, 0x5AFE, 0x46)]
    [InlineData(57242, 0x00FF, 0x57FF, 0x47)]
    [InlineData(57243, 0x00FF, 0x5AFF, 0x48)]
    public void WhenReadingUnattachedPort_AndMatchingCycle_ShouldReturnFloatingBusValue(int ticks, Word port, Word address, byte expectedValue)
    {
        _clock.AddTicks(ticks);
        _memory.Write(address, expectedValue);

        var value = _floatingBus.ReadPort(port);

        value.ShouldBe(expectedValue);
    }

    [Theory]
    [InlineData(14340, 0x00FF, 0x4000, 0xAA)]
    [InlineData(14341, 0x00FF, 0x5800, 0xBB)]
    [InlineData(14342, 0x00FF, 0x4001, 0xCC)]
    [InlineData(14343, 0x00FF, 0x5801, 0xDD)]
    [InlineData(14564, 0x00FF, 0x4100, 0x11)]
    [InlineData(14565, 0x00FF, 0x5800, 0x22)]
    [InlineData(14566, 0x00FF, 0x4101, 0x33)]
    [InlineData(14567, 0x00FF, 0x5801, 0x44)]
    public void WhenReadingUnattachedPort_AndNotMatchingCycle_ShouldReturnNull(int ticks, Word port, Word address, byte expectedValue)
    {
        _clock.AddTicks(ticks);
        _memory.Write(address, expectedValue);

        var value = _floatingBus.ReadPort(port);

        value.ShouldBeNull();
    }
}