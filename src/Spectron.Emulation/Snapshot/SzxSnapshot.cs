using System.IO.Compression;
using OldBit.Spectron.Emulation.Devices;
using OldBit.Spectron.Emulation.Devices.Audio;
using OldBit.Spectron.Emulation.Devices.Joystick;
using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Spectron.Emulation.Screen;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Z80Cpu;
using OldBit.ZX.Files.Extensions;
using OldBit.ZX.Files.Szx;
using OldBit.ZX.Files.Szx.Blocks;
using OldBit.ZX.Files.Tap;
using OldBit.ZX.Files.Tzx;
using JoystickType = OldBit.Spectron.Emulation.Devices.Joystick.JoystickType;

namespace OldBit.Spectron.Emulation.Snapshot;

public sealed class SzxSnapshot(EmulatorFactory emulatorFactory)
{
    internal Emulator Load(Stream stream)
    {
        var snapshot = SzxFile.Load(stream);

        return CreateEmulator(snapshot);
    }

    internal Emulator CreateEmulator(SzxFile snapshot)
    {
        var computerType = snapshot.Header.MachineId switch
        {
            SzxHeader.MachineId16K => ComputerType.Spectrum16K,
            SzxHeader.MachineId48K => ComputerType.Spectrum48K,
            SzxHeader.MachineId128K => ComputerType.Spectrum128K,
            _ => throw new NotSupportedException($"Snapshot hardware mode not supported: {snapshot.Header.MachineId}")
        };

        var romType = snapshot.CustomRom?.Data != null ? RomType.Custom : RomType.Original;

        var emulator = emulatorFactory.Create(computerType, romType, snapshot.CustomRom?.Data);

        LoadRegisters(emulator.Cpu, snapshot.Z80Registers);
        LoadMemory(emulator.Memory, snapshot.RamPages, snapshot.SpecRegs);
        LoadSpectrumRegisters(emulator.ScreenBuffer, snapshot.SpecRegs);
        LoadUlaPlus(emulator.UlaPlus, snapshot.Palette);
        LoadJoystick(emulator.JoystickManager, snapshot.Joystick);
        LoadTape(emulator.TapeManager, snapshot.Tape);
        LoadAyRegisters(emulator.AudioManager, snapshot.Ay);

        // TODO: Load the rest of the snapshot

        return emulator;
    }

    internal static void Save(string fileName, Emulator emulator)
    {
        var snapshot = CreateSnapshot(emulator);

        snapshot.Save(fileName);
    }

    public static SzxFile CreateSnapshot(Emulator emulator, CompressionLevel compressionLevel = CompressionLevel.SmallestSize)
    {
        var snapshot = new SzxFile
        {
            Creator = new CreatorBlock()
            {
                Name = "Spectron",
                MinorVersion = 1,
                MajorVersion = 0,
            }
        };

        snapshot.Header.MachineId = emulator.ComputerType switch
        {
            ComputerType.Spectrum16K => SzxHeader.MachineId16K,
            ComputerType.Spectrum48K => SzxHeader.MachineId48K,
            ComputerType.Spectrum128K => SzxHeader.MachineId128K,
            _ => snapshot.Header.MachineId
        };

        SaveRegisters(emulator.Cpu, snapshot.Z80Registers);
        SaveMemory(emulator.Memory, snapshot.RamPages, snapshot.SpecRegs, compressionLevel);
        SaveSpectrumRegisters(emulator.ScreenBuffer, snapshot.SpecRegs);
        SaveUlaPlus(emulator.UlaPlus, snapshot);
        SaveJoystick(emulator.JoystickManager, snapshot);
        SaveTape(emulator.TapeManager, snapshot, compressionLevel);
        SaveAyRegisters(emulator.AudioManager, snapshot);

        if (emulator.RomType.IsCustomRom())
        {
            SaveCustomRom(emulator.Memory, snapshot, compressionLevel);
        }

        return snapshot;
    }

    private static void LoadRegisters(Z80 cpu, Z80RegsBlock registers)
    {
        cpu.Registers.AF = registers.AF;
        cpu.Registers.BC = registers.BC;
        cpu.Registers.DE = registers.DE;
        cpu.Registers.HL = registers.HL;

        cpu.Registers.Prime.AF = registers.AF1;
        cpu.Registers.Prime.BC = registers.BC1;
        cpu.Registers.Prime.DE = registers.DE1;
        cpu.Registers.Prime.HL = registers.HL1;

        cpu.Registers.IX = registers.IX;
        cpu.Registers.IY = registers.IY;
        cpu.Registers.SP = registers.SP;
        cpu.Registers.PC = registers.PC;

        cpu.Registers.I = registers.I;
        cpu.Registers.R = registers.R;
        cpu.IM = (InterruptMode)registers.IM;
        cpu.IFF1 =registers.IFF1 == 1;
        cpu.IFF2 = registers.IFF2 == 1;
        cpu.IsHalted = registers.Flags == Z80RegsBlock.FlagsHalted;

        // TODO: Restart cycle counter at the given value
        // registers.CyclesStart;
        // registers.HoldIntReqCycles;
    }

