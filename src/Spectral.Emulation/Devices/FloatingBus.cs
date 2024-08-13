using OldBit.Spectral.Emulation.Screen;
using OldBit.Z80Cpu;

namespace OldBit.Spectral.Emulation.Devices;

/// <summary>
/// When the Z80 reads from an unattached port it will read the data present on the ULA bus,
/// which will be a display byte being transferred to the video circuits or 0xFF when idle,
/// such as periods spent building the border.
/// https://sinclair.wiki.zxnet.co.uk/wiki/Floating_bus
/// </summary>
internal sealed class FloatingBus : IDevice
{
    private readonly IMemory _memory;
    private readonly Clock _clock;
    private readonly Dictionary<int, Word> _floatingBusAddressIndex = new();

    internal FloatingBus(IMemory memory, Clock clock)
    {
        _memory = memory;
        _clock = clock;

        foreach (var screenEvent in FastLookup.ScreenRenderEvents)
        {
            _floatingBusAddressIndex[screenEvent.Ticks + 0] = screenEvent.BitmapAddress;
            _floatingBusAddressIndex[screenEvent.Ticks + 1] = screenEvent.AttributeAddress;
            _floatingBusAddressIndex[screenEvent.Ticks + 2] = (Word)(screenEvent.BitmapAddress + 1);
            _floatingBusAddressIndex[screenEvent.Ticks + 3] = (Word)(screenEvent.AttributeAddress + 1);
        }
    }

    public byte? ReadPort(Word address)
    {
        if (Ula.IsUlaPort(address))
        {
            return null;
        }

        if (_clock.FrameTicks is < DefaultTimings.FirstPixelTick or > DefaultTimings.LastPixelTick)
        {
            return null;
        }

        return _floatingBusAddressIndex.TryGetValue(_clock.FrameTicks, out var screenAddress) ?
            _memory.Read((Word)(0x4000 + screenAddress)) : null;
    }

    public int Priority => int.MaxValue;
}