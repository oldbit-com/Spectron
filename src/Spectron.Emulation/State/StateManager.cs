using OldBit.Spectron.Emulation.Devices;
using OldBit.Spectron.Emulation.Devices.Audio;
using OldBit.Spectron.Emulation.Devices.Joystick;
using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Spectron.Emulation.State.Components;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Spectron.Files.Tzx;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulation.State;

public sealed class StateManager(EmulatorFactory emulatorFactory)
{
    public Emulator CreateEmulator(EmulatorState snapshot)
    {
        var romType = snapshot.CustomRom?.RomType ?? RomType.Original;

        var emulator = emulatorFactory.Create(snapshot.ComputerType, romType, snapshot.CustomRom?.Concatenated);

        LoadCpu(emulator.Cpu, snapshot.Cpu);
        LoadMemory(emulator.Memory, snapshot.Memory);
        LoadUlaPlus(emulator.UlaPlus, snapshot.UlaPlus);
        LoadJoystick(emulator.JoystickManager, snapshot.Joystick);
        LoadTape(emulator.TapeManager, snapshot.Tape);
        LoadAy(emulator.AudioManager, snapshot.Ay);
        LoadOther(emulator, snapshot);

        return emulator;
    }

    public static EmulatorState CreateSnapshot(Emulator emulator)
    {
        var snapshot = new EmulatorState
        {
            ComputerType = emulator.ComputerType
        };

        SaveCpu(emulator.Cpu, snapshot.Cpu);
        SaveMemory(emulator.Memory, snapshot.Memory);
        SaveUlaPlus(emulator.UlaPlus, snapshot);
        SaveJoystick(emulator.JoystickManager, snapshot.Joystick);
        SaveTape(emulator.TapeManager, snapshot);
        SaveAy(emulator.AudioManager, snapshot);
        SaveOther(emulator, snapshot);

        if (emulator.RomType.IsCustomRom())
        {
            SaveCustomRom(emulator, snapshot);
        }

        return snapshot;
    }

    private static void SaveCpu(Z80 cpu, CpuState cpuState)
    {
        cpuState.AF = cpu.Registers.AF;
        cpuState.BC = cpu.Registers.BC;
        cpuState.DE = cpu.Registers.DE;
        cpuState.HL = cpu.Registers.HL;
        cpuState.IX = cpu.Registers.IX;
        cpuState.IY = cpu.Registers.IY;
        cpuState.PC = cpu.Registers.PC;
        cpuState.SP = cpu.Registers.SP;

        cpuState.I = cpu.Registers.I;
        cpuState.R = cpu.Registers.R;

        cpuState.AFPrime = cpu.Registers.Prime.AF;
        cpuState.BCPrime = cpu.Registers.Prime.BC;
        cpuState.DEPrime = cpu.Registers.Prime.DE;
        cpuState.HLPrime = cpu.Registers.Prime.HL;

        cpuState.IM = cpu.IM;
        cpuState.IFF1 = cpu.IFF1;
        cpuState.IFF2 = cpu.IFF2;
        cpuState.IsHalted = cpu.IsHalted;
    }

    private static void SaveMemory(IMemory memory, MemoryState memoryState)
    {
        switch (memory)
        {
            case Memory16K memory16K:
                memoryState.SetBank(memory16K.Memory[0x4000..0x8000], pageNumber: 5);
                break;

            case Memory48K memory48K:
                memoryState.SetBank(memory48K.Memory[0x4000..0x8000], pageNumber: 5);
                memoryState.SetBank(memory48K.Memory[0x8000..0xC000], pageNumber: 2);
                memoryState.SetBank(memory48K.Memory[0xC000..0x10000], pageNumber: 0);
                break;

            case Memory128K memory128K:
            {
                for (byte i = 0; i < 8; i++)
                {
                    memoryState.SetBank(memory128K.Banks[i][..0x4000], pageNumber: i);
                }

                memoryState.PagingMode = memory128K.LastPagingModeValue;
                break;
            }
        }
    }

    private static void SaveUlaPlus(UlaPlus ulaPlus, EmulatorState state)
    {
        if (!ulaPlus.IsActive)
        {
            return;
        }

        state.UlaPlus = new UlaPlusState
        {
            IsActive = ulaPlus.IsActive,
            IsEnabled = ulaPlus.IsEnabled,
            PaletteGroup = ulaPlus.PaletteGroup,
            PaletteColors = ulaPlus.PaletteColors,
        };
    }

    private static void SaveJoystick(JoystickManager joystickManager, JoystickState joystickState) =>
        joystickState.JoystickType = joystickManager.JoystickType;

    private static void SaveTape(TapeManager tapeManager, EmulatorState snapshot)
    {
        if (tapeManager.Cassette.IsEmpty)
        {
            return;
        }

        snapshot.Tape = new TapeState
        {
            CurrentBlockNo = tapeManager.Cassette.Position,
            TzxData = tapeManager.Cassette.ContentBytes,
        };
    }

