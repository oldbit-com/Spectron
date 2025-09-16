using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Spectron.Emulation.Rom;

namespace OldBit.Spectron.Emulation.Devices.Interface1;

public sealed class ShadowRom : IRomMemory
{
    private readonly IEmulatorMemory _emulatorMemory;
    private Interface1RomVersion _version = Interface1RomVersion.V2;

    public Interface1RomVersion Version
    {
        get => _version;
        set
        {
            _version = value;

            Memory = RomReader.ReadRom(_version == Interface1RomVersion.V1 ?
                RomType.Interface1V1 :
                RomType.Interface1V2);
        }
    }

    internal ShadowRom(IEmulatorMemory emulatorMemory, Interface1RomVersion version)
    {
        _emulatorMemory = emulatorMemory;

        Version = version;
    }

    internal void Page() => _emulatorMemory.ShadowRom(this);

    internal void UnPage() => _emulatorMemory.ShadowRom(null);

    public byte Read(Word address) => address < 0x2000 ?
        Memory[address] : _emulatorMemory.OriginalRom.Read(address);

    public byte[] Memory { get; private set; } = [];
}