using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulation.Devices.Memory;

public delegate void MemoryUpdatedEvent(Word address, byte value);

public interface IEmulatorMemory : IMemory, IDevice
{
    void Reset() { }

    byte ReadActiveScreen(Word address, Word screenBaseAddress = 0x4000) => Read((Word)(address + screenBaseAddress));

    void ShadowRom(IRomMemory? shadowRom);

    IRomMemory OriginalRom { get; }

    RomBank RomBank => RomBank.Bank0;

    event MemoryUpdatedEvent? MemoryUpdated;
}