using OldBit.Z80Cpu;

namespace OldBit.Spectral.Emulation.Devices.Memory;

public abstract class EmulatorMemory : IMemory
{
    public abstract byte Read(Word address);

    public abstract void Write(Word address, byte data);

    internal delegate void ScreenMemoryUpdatedEvent(Word address);
    internal event ScreenMemoryUpdatedEvent? ScreenMemoryUpdated;

    protected void OnScreenMemoryUpdated(Word address) => ScreenMemoryUpdated?.Invoke(address);

    internal virtual byte ReadScreen(Word address) => Read((Word)(address + 0x4000));
}