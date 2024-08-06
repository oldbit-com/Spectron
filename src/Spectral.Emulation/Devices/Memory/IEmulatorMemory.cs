using OldBit.Z80Cpu;

namespace OldBit.Spectral.Emulation.Devices.Memory;

public delegate void ScreenMemoryUpdatedEvent(Word address);

internal interface IEmulatorMemory : IMemory, IDevice
{
    void Reset() { }

    byte ReadScreen(Word address) => Read((Word)(address + 0x4000));

    event ScreenMemoryUpdatedEvent? ScreenMemoryUpdated;
}