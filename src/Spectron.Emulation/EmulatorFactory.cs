using Microsoft.Extensions.Logging;
using OldBit.Spectron.Emulation.Devices.Joystick.Gamepad;
using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Z80Cpu.Contention;

namespace OldBit.Spectron.Emulation;

internal sealed record EmulatorArgs(
    ComputerType ComputerType,
    RomType RomType,
    IEmulatorMemory Memory,
    IContentionProvider ContentionProvider);

public sealed class EmulatorFactory(
    TimeMachine timeMachine,
    TapeManager tapeManager,
    GamepadManager gamepadManager,
    ILogger<EmulatorFactory> logger)
{
    public Emulator Create(ComputerType computerType, RomType romType, byte[]? customRom = null)
    {
        byte[] rom;

        switch (computerType)
        {
            case ComputerType.Spectrum16K:
                rom = customRom ?? GetSpectrum48KRom(romType);
                return CreateSpectrum(computerType, romType, new Memory16K(rom));

            case ComputerType.Spectrum48K:
                rom = customRom ?? GetSpectrum48KRom(romType);
                return CreateSpectrum(computerType, romType, new Memory48K(rom));

            case ComputerType.Spectrum128K:
                rom = customRom != null ? customRom[..0x4000] : GetSpectrum128KRom(romType);
                var bank1Rom = customRom != null ? customRom[0x4000..] : RomReader.ReadRom(RomType.Original128Bank1);
                return CreateSpectrum128K(romType, new Memory128K(rom, bank1Rom));

            case ComputerType.Timex2048:
                throw new NotImplementedException();

            default:
                throw new ArgumentOutOfRangeException(nameof(computerType));
        }
    }

    private Emulator CreateSpectrum128K(RomType romType, Memory128K memory)
    {
        var contentionProvider = new ContentionProvider(
            Hardware.Spectrum128K.FirstPixelTicks,
            Hardware.Spectrum128K.TicksPerLine);

        memory.BankPaged += bankId => contentionProvider.MemoryBankId = bankId;

        var emulatorSettings = new EmulatorArgs(
            ComputerType.Spectrum128K,
            romType,
            memory,
            contentionProvider);

        return new Emulator(emulatorSettings, Hardware.Spectrum128K, tapeManager, gamepadManager, timeMachine, logger);
    }

    private Emulator CreateSpectrum(ComputerType computerType, RomType romType, IEmulatorMemory memory)
    {
        var contentionProvider = new ContentionProvider(
            Hardware.Spectrum48K.FirstPixelTicks,
            Hardware.Spectrum48K.TicksPerLine);

        var emulatorSettings = new EmulatorArgs(
            computerType,
            romType,
            memory,
            contentionProvider);

        return new Emulator(emulatorSettings, Hardware.Spectrum48K, tapeManager, gamepadManager, timeMachine, logger);
    }

    private static byte[] GetSpectrum48KRom(RomType romType) =>
        RomReader.ReadRom(romType == RomType.Original ? RomType.Original48 : romType);

    private static byte[] GetSpectrum128KRom(RomType romType) =>
        RomReader.ReadRom(romType == RomType.Original ? RomType.Original128Bank0 : romType);
}