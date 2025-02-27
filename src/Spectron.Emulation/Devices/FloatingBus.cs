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

        var screenRenderEvents = FastLookup.GetScreenRenderEvents(hardware);

        foreach (var screenEvent in screenRenderEvents)
        {
            _floatingBusAddressIndex[screenEvent.Ticks + 0] = screenEvent.BitmapAddress;
            _floatingBusAddressIndex[screenEvent.Ticks + 1] = screenEvent.AttributeAddress;
            _floatingBusAddressIndex[screenEvent.Ticks + 2] = (Word)(screenEvent.BitmapAddress + 1);
            _floatingBusAddressIndex[screenEvent.Ticks + 3] = (Word)(screenEvent.AttributeAddress + 1);
        }
    }

    public byte? ReadPort(Word address)
    {
        if (!IsEnabled || Ula.IsUlaPort(address))
        {
            return null;
        }

        if (_clock.CurrentFrameTicks < _hardware.FirstPixelTicks || _clock.CurrentFrameTicks > _hardware.LastPixelTicks)
        {
            return null;
        }

        return _floatingBusAddressIndex.TryGetValue(_clock.CurrentFrameTicks, out var screenAddress)
            ? _memory.Read((Word)(0x4000 + screenAddress))
            : null;
    }

    public int Priority => int.MaxValue;
}