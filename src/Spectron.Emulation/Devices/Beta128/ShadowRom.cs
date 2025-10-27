using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Spectron.Emulation.Rom;

namespace OldBit.Spectron.Emulation.Devices.Beta128;

public sealed class ShadowRom(IEmulatorMemory emulatorMemory) : IRomMemory
{
    public byte Read(Word address) => Memory[address];

    public byte[] Memory { get; } = RomReader.ReadRom(RomType.TrDos);

    internal void Page() => emulatorMemory.ShadowRom(this);

    internal void UnPage() => emulatorMemory.ShadowRom(null);
}