    private static void LoadMemory(IMemory memory, List<RamPageBlock> ramPages, SpecRegsBlock specRegs)
    {
        foreach (var ramPage in ramPages)
        {
            switch (memory)
            {
                case Memory16K or Memory48K:
                {
                    var address = ramPage.PageNumber switch
                    {
                        5 => 0x4000,
                        2 => 0x8000,
                        0 => 0xC000,
                        _ => -1,
                    };

                    for (var i = 0; i < ramPage.Data.Length; i++)
                    {
                        memory.Write((Word)(address + i), ramPage.Data[i]);
                    }

                    break;
                }
                case Memory128K memory128:
                    var bank = memory128.Banks[ramPage.PageNumber];
                    for (var i = 0; i < ramPage.Data.Length; i++)
                    {
                        bank[i] = ramPage.Data[i];
                    }

                    break;

                default:
                    throw new NotSupportedException($"Memory type not supported: {memory.GetType()}");
            }
        }

        if (memory is Memory128K mem128)
        {
            mem128.SetPagingMode(specRegs.Port7FFD);
        }
    }

    private static void LoadSpectrumRegisters(ScreenBuffer screenBuffer, SpecRegsBlock specRegs)
    {
        var borderColor = SpectrumPalette.GetBorderColor(specRegs.Border);

        screenBuffer.Reset();
        screenBuffer.UpdateBorder(borderColor);
    }

    private static void LoadUlaPlus(UlaPlus ulaPlus, PaletteBlock? palette)
    {
        if (palette == null)
        {
            return;
        }

        ulaPlus.IsActive = palette.Flags == PaletteBlock.FlagsPaletteEnabled;
        ulaPlus.IsEnabled = true;

        ulaPlus.PaletteGroup = palette.CurrentRegister;
        ulaPlus.SetPaletteData(palette.Registers);
    }

    private static void LoadJoystick(JoystickManager joystickManager, JoystickBlock? joystickBlock)
    {
        if (joystickBlock == null)
        {
            return;
        }

        joystickManager.SetupJoystick(joystickBlock.JoystickTypePlayer1 switch
        {
            JoystickBlock.JoystickKempston => JoystickType.Kempston,
            JoystickBlock.JoystickSinclair1 => JoystickType.Sinclair1,
            JoystickBlock.JoystickSinclair2 => JoystickType.Sinclair2,
            JoystickBlock.JoystickCursor => JoystickType.Cursor,
            JoystickBlock.JoystickFuller => JoystickType.Fuller,
            _ => JoystickType.None,
        });
    }

    private void LoadTape(TapeManager tapeManager, TapeBlock? tapeBlock)
    {
        if (tapeBlock == null)
        {
            tapeManager.EjectTape();
            return;
        }

        using var data = new MemoryStream(tapeBlock.Data);
        if (tapeBlock.FileExtension.EndsWith("tap", StringComparison.OrdinalIgnoreCase))
        {
            var tapFile = TapFile.Load(data);
            tapeManager.InsertTape(tapFile.ToTzx(), tapeBlock.CurrentBlockNo);
        }
        else if (tapeBlock.FileExtension.EndsWith("tzx", StringComparison.OrdinalIgnoreCase))
        {
            var tzx = TzxFile.Load(data);
            tapeManager.InsertTape(tzx, tapeBlock.CurrentBlockNo);
        }
    }

    private void LoadAyRegisters(AudioManager audioManager, AyBlock? ay)
    {
        if (ay == null  || !audioManager.IsAySupported)
        {
            return;
        }

        audioManager.Ay.LoadRegisters(ay.CurrentRegister, ay.Registers);
    }

    private static void SaveRegisters(Z80 cpu, Z80RegsBlock registers)
    {
        registers.AF = cpu.Registers.AF;
        registers.BC = cpu.Registers.BC;
        registers.DE = cpu.Registers.DE;
        registers.HL = cpu.Registers.HL;

        registers.AF1 = cpu.Registers.Prime.AF;
        registers.BC1 = cpu.Registers.Prime.BC;
        registers.DE1 = cpu.Registers.Prime.DE;
        registers.HL1 = cpu.Registers.Prime.HL;

        registers.IX = cpu.Registers.IX;
        registers.IY = cpu.Registers.IY;
        registers.SP = cpu.Registers.SP;
        registers.PC = cpu.Registers.PC;

        registers.I = cpu.Registers.I;
        registers.R = cpu.Registers.R;
        registers.IM = (byte)cpu.IM;
        registers.IFF1 = (byte)(cpu.IFF1 ? 1 : 0);
        registers.IFF2 = (byte)(cpu.IFF2 ? 1 : 0);

        if (cpu.IsHalted)
        {
            registers.Flags = Z80RegsBlock.FlagsHalted;
        }
    }