    private static void SaveAy(AudioManager audioManager, EmulatorState state)
    {
        if (!audioManager.IsAySupported)
        {
            return;
        }

        state.Ay = new AyState
        {
            CurrentRegister = audioManager.Ay.CurrentRegister,
            Registers = audioManager.Ay.Registers,
        };
    }

    private static void SaveOther(Emulator emulator, EmulatorState state) =>
        state.BorderColor = emulator.ScreenBuffer.LastBorderColor;

    private static void SaveCustomRom(Emulator emulator, EmulatorState state)
    {
        state.CustomRom = emulator.Memory switch
        {
            Memory16K memory16K => new CustomRomState
            {
                Bank0 = memory16K.Memory[..0x4000],
                RomType = emulator.RomType,
            },
            Memory48K memory48K => new CustomRomState
            {
                Bank0 = memory48K.Memory[..0x4000],
                RomType = emulator.RomType,
            },
            Memory128K memory128K => new CustomRomState
            {
                Bank0 = memory128K.RomBank0,
                Bank1 = memory128K.RomBank1,
                RomType = emulator.RomType,
            },
            _ => null
        };
    }

    private static void LoadCpu(Z80 cpu, CpuState cpuState)
    {
        cpu.Registers.AF = cpuState.AF;
        cpu.Registers.BC = cpuState.BC;
        cpu.Registers.DE = cpuState.DE;
        cpu.Registers.HL = cpuState.HL;
        cpu.Registers.IX = cpuState.IX;
        cpu.Registers.IY = cpuState.IY;
        cpu.Registers.PC = cpuState.PC;
        cpu.Registers.SP = cpuState.SP;

        cpu.Registers.I = cpuState.I;
        cpu.Registers.R = cpuState.R;

        cpu.Registers.Prime.AF = cpuState.AFPrime;
        cpu.Registers.Prime.BC = cpuState.BCPrime;
        cpu.Registers.Prime.DE = cpuState.DEPrime;
        cpu.Registers.Prime.HL = cpuState.HLPrime;

        cpu.IM = cpuState.IM;
        cpu.IFF1 = cpuState.IFF1;
        cpu.IFF2 = cpuState.IFF2;
        cpu.IsHalted = cpuState.IsHalted;
    }

    private static void LoadMemory(IMemory memory, MemoryState memoryState)
    {
        for (var bankNumber = 0; bankNumber < memoryState.Banks.Length; bankNumber++)
        {
            var bank = memoryState.Banks[bankNumber];

            switch (memory)
            {
                case Memory16K or Memory48K:
                {
                    var address = bankNumber switch
                    {
                        5 => 0x4000,
                        2 => 0x8000,
                        0 => 0xC000,
                        _ => -1,
                    };


                    for (var i = 0; i < bank.Length; i++)
                    {
                        memory.Write((Word)(address + i), bank[i]);
                    }

                    break;
                }
                case Memory128K memory128:
                    Array.Copy(bank, memory128.Banks[bankNumber], bank.Length);

                    break;

                default:
                    throw new NotSupportedException($"Memory type not supported: {memory.GetType()}");
            }
        }

        if (memory is Memory128K mem128)
        {
            mem128.SetPagingMode(memoryState.PagingMode);
        }
    }

    private static void LoadUlaPlus(UlaPlus ulaPlus, UlaPlusState? ulaPlusState)
    {
        if (ulaPlusState == null)
        {
            return;
        }

        ulaPlus.IsActive = ulaPlusState.IsActive;
        ulaPlus.IsEnabled = ulaPlusState.IsEnabled;
        ulaPlus.PaletteGroup = ulaPlusState.PaletteGroup;
        ulaPlus.PaletteColors = ulaPlusState.PaletteColors;
    }

    private static void LoadJoystick(JoystickManager joystickManager, JoystickState joystickState) =>
        joystickManager.SetupJoystick(joystickState.JoystickType);

    private static void LoadTape(TapeManager tapeManager, TapeState? tapeState)
    {
        if (tapeState == null)
        {
            tapeManager.EjectTape();
            return;
        }

        using var data = new MemoryStream(tapeState.TzxData);
        var tzx = TzxFile.Load(data);

        tapeManager.InsertTape(tzx, tapeState.CurrentBlockNo);
    }

    private static void LoadOther(Emulator emulator, EmulatorState state)
    {
        emulator.ScreenBuffer.Reset();
        emulator.ScreenBuffer.UpdateBorder(state.BorderColor);
    }

    private static void LoadAy(AudioManager audioManager, AyState? ayState)
    {
        if (ayState == null)
        {
            return;
        }

        audioManager.IsAyEnabled = true;
        audioManager.IsBeeperEnabled = true;

        audioManager.Ay.LoadRegisters(ayState.CurrentRegister, ayState.Registers);
    }
}