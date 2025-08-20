using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Spectron.Emulation.Rom;

namespace OldBit.Spectron.Emulation.Devices.Interface1;

internal sealed class ShadowRom : IRomMemory
{
    private readonly IEmulatorMemory _emulatorMemory;
    private RomVersion _romVersion = RomVersion.V2;

    internal RomVersion RomVersion
    {
        get => _romVersion;
        set
        {
            _romVersion = value;

            Memory = RomReader.ReadRom(_romVersion == RomVersion.V1 ?
                RomType.Interface1V1 :
                RomType.Interface1V2);
        }
    }

    internal ShadowRom(IEmulatorMemory emulatorMemory, RomVersion romVersion)
    {
        _emulatorMemory = emulatorMemory;

        RomVersion = romVersion;
    }

    internal void Page() => _emulatorMemory.ShadowRom(this);

    internal void Unpage() => _emulatorMemory.ShadowRom(null);

    public byte Read(Word address)
    {
        throw new NotImplementedException();
    }

    public byte[] Memory { get; private set; } = [];
}