    private static void SaveMemory(IMemory memory, List<RamPageBlock> ramPages, SpecRegsBlock specRegs, CompressionLevel compressionLevel)
    {
        switch (memory)
        {
            case Memory16K memory16K:
                ramPages.Add(new RamPageBlock(memory16K.Memory[0x4000..0x8000], pageNumber: 5, compressionLevel));
                break;

            case Memory48K memory48K:
                ramPages.Add(new RamPageBlock(memory48K.Memory[0x4000..0x8000], pageNumber: 5, compressionLevel));
                ramPages.Add(new RamPageBlock(memory48K.Memory[0x8000..0xC000], pageNumber: 2, compressionLevel));
                ramPages.Add(new RamPageBlock(memory48K.Memory[0xC000..0x10000], pageNumber: 0, compressionLevel));
                break;

            case Memory128K memory128K:
            {
                for (byte i = 0; i < 8; i++)
                {
                    ramPages.Add(new RamPageBlock(memory128K.Banks[i][..0x4000], pageNumber: i, compressionLevel));
                }

                specRegs.Port7FFD = memory128K.LastPagingModeValue;
                break;
            }
        }
    }

    private static void SaveCustomRom(IMemory memory, SzxFile snapshot, CompressionLevel compressionLevel)
    {
        if (memory is Memory16K memory16K)
        {
            snapshot.CustomRom = new CustomRomBlock(memory16K.Memory[..0x4000], compressionLevel);
        }
        else if (memory is Memory48K memory48K)
        {
            snapshot.CustomRom = new CustomRomBlock(memory48K.Memory[..0x4000], compressionLevel);
        }
        else if (memory is Memory128K memory128K)
        {
            snapshot.CustomRom = new CustomRomBlock(ConcatenateArrays(memory128K.RomBank0, memory128K.RomBank1), compressionLevel);
        }
    }

    private static void SaveSpectrumRegisters(ScreenBuffer screenBuffer, SpecRegsBlock specRegs) =>
        specRegs.Border = SpectrumPalette.ReverseBorderColors[screenBuffer.LastBorderColor];

    private static void SaveUlaPlus(UlaPlus ulaPlus, SzxFile snapshot)
    {
        if (!ulaPlus.IsEnabled || !ulaPlus.IsActive)
        {
            return;
        }

        snapshot.Palette = new PaletteBlock
        {
            Flags = PaletteBlock.FlagsPaletteEnabled,
            CurrentRegister = ulaPlus.PaletteGroup,
        };

        ulaPlus.GetPaletteData().CopyTo(snapshot.Palette.Registers, 0);
    }

    private static void SaveJoystick(JoystickManager joystickManager, SzxFile snapshot)
    {
        if (joystickManager.JoystickType == JoystickType.None)
        {
            return;
        }

        snapshot.Joystick = new JoystickBlock
        {
            JoystickTypePlayer1 = joystickManager.JoystickType switch
            {
                JoystickType.Kempston => JoystickBlock.JoystickKempston,
                JoystickType.Sinclair1 => JoystickBlock.JoystickSinclair1,
                JoystickType.Sinclair2 => JoystickBlock.JoystickSinclair2,
                JoystickType.Cursor => JoystickBlock.JoystickCursor,
                JoystickType.Fuller => JoystickBlock.JoystickFuller,
                _ => JoystickBlock.JoystickDisabled,
            },
            JoystickTypePlayer2 = JoystickBlock.JoystickDisabled
        };
    }

    private static void SaveTape(TapeManager tapeManager, SzxFile snapshot, CompressionLevel compressionLevel)
    {
        if (tapeManager.Cassette.IsEmpty)
        {
            return;
        }

        snapshot.Tape = new TapeBlock(tapeManager.Cassette.ContentBytes, compressionLevel)
        {
            FileExtension = "tzx",
            CurrentBlockNo = (ushort)tapeManager.Cassette.Position
        };
    }

    private static void SaveAyRegisters(AudioManager audioManager, SzxFile snapshot)
    {
        if (!audioManager.IsAySupported)
        {
            return;
        }

        snapshot.Ay = new AyBlock
        {
            CurrentRegister = audioManager.Ay.CurrentRegister
        };
        for (var i = 0; i <  audioManager.Ay.Registers.Length; i++)
        {
            snapshot.Ay.Registers[i] = audioManager.Ay.Registers[i];
        }
    }

    private static T[] ConcatenateArrays<T>(T[] first, T[] second)
    {
        var result = new T[first.Length + second.Length];

        Array.Copy(first, 0, result, 0, first.Length);
        Array.Copy(second, 0, result, first.Length, second.Length);

        return result;
    }
}