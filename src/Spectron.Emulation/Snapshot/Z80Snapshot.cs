using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Spectron.Emulation.Screen;
using OldBit.Z80Cpu;
using OldBit.Z80Cpu.Registers;
using OldBit.ZX.Files.Z80;
using OldBit.ZX.Files.Z80.Types;

namespace OldBit.Spectron.Emulation.Snapshot;

public sealed class Z80Snapshot(EmulatorFactory emulatorFactory)
{
    internal Emulator Load(Stream stream)
    {
        var snapshot = Z80File.Load(stream);

        return CreateEmulator(snapshot);
    }

    internal static void Save(string fileName, Emulator emulator)
    {
        var snapshot = new Z80File();

        // TODO: Populate snapshot with emulator state

        snapshot.Save(fileName);
    }

    private Emulator CreateEmulator(Z80File snapshot)
    {
        var emulator = (snapshot.Header.HardwareMode, snapshot.Header.Flags3?.ModifyHardware) switch
        {
            (HardwareMode.Spectrum48, false) => emulatorFactory.Create(ComputerType.Spectrum48K, RomType.Original),
            (HardwareMode.Spectrum48, true) => emulatorFactory.Create(ComputerType.Spectrum16K, RomType.Original),
            (HardwareMode.Spectrum128, false) => emulatorFactory.Create(ComputerType.Spectrum128K, RomType.Original),
            _ => throw new NotSupportedException($"Snapshot hardware mode not supported: {snapshot.Header.HardwareMode}")
        };

        var (cpu, memory, screenBuffer) = (emulator.Cpu, emulator.Memory, emulator.ScreenBuffer);

        cpu.Registers.A = snapshot.Header.A;
        cpu.Registers.F = (Flags)snapshot.Header.F;
        cpu.Registers.BC = snapshot.Header.BC;
        cpu.Registers.DE = snapshot.Header.DE;
        cpu.Registers.HL = snapshot.Header.HL;

        cpu.Registers.Prime.A = snapshot.Header.APrime;
        cpu.Registers.Prime.F = (Flags)snapshot.Header.FPrime;
        cpu.Registers.Prime.BC = snapshot.Header.BCPrime;
        cpu.Registers.Prime.DE = snapshot.Header.DEPrime;
        cpu.Registers.Prime.HL = snapshot.Header.HLPrime;

        cpu.Registers.IX = snapshot.Header.IX;
        cpu.Registers.IY = snapshot.Header.IY;
        cpu.Registers.SP = snapshot.Header.SP;

        cpu.Registers.I = snapshot.Header.I;
        cpu.Registers.R = snapshot.Header.R;
        cpu.IM = (InterruptMode)snapshot.Header.Flags2.InterruptMode;
        cpu.IFF1 = (snapshot.Header.IFF1 & 0x01) != 0;
        cpu.IFF2 = (snapshot.Header.IFF2 & 0x01) != 0;

        cpu.Registers.PC = snapshot.Header.PC;
        cpu.Registers.SP = snapshot.Header.SP;

        LoadMemory(emulator.ComputerType, snapshot, memory);
        SetupJoystick(snapshot, emulator);

        screenBuffer.Reset();
        var borderColor = SpectrumPalette.GetBorderColor(snapshot.Header.Flags1.BorderColor);;
        screenBuffer.UpdateBorder(borderColor);

        return emulator;
    }

    private static void LoadMemory(ComputerType computerType, Z80File snapshot, IMemory memory)
    {
        if (snapshot.Header.Version == 1)
        {
            for (var i = 16384; i < 65536; i++)
            {
                memory.Write((Word)i, snapshot.Memory[i]);
            }
        }
        else
        {
            switch (computerType)
            {
                case ComputerType.Spectrum16K:
                case ComputerType.Spectrum48K:
                    LoadMemory48K(snapshot, memory);
                    break;

                case ComputerType.Spectrum128K:
                    LoadMemory128K(snapshot, memory);
                    break;
            }
        }
    }

    private static void LoadMemory48K(Z80File snapshot, IMemory memory)
    {
        foreach (var block in snapshot.MemoryBlocks)
        {
            var address = block.PageNumber switch
            {
                4 => 0x8000,
                5 => 0xC000,
                8 => 0x4000,
                _ => -1,
            };

            if (address == -1)
            {
                continue;
            }

            for (var i = 0; i < block.Data.Length; i++)
            {
                memory.Write((Word)(address + i), block.Data[i]);
            }
        }
    }

    private static void LoadMemory128K(Z80File snapshot, IMemory memory)
    {
        var memory128 = (Memory128K)memory;

        foreach (var block in snapshot.MemoryBlocks)
        {
            var bankNumber = block.PageNumber - 3;

            if (bankNumber is < 0 or >= 8)
            {
                continue;
            }

            var bank = memory128.Banks[bankNumber];

            for (var i = 0; i < block.Data.Length; i++)
            {
                bank[i] = block.Data[i];
            }
        }

        memory128.SetPagingMode(snapshot.Header[35]);
    }

    private static void SetupJoystick(Z80File snapshot, Emulator emulator)
    {
        switch (snapshot.Header.Flags2.JoystickType)
        {
            case JoystickType.Cursor:
                emulator.JoystickManager.SetupJoystick(Devices.Joystick.JoystickType.Cursor);
                break;

            case JoystickType.Kempston:
                emulator.JoystickManager.SetupJoystick(Devices.Joystick.JoystickType.Kempston);
                break;

            case JoystickType.Sinclair1:
                emulator.JoystickManager.SetupJoystick(Devices.Joystick.JoystickType.Sinclair1);
                break;

            case JoystickType.Sinclair2:
                emulator.JoystickManager.SetupJoystick(Devices.Joystick.JoystickType.Sinclair2);
                break;

            default:
                emulator.JoystickManager.SetupJoystick(Devices.Joystick.JoystickType.None);
                break;
        }
    }
}