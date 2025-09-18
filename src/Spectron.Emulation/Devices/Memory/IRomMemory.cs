using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulation.Devices.Memory;

public interface IRomMemory : IMemory
{
    byte[] Memory { get; }

    void IMemory.Write(Word address, byte data) {}
}