using System.IO.Compression;
using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Spectron.Emulation.Screen;
using OldBit.Z80Cpu;
using OldBit.ZXTape.Szx;
using OldBit.ZXTape.Szx.Blocks;

namespace OldBit.Spectron.Emulation.Snapshot;

internal static class SzxSnapshot
{
    internal static Emulator Load(string fileName)
    {
        var snapshot = SzxFile.Load(fileName);

        var computerType = snapshot.Header.MachineId switch
        {
            SzxHeader.MachineId16K => ComputerType.Spectrum48K,
            SzxHeader.MachineId48K => ComputerType.Spectrum48K,
            SzxHeader.MachineId128K => ComputerType.Spectrum128K,
            _ => throw new NotSupportedException($"Snapshot hardware mode not supported: {snapshot.Header.MachineId}")
        };

        var romType = snapshot.CustomRom?.Data != null ? RomType.Custom : RomType.Original;

        var emulator = EmulatorFactory.Create(computerType, romType, snapshot.CustomRom?.Data);

        var (cpu, memory, screenBuffer) = (emulator.Cpu, emulator.Memory, emulator.ScreenBuffer);

        LoadRegisters(cpu, snapshot.Z80Registers);
        LoadMemory(memory, snapshot.RamPages, snapshot.SpecRegs);
        LoadSpectrumRegisters(screenBuffer, snapshot.SpecRegs);

        // TODO: Load the rest of the snapshot

        return emulator;
    }

    internal static void Save(string fileName, Emulator emulator)
    {
        var snapshot = CreateSnapshot(emulator);

        snapshot.Save(fileName);
    }

    internal static SzxFile CreateSnapshot(Emulator emulator, CompressionLevel compressionLevel = CompressionLevel.SmallestSize)
    {
        var snapshot = new SzxFile();

        SaveRegisters(emulator.Cpu, snapshot.Z80Registers);
        SaveMemory(emulator.Memory, snapshot.RamPages, snapshot.SpecRegs, compressionLevel);
        SaveSpectrumRegisters(emulator.ScreenBuffer, snapshot.SpecRegs);

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

        // TODO: Restart cycle counter at the gven value
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
                    var bankNumber = ramPage.PageNumber - 3;
                    if (bankNumber is < 0 or >= 8)
                    {
                        continue;
                    }

                    var bank = memory128.Banks[bankNumber];
                    for (var i = 0; i < ramPage.Data.Length; i++)
                    {
                        bank[i] = ramPage.Data[i];
                    }

                    memory128.SetPagingMode(specRegs.Port7FFD);

                    break;

                default:
                    throw new NotSupportedException($"Memory type not supported: {memory.GetType()}");
            }
        }
    }

    private static void LoadSpectrumRegisters(ScreenBuffer screenBuffer, SpecRegsBlock specRegs)
    {
        var borderColor = Palette.BorderColors[(byte)(specRegs.Border & 0x07)];

        screenBuffer.Reset();
        screenBuffer.UpdateBorder(borderColor);
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
                    ramPages.Add(new RamPageBlock(memory128K.Banks[i], pageNumber: i, compressionLevel));
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

    private static void SaveSpectrumRegisters(ScreenBuffer screenBuffer, SpecRegsBlock specRegs)
    {
        specRegs.Border = Palette.ReverseBorderColors[screenBuffer.LastBorderColor];
    }

    private static T[] ConcatenateArrays<T>(T[] first, T[] second)
    {
        var result = new T[first.Length + second.Length];

        Array.Copy(first, 0, result, 0, first.Length);
        Array.Copy(second, 0, result, first.Length, second.Length);

        return result;
    }
}