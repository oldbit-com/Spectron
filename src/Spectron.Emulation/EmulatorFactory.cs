using OldBit.Spectron.Emulation.Devices.Audio;
using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Z80Cpu.Contention;

namespace OldBit.Spectron.Emulation;

internal sealed record EmulatorSettings(
    ComputerType ComputerType,
    RomType RomType,
    IEmulatorMemory Memory,
    IContentionProvider ContentionProvider,
    Beeper Beeper,
    bool UseAYSound);

public static class EmulatorFactory
{
    public static Emulator Create(ComputerType computerType, RomType romType) => computerType switch
    {
        ComputerType.Spectrum16K => CreateSpectrum16K(romType),
        ComputerType.Spectrum48K => CreateSpectrum48K(romType),
        ComputerType.Spectrum128K => CreateSpectrum128K(romType),
        ComputerType.Timex2048 => throw new NotImplementedException(),
        _ => throw new ArgumentOutOfRangeException(nameof(computerType))
    };

    private static Emulator CreateSpectrum16K(RomType romType) =>
        CreateSpectrum16Or48K(
            romType,
            new Memory16K(RomReader.ReadRom(romType == RomType.Original ? RomType.Original48 : romType)));

    private static Emulator CreateSpectrum48K(RomType romType) =>
        CreateSpectrum16Or48K(
            romType,
            new Memory48K(RomReader.ReadRom(romType == RomType.Original ? RomType.Original48 : romType)));

    private static Emulator CreateSpectrum128K(RomType romType)
    {
        var contentionProvider = new ContentionProvider(
            Hardware.Spectrum128K.FirstPixelTick,
            Hardware.Spectrum128K.TicksPerLine);

        var romBank0 = RomReader.ReadRom(romType == RomType.Original ? RomType.Original128Bank0 : romType);

        var memory = new Memory128K(
            romBank0,
            RomReader.ReadRom(RomType.Original128Bank1));

        memory.BankPaged += bankId => contentionProvider.MemoryBankId = bankId;

        var emulatorSettings = new EmulatorSettings(
            ComputerType.Spectrum128K,
            romType,
            memory,
            contentionProvider,
            new Beeper(Hardware.Spectrum128K.ClockMhz),
            UseAYSound: true);

        return new Emulator(emulatorSettings, Hardware.Spectrum128K);
    }

    private static Emulator CreateSpectrum16Or48K(RomType romType, IEmulatorMemory memory)
    {
        var contentionProvider = new ContentionProvider(
            Hardware.Spectrum48K.FirstPixelTick,
            Hardware.Spectrum48K.TicksPerLine);

        var emulatorSettings = new EmulatorSettings(
            ComputerType.Spectrum48K,
            romType,
            memory,
            contentionProvider,
            new Beeper(Hardware.Spectrum48K.ClockMhz),
            UseAYSound: false);

        return new Emulator(emulatorSettings, Hardware.Spectrum48K);
    }
}