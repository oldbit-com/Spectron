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
        UpdateBorder(screenBuffer, snapshot.SpecRegs);

        return emulator;
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

    private static void UpdateBorder(ScreenBuffer screenBuffer, SpecRegsBlock specRegs)
    {
        var borderColor = Palette.BorderColors[(byte)(specRegs.Border & 0x07)];

        screenBuffer.Reset();
        screenBuffer.UpdateBorder(borderColor);
    }
}