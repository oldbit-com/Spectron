using OldBit.Spectral.Emulation.Devices.Audio;
using OldBit.Spectral.Emulation.Devices.Memory;
using OldBit.Spectral.Emulation.Rom;

namespace OldBit.Spectral.Emulation.Computers;

public static class EmulatorFactory
{
    public static Emulator Create(EmulatedComputer computer, RomType romType) => computer switch
    {
        EmulatedComputer.Spectrum16K => CreateSpectrum16K(romType),
        EmulatedComputer.Spectrum48K => CreateSpectrum48K(romType),
        EmulatedComputer.Spectrum128K => CreateSpectrum128K(romType),
        _ => throw new ArgumentOutOfRangeException(nameof(computer))
    };

    private static Emulator CreateSpectrum16K(RomType romType) =>
        CreateSpectrum16Or48K(romType, new Memory16K(RomReader.ReadRom(romType)));

    private static Emulator CreateSpectrum48K(RomType romType) =>
        CreateSpectrum16Or48K(romType, new Memory48K(RomReader.ReadRom(romType)));

    private static Emulator CreateSpectrum16Or48K(RomType romType, EmulatorMemory memory)
    {
        const float clockMHz = 3.5f;

        var contentionProvider = new ContendedMemory();
        var beeper = new Beeper(clockMHz);

        return new Emulator(memory, beeper, contentionProvider);
    }

    private static Emulator CreateSpectrum128K(RomType romType)
    {
        throw new NotImplementedException();
    }
}