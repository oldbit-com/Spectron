using OldBit.Spectron.Emulation.Screen;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulation.Devices;

/// <summary>
/// When the Z80 reads from an unattached port it will read the data present on the ULA bus,
/// which will be a display byte being transferred to the video circuits or 0xFF when idle,
/// such as periods spent building the border.
/// https://sinclair.wiki.zxnet.co.uk/wiki/Floating_bus
/// </summary>
internal sealed class FloatingBus : IDevice
{
    private readonly HardwareSettings _hardware;
    private readonly IMemory _memory;
    private readonly Clock _clock;
    private readonly Dictionary<int, Word> _floatingBusAddressIndex = new();

    public bool IsEnabled { get; set; } = true;

    internal FloatingBus(HardwareSettings hardware, IMemory memory, Clock clock)
    {
        _hardware = hardware;
        _memory = memory;
        _clock = clock;

        BuildFloatingBusTable();
    }

    public byte? ReadPort(Word address)
    {
        if (!IsEnabled || Ula.IsUlaPort(address))
        {
            return null;
        }

        var ticksPostIn = _clock.CurrentFrameTicks + 3;

        if (ticksPostIn < _hardware.FloatingBusStartTicks || ticksPostIn > _hardware.LastPixelTicks)
        {
            return null;
        }

        return _floatingBusAddressIndex.TryGetValue(ticksPostIn, out var screenAddress)
            ? _memory.Read(screenAddress)
            : null;
    }

    public int Priority => int.MaxValue;

    private void BuildFloatingBusTable()
    {
        for (var y = 0; y < ScreenSize.ContentHeight; y++)
        {
            var startTicks = _hardware.FloatingBusStartTicks + y * _hardware.TicksPerLine;

            for (var x = 0; x < 16; x++)
            {
                var bitmapAddress = ScreenAddress.Calculate(2 * x, y);
                var attributeAddress = ScreenAddress.CalculateAttribute(2 * x, y);

                _floatingBusAddressIndex[startTicks + 0] = bitmapAddress;
                _floatingBusAddressIndex[startTicks + 1] = attributeAddress;
                _floatingBusAddressIndex[startTicks + 2] = (Word)(bitmapAddress + 1);
                _floatingBusAddressIndex[startTicks + 3] = (Word)(attributeAddress + 1);

                startTicks += 8;
            }
        }
    }
}