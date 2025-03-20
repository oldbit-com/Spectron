using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulation.Devices.Memory;

internal interface IRomMemory : IMemory
{
    byte[] Memory { get; }
}