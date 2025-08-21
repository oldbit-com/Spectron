using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulation.Devices.Memory;

public delegate void MemoryUpdatedEvent(Word address);

public interface IEmulatorMemory : IMemory, IDevice
{
    void Reset() { }

    byte ReadScreen(Word address) => Read((Word)(address + 0x4000));

    void ShadowRom(IRomMemory? shadowRom);

    IRomMemory OriginalRom { get; }

    event MemoryUpdatedEvent? MemoryUpdated;
}