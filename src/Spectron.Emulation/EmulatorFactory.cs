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
    public static Emulator Create(ComputerType computerType, RomType romType, byte[]? customRom = null)
    {
        byte[] rom;

        switch (computerType)
        {
            case ComputerType.Spectrum16K:
                rom = customRom ?? GetSpectrum48KRom(romType);
                return CreateSpectrum16Or48K(RomType.Custom, new Memory16K(rom));

            case ComputerType.Spectrum48K:
                rom = customRom ?? GetSpectrum48KRom(romType);
                return CreateSpectrum16Or48K(RomType.Custom, new Memory48K(rom));

            case ComputerType.Spectrum128K:
                rom = customRom != null ? customRom[0..0x4000] : GetSpectrum128KRom(romType);
                var bank1Rom = customRom != null ? customRom[0x4000..] : RomReader.ReadRom(RomType.Original128Bank1);
                return CreateSpectrum128K(romType, new Memory128K(rom, bank1Rom));

            case ComputerType.Timex2048:
                throw new NotImplementedException();

            default:
                throw new ArgumentOutOfRangeException(nameof(computerType));
        }
    }

    private static Emulator CreateSpectrum128K(RomType romType, Memory128K memory)
    {
        var contentionProvider = new ContentionProvider(
            Hardware.Spectrum128K.FirstPixelTick,
            Hardware.Spectrum128K.TicksPerLine);

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

    private static byte[] GetSpectrum48KRom(RomType romType) =>
        RomReader.ReadRom(romType == RomType.Original ? RomType.Original48 : romType);

    private static byte[] GetSpectrum128KRom(RomType romType) =>
        RomReader.ReadRom(romType == RomType.Original ? RomType.Original128Bank0 : romType);
}