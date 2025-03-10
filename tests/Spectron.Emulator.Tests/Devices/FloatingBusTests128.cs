using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices;
using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Z80Cpu;
using Shouldly;

namespace OldBit.Spectron.Emulator.Tests.Devices;

public class FloatingBusTests128
{
    private readonly FloatingBus _floatingBus;
    private readonly Memory128K _memory;
    private readonly Clock _clock;

    public FloatingBusTests128()
    {
        _memory = new Memory128K(new byte[16384], new byte[16384]);
        _clock = new Clock();

        _floatingBus = new FloatingBus(Hardware.Spectrum128K, _memory, _clock);
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
    [InlineData(14362, 0x00FF, 0x4000, 0xAA)]
    [InlineData(14363, 0x00FF, 0x5800, 0xBB)]
    [InlineData(14364, 0x00FF, 0x4001, 0xCC)]
    [InlineData(14365, 0x00FF, 0x5801, 0xDD)]
    [InlineData(14590, 0x00FF, 0x4100, 0x11)]
    [InlineData(14591, 0x00FF, 0x5800, 0x22)]
    [InlineData(14592, 0x00FF, 0x4101, 0x33)]
    [InlineData(14593, 0x00FF, 0x5801, 0x44)]
    [InlineData(58030, 0x00FF, 0x57FE, 0x45)]
    [InlineData(58031, 0x00FF, 0x5AFE, 0x46)]
    [InlineData(58032, 0x00FF, 0x57FF, 0x47)]
    [InlineData(58033, 0x00FF, 0x5AFF, 0x48)]
    public void WhenReadingUnattachedPort_AndMatchingCycle_ShouldReturnFloatingBusValue(int ticks, Word port, Word address, byte expectedValue)
    {
        _clock.AddTicks(ticks);
        _memory.Write(address, expectedValue);

        var value = _floatingBus.ReadPort(port);

        value.ShouldBe(expectedValue);
    }

    [Theory]
    [InlineData(14366, 0x00FF, 0x4000, 0xAA)]
    [InlineData(14367, 0x00FF, 0x5800, 0xBB)]
    [InlineData(14368, 0x00FF, 0x4001, 0xCC)]
    [InlineData(14369, 0x00FF, 0x5801, 0xDD)]
    [InlineData(14594, 0x00FF, 0x4100, 0x11)]
    [InlineData(14595, 0x00FF, 0x5800, 0x22)]
    [InlineData(14596, 0x00FF, 0x4101, 0x33)]
    [InlineData(14597, 0x00FF, 0x5801, 0x44)]
    public void WhenReadingUnattachedPort_AndNotMatchingCycle_ShouldReturnNull(int ticks, Word port, Word address, byte expectedValue)
    {
        _clock.AddTicks(ticks);
        _memory.Write(address, expectedValue);

        var value = _floatingBus.ReadPort(port);

        value.ShouldBeNull();
    }
}