using OldBit.Spectral.Emulation.Devices.Audio;
using OldBit.Spectral.Emulation.Devices.Memory;
using OldBit.Spectral.Emulation.Rom;

namespace OldBit.Spectral.Emulation.Computers;

public static class EmulatorFactory
{
    public static Emulator Create(ComputerType computerType, RomType romType) => computerType switch
    {
        ComputerType.Spectrum16K => CreateSpectrum16K(romType),
        ComputerType.Spectrum48K => CreateSpectrum48K(romType),
        ComputerType.Spectrum128K => CreateSpectrum128K(),
        ComputerType.Timex2048 => throw new NotImplementedException(),
        _ => throw new ArgumentOutOfRangeException(nameof(computerType))
    };

    private static Emulator CreateSpectrum16K(RomType romType) =>
        CreateSpectrum16Or48K(new Memory16K(RomReader.ReadRom(romType)));

    private static Emulator CreateSpectrum48K(RomType romType) =>
        CreateSpectrum16Or48K(new Memory48K(RomReader.ReadRom(romType)));

    private static Emulator CreateSpectrum128K()
    {
        var memory = new Memory128K(
            RomReader.ReadRom(RomType.Original128Bank0),
            RomReader.ReadRom(RomType.Original128Bank1));

        var emulatorSettings = new EmulatorSettings(
            ComputerType.Spectrum128K,
            memory,
            new ContentionProvider(
                Hardware.Spectrum128K.FirstPixelTick,
                Hardware.Spectrum128K.TicksPerLine),
            new Beeper(Hardware.Spectrum128K.ClockMhz),
            UseAYSound: true);

        return new Emulator(emulatorSettings);
    }

    private static Emulator CreateSpectrum16Or48K(EmulatorMemory memory)
    {
        var emulatorSettings = new EmulatorSettings(
            ComputerType.Spectrum48K,
            memory,
            new ContentionProvider(
                Hardware.Spectrum48K.FirstPixelTick,
                Hardware.Spectrum48K.TicksPerLine),
            new Beeper(Hardware.Spectrum48K.ClockMhz),
            UseAYSound: false);

        return new Emulator(emulatorSettings);
    }


}