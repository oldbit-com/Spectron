using OldBit.Spectral.Emulation.Devices.Audio;
using OldBit.Spectral.Emulation.Devices.Memory;
using OldBit.Spectral.Emulation.Rom;

namespace OldBit.Spectral.Emulation.Computers;

public static class EmulatorFactory
{
    public static Emulator Create(Computer computer, RomType romType) => computer switch
    {
        Computer.Spectrum16K => CreateSpectrum16K(romType),
        Computer.Spectrum48K => CreateSpectrum48K(romType),
        Computer.Spectrum128K => CreateSpectrum128K(),
        Computer.Timex2048 => throw new NotImplementedException(),
        _ => throw new ArgumentOutOfRangeException(nameof(computer))
    };

    private static Emulator CreateSpectrum16K(RomType romType) =>
        CreateSpectrum16Or48K(new Memory16K(RomReader.ReadRom(romType)));

    private static Emulator CreateSpectrum48K(RomType romType) =>
        CreateSpectrum16Or48K(new Memory48K(RomReader.ReadRom(romType)));

    private static Emulator CreateSpectrum128K()
    {
        // TODO: Proper timing and contention
        const float clockMHz = 3.5f;

        var memory = new Memory128K(
            RomReader.ReadRom(RomType.Original128Bank0),
            RomReader.ReadRom(RomType.Original128Bank1));

        var emulatorSettings = new EmulatorSettings(
            Computer.Spectrum128K,
            memory,
            new ContendedMemory(),
            new Beeper(clockMHz),
            UseAYSound: true);

        return new Emulator(emulatorSettings);
    }

    private static Emulator CreateSpectrum16Or48K(EmulatorMemory memory)
    {
        const float clockMHz = 3.5f;

        var emulatorSettings = new EmulatorSettings(
            Computer.Spectrum48K,
            memory,
            new ContendedMemory(),
            new Beeper(clockMHz),
            UseAYSound: false);

        return new Emulator(emulatorSettings);
    }


}