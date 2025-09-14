using OldBit.Spectron.Emulation.Devices;
using OldBit.Spectron.Emulation.Devices.Audio;
using OldBit.Spectron.Emulation.Devices.DivMmc;
using OldBit.Spectron.Emulation.Devices.Interface1;
using OldBit.Spectron.Emulation.Devices.Interface1.Microdrive;
using OldBit.Spectron.Emulation.Devices.Joystick;
using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Spectron.Emulation.Devices.Mouse;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Spectron.Emulation.State.Components;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Spectron.Files.Tzx;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulation.State;

public sealed class StateManager(EmulatorFactory emulatorFactory)
{
    public Emulator CreateEmulator(StateSnapshot snapshot)
    {
        var romType = snapshot.CustomRom?.RomType ?? RomType.Original;

        var emulator = emulatorFactory.Create(snapshot.ComputerType, romType, snapshot.CustomRom?.Concatenated);

        LoadCpu(emulator.Cpu, snapshot.Cpu);
        LoadMemory(emulator.Memory, snapshot.Memory);
        LoadUlaPlus(emulator.UlaPlus, snapshot.UlaPlus);
        LoadJoystick(emulator.JoystickManager, snapshot.Joystick);
        LoadMouse(emulator.MouseManager, snapshot.Mouse);
        LoadTape(emulator.TapeManager, snapshot.Tape);
        LoadAy(emulator.AudioManager, snapshot.Ay);
        LoadDivMmc(emulator.DivMmc, snapshot.DivMmc);
        LoadInterface1(emulator.Interface1, emulator.MicrodriveManager, snapshot.Interface1);
        LoadOther(emulator, snapshot);

        return emulator;
    }

    public static StateSnapshot CreateSnapshot(Emulator emulator, bool isTimeMachine = false)
    {
        var snapshot = new StateSnapshot
        {
            ComputerType = emulator.ComputerType
        };

        SaveCpu(emulator.Cpu, snapshot.Cpu);
        SaveMemory(emulator.Memory, snapshot.Memory);
        SaveUlaPlus(emulator.UlaPlus, snapshot);
        SaveJoystick(emulator.JoystickManager, snapshot.Joystick);
        SaveMouse(emulator.MouseManager, snapshot.Mouse);
        SaveTape(emulator.TapeManager, snapshot);
        SaveAy(emulator.AudioManager, snapshot);
        SaveDivMmc(emulator.DivMmc, snapshot);
        SaveInterface1(emulator.Interface1, emulator.MicrodriveManager, isTimeMachine, snapshot);
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
                memoryState.SetBank(memory16K.Ram.ToArray(), pageNumber: 0);

                break;

            case Memory48K memory48K:
                memoryState.SetBank(memory48K.Ram.ToArray(), pageNumber: 0);

                break;

            case Memory128K memory128K:
            {
                for (var i = 0; i < 8; i++)
                {
                    memoryState.SetBank(memory128K.Banks[i], pageNumber: i);
                }

                memoryState.PagingMode = memory128K.LastPagingModeValue;

                break;
            }
        }
    }

    private static void SaveUlaPlus(UlaPlus ulaPlus, StateSnapshot stateSnapshot)
    {
        if (!ulaPlus.IsEnabled)
        {
            return;
        }

        stateSnapshot.UlaPlus = new UlaPlusState
        {
            IsActive = ulaPlus.IsActive,
            IsEnabled = ulaPlus.IsEnabled,
            PaletteGroup = ulaPlus.PaletteGroup,
            PaletteColors = ulaPlus.PaletteColors,
        };
    }

    private static void SaveJoystick(JoystickManager joystickManager, JoystickState joystickState) =>
        joystickState.JoystickType = joystickManager.JoystickType;

    private static void SaveMouse(MouseManager mouseManager, MouseState mouseState) =>
        mouseState.MouseType = mouseManager.MouseType;

    private static void SaveTape(TapeManager tapeManager, StateSnapshot snapshot)
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

    private static void SaveAy(AudioManager audioManager, StateSnapshot stateSnapshot)
    {
        if (!audioManager.IsAySupported)
        {
            return;
        }

        stateSnapshot.Ay = new AyState
        {
            CurrentRegister = audioManager.Ay.CurrentRegister,
            Registers = audioManager.Ay.Registers,
        };
    }

    private static void SaveDivMmc(DivMmcDevice divMmc, StateSnapshot stateSnapshot)
    {
        if (!divMmc.IsEnabled)
        {
            return;
        }

        stateSnapshot.DivMmc = new DivMmcState
        {
            ControlRegister = divMmc.Memory.ControlRegister,
            Banks = divMmc.Memory.Banks,
        };
    }

    private static void SaveInterface1(Interface1Device interface1, MicrodriveManager microdriveManager,
        bool isTimeMachine, StateSnapshot stateSnapshot)
    {
        if (!interface1.IsEnabled)
        {
            return;
        }

        var microdrives = new List<MicrodriveState>();

        if (!isTimeMachine)
        {
            foreach (var (microdriveId, microdrive) in microdriveManager.Microdrives)
            {
                if (microdrive.IsCartridgeInserted)
                {
                    microdrives.Add(new MicrodriveState
                    {
                        MicrodriveId = microdriveId,
                        FilePath = microdrive.Cartridge!.FilePath,
                        Data = microdrive.Cartridge.GetData()
                    });
                }
            }
        }

        stateSnapshot.Interface1 = new Interface1State
        {
            RomVersion = interface1.ShadowRom.Version,
            Microdrives = microdrives.ToArray(),
        };
    }

    private static void SaveOther(Emulator emulator, StateSnapshot stateSnapshot) =>
        stateSnapshot.BorderColor = emulator.ScreenBuffer.LastBorderColor;

    private static void SaveCustomRom(Emulator emulator, StateSnapshot stateSnapshot)
    {
        stateSnapshot.CustomRom = emulator.Memory switch
        {
            Memory16K memory16K => new CustomRomState
            {
                Bank0 = memory16K.Rom.ToArray(),
                RomType = emulator.RomType,
            },
            Memory48K memory48K => new CustomRomState
            {
                Bank0 = memory48K.Rom.ToArray(),
                RomType = emulator.RomType,
            },
            Memory128K memory128K => new CustomRomState
            {
                Bank0 = memory128K.RomBank0.Memory,
                Bank1 = memory128K.RomBank1.Memory,
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
        switch (memory)
        {
            case Memory16K memory16K:
                memoryState.Banks[0].CopyTo(memory16K.Ram);

                break;

            case Memory48K memory48K:
                memoryState.Banks[0].CopyTo(memory48K.Ram);

                break;

            case Memory128K memory128:
                for (var bankNumber = 0; bankNumber < memoryState.Banks.Length; bankNumber++)
                {
                    Array.Copy(memoryState.Banks[bankNumber], memory128.Banks[bankNumber], memoryState.Banks[bankNumber].Length);
                }

                memory128.SetPagingMode(memoryState.PagingMode);

                break;
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

    private static void LoadMouse(MouseManager mouseManager, MouseState mouseState) =>
        mouseManager.SetupMouse(mouseState.MouseType);

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

    private static void LoadOther(Emulator emulator, StateSnapshot stateSnapshot)
    {
        emulator.ScreenBuffer.Reset();
        emulator.ScreenBuffer.UpdateBorder(stateSnapshot.BorderColor);
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

    private static void LoadDivMmc(DivMmcDevice divMmc, DivMmcState? divMmcState)
    {
        if (divMmcState == null)
        {
            return;
        }

        divMmc.Memory.Banks = divMmcState.Banks;
        divMmc.Memory.PagingControl(divMmcState.ControlRegister);
        divMmc.Enable();
    }

    private static void LoadInterface1(Interface1Device interface1, MicrodriveManager microdriveManager,
        Interface1State? interface1State)
    {
        if (interface1State == null)
        {
            return;
        }

        interface1.ShadowRom.Version = interface1State.RomVersion;

        foreach (var microdrive in interface1State.Microdrives)
        {
            microdriveManager.Microdrives[microdrive.MicrodriveId]
                .InsertCartridge(microdrive.FilePath, microdrive.Data);
        }

        interface1.Enable();
    }
}