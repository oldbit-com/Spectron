using OldBit.Spectral.Emulation.Devices;
using OldBit.Spectral.Emulation.Devices.Audio;
using OldBit.Spectral.Emulation.Rom;

namespace OldBit.Spectral.Emulation.Computers;

public static class EmulatorFactory
{
    public static Emulator Create(EmulationMode mode, RomType romType) => mode switch
    {
        EmulationMode.Spectrum16K => CreateSpectrum16K(romType),
        EmulationMode.Spectrum48K => CreateSpectrum48K(romType),
        EmulationMode.Spectrum128K => CreateSpectrum128K(romType),
        _ => throw new ArgumentOutOfRangeException(nameof(mode))
    };

    private static Emulator CreateSpectrum16K(RomType romType) =>
        CreateSpectrum16Or48K(romType, new Memory16K(RomReader.ReadRom(romType)));

    private static Emulator CreateSpectrum48K(RomType romType) =>
        CreateSpectrum16Or48K(romType, new Memory48K(RomReader.ReadRom(romType)));

    private static Emulator CreateSpectrum16Or48K(RomType romType, Memory memory)
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