using Microsoft.Extensions.Logging;
using OldBit.Spectron.Emulation.Commands;
using OldBit.Spectron.Emulation.Devices.Beta128;
using OldBit.Spectron.Emulation.Devices.Contention;
using OldBit.Spectron.Emulation.Devices.Gamepad;
using OldBit.Spectron.Emulation.Devices.Interface1.Microdrives;
using OldBit.Spectron.Emulation.Devices.Keyboard;
using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Spectron.Emulation.TimeTravel;
using OldBit.Z80Cpu.Contention;

namespace OldBit.Spectron.Emulation;

internal sealed record EmulatorArgs(
    ComputerType ComputerType,
    RomType RomType,
    IEmulatorMemory Memory,
    int ClockMultiplier,
    IContentionProvider ContentionProvider);

public sealed class EmulatorFactory(
    TimeMachine timeMachine,
    TapeManager tapeManager,
    MicrodriveManager microdriveManager,
    DiskDriveManager diskDriveManager,
    GamepadManager gamepadManager,
    KeyboardState keyboardState,
    CommandManager commandManager,
    ILogger<EmulatorFactory> logger)
{
    public Emulator Create(ComputerType computerType, RomType romType, int clockMultiplier = 1, byte[]? customRom = null)
    {
        byte[] rom;

        switch (computerType)
        {
            case ComputerType.Spectrum16K:
                rom = customRom ?? GetSpectrum48KRom(romType);
                return CreateSpectrum(computerType, romType, new Memory16K(rom), clockMultiplier);

            case ComputerType.Spectrum48K:
                rom = customRom ?? GetSpectrum48KRom(romType);
                return CreateSpectrum(computerType, romType, new Memory48K(rom), clockMultiplier);

            case ComputerType.Spectrum128K:
                rom = customRom != null ? customRom[..0x4000] : GetSpectrum128KRom(romType);
                var bank1Rom = customRom?.Length == 0x8000 ? customRom[0x4000..] : RomReader.ReadRom(RomType.Original128Bank1);
                return CreateSpectrum128K(romType, new Memory128K(rom, bank1Rom), clockMultiplier);

            case ComputerType.Timex2048:
                rom = customRom ?? GetTimex2048Rom(romType);
                return CreateTimex(romType, new Memory48K(rom), clockMultiplier);

            default:
                throw new ArgumentOutOfRangeException(nameof(computerType));
        }
    }

    private Emulator CreateSpectrum128K(RomType romType, Memory128K memory, int clockMultiplier)
    {
        var contentionProvider = new ContentionProvider128K(
            Hardware.Spectrum128K.ContentionStartTicks,
            Hardware.Spectrum128K.TicksPerLine);

        memory.RamBankPaged += bankId => contentionProvider.ActiveRamBankId = bankId;

        var emulatorSettings = new EmulatorArgs(
            ComputerType.Spectrum128K,
            romType,
            memory,
            clockMultiplier,
            contentionProvider);

        return new Emulator(
            emulatorSettings,
            Hardware.Spectrum128K,
            tapeManager,
            microdriveManager,
            diskDriveManager,
            gamepadManager,
            keyboardState,
            timeMachine,
            commandManager,
            logger);
    }

    private Emulator CreateSpectrum(ComputerType computerType, RomType romType, IEmulatorMemory memory, int clockMultiplier)
    {
        var contentionProvider = new ContentionProvider48K(
            Hardware.Spectrum48K.ContentionStartTicks,
            Hardware.Spectrum48K.TicksPerLine);

        var emulatorSettings = new EmulatorArgs(
            computerType,
            romType,
            memory,
            clockMultiplier,
            contentionProvider);

        return new Emulator(
            emulatorSettings,
            Hardware.Spectrum48K,
            tapeManager,
            microdriveManager,
            diskDriveManager,
            gamepadManager,
            keyboardState,
            timeMachine,
            commandManager,
            logger);
    }

    private Emulator CreateTimex(RomType romType, IEmulatorMemory memory, int clockMultiplier)
    {
        var contentionProvider = new ContentionProvider48K(
            Hardware.Timex2048.ContentionStartTicks,
            Hardware.Timex2048.TicksPerLine);

        var emulatorSettings = new EmulatorArgs(
            ComputerType.Timex2048,
            romType,
            memory,
            clockMultiplier,
            contentionProvider);

        return new Emulator(
            emulatorSettings,
            Hardware.Timex2048,
            tapeManager,
            microdriveManager,
            diskDriveManager,
            gamepadManager,
            keyboardState,
            timeMachine,
            commandManager,
            logger);
    }

    private static byte[] GetSpectrum48KRom(RomType romType) => romType switch
    {
        RomType.Original or RomType.Custom => RomReader.ReadRom(RomType.Original48),
        _ => RomReader.ReadRom(romType)
    };

    private static byte[] GetSpectrum128KRom(RomType romType) => romType switch
    {
        RomType.Pentagon128 => RomReader.ReadRom(RomType.Pentagon128),
        RomType.Original or RomType.Custom => RomReader.ReadRom(RomType.Original128Bank0),
        _ => RomReader.ReadRom(romType)
    };

    private static byte[] GetTimex2048Rom(RomType romType) => romType switch
    {
        RomType.Original or RomType.Custom => RomReader.ReadRom(RomType.Timex2048),
        _ => RomReader.ReadRom(romType)
    };
}