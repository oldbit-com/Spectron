using OldBit.Spectral.Emulator.Screen;
using OldBit.Z80Cpu;

namespace OldBit.Spectral.Emulator.Hardware;

/// <summary>
/// When the Z80 reads from an unattached port it will read the data present on the ULA bus,
/// which will be a display byte being transferred to the video circuits or 0xFF when idle,
/// such as periods spent building the border.
/// https://sinclair.wiki.zxnet.co.uk/wiki/Floating_bus
/// </summary>
internal class FloatingBus
{
    private readonly IMemory _memory;
    private readonly Dictionary<int, Word> _floatingBusAddreesIndex = new();

    internal FloatingBus(IMemory memory)
    {
        _memory = memory;
        foreach (var screenEvent in FastLookup.ScreenRenderEvents)
        {
            _floatingBusAddreesIndex[screenEvent.Ticks + 0] = screenEvent.BitmapAddress;
            _floatingBusAddreesIndex[screenEvent.Ticks + 1] = screenEvent.AttributeAddress;
            _floatingBusAddreesIndex[screenEvent.Ticks + 2] = (Word)(screenEvent.BitmapAddress + 1);
            _floatingBusAddreesIndex[screenEvent.Ticks + 3] = (Word)(screenEvent.AttributeAddress + 1);
        }
    }

    internal byte GetFloatingValue(int frameTicks)
    {
        if (frameTicks is < DefaultTimings.FirstPixelTick or > DefaultTimings.LastPixelTick)
        {
            return 0xFF;
        }

        return _floatingBusAddreesIndex.TryGetValue(frameTicks, out var address) ?
            _memory.Read((Word)(0x4000 + address)) :
            (byte)0xFF;
    }
}