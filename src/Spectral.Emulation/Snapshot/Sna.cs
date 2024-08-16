using OldBit.Spectral.Emulation.Devices.Memory;
using OldBit.Spectral.Emulation.Rom;
using OldBit.Spectral.Emulation.Screen;
using OldBit.Z80Cpu;
using OldBit.ZXTape.Sna;

namespace OldBit.Spectral.Emulation.Snapshot;

internal static class Sna
{
    internal static Emulator Load(string fileName)
    {
        var snapshot = SnaFile.Load(fileName);

        var emulator = EmulatorFactory.Create(snapshot.Header128 != null ?
            ComputerType.Spectrum128K : ComputerType.Spectrum48K, RomType.Original);
        var (cpu, memory, screenBuffer) = (emulator.Cpu, emulator.Memory, emulator.ScreenBuffer);

        cpu.Registers.AF = snapshot.Header.AF;
        cpu.Registers.BC = snapshot.Header.BC;
        cpu.Registers.DE = snapshot.Header.DE;
        cpu.Registers.HL = snapshot.Header.HL;

        cpu.Registers.Prime.AF = snapshot.Header.AFPrime;
        cpu.Registers.Prime.BC = snapshot.Header.BCPrime;
        cpu.Registers.Prime.DE = snapshot.Header.DEPrime;
        cpu.Registers.Prime.HL = snapshot.Header.HLPrime;

        cpu.Registers.IX = snapshot.Header.IX;
        cpu.Registers.IY = snapshot.Header.IY;
        cpu.Registers.SP = snapshot.Header.SP;

        cpu.Registers.I = snapshot.Header.I;
        cpu.Registers.R = snapshot.Header.R;
        cpu.IM = (InterruptMode)snapshot.Header.InterruptMode;
        cpu.IFF1 = (snapshot.Header.Interrupt & 0x04) != 0;
        cpu.IFF2 = (snapshot.Header.Interrupt & 0x04) != 0;

        if (snapshot.Header128 != null)
        {
            LoadMemory(snapshot, (Memory128K)memory);

            cpu.Registers.PC = snapshot.Header128.PC;
        }
        else
        {
            LoadMemory(snapshot, (Memory48K)memory);

            cpu.Registers.PC = (Word)(memory.Read((Word)(cpu.Registers.SP + 1)) << 8 |
                                      memory.Read(cpu.Registers.SP));
            cpu.Registers.SP += 2;
        }

        screenBuffer.Reset();
        var borderColor = Palette.BorderColors[(byte)(snapshot.Header.BorderColor & 0x07)];
        screenBuffer.UpdateBorder(borderColor);

        return emulator;
    }

    internal static void Save(string fileName, Emulator emulator)
    {
        var snapshot = new SnaFile
        {
            Header =
            {
                AF = emulator.Cpu.Registers.AF,
                BC = emulator.Cpu.Registers.BC,
                DE = emulator.Cpu.Registers.DE,
                HL = emulator.Cpu.Registers.HL,

                AFPrime = emulator.Cpu.Registers.Prime.AF,
                BCPrime = emulator.Cpu.Registers.Prime.BC,
                DEPrime = emulator.Cpu.Registers.Prime.DE,
                HLPrime = emulator.Cpu.Registers.Prime.HL,

                IX = emulator.Cpu.Registers.IX,
                IY = emulator.Cpu.Registers.IY,
                SP = emulator.Cpu.Registers.SP,

                I = emulator.Cpu.Registers.I,
                R = emulator.Cpu.Registers.R,
                InterruptMode = (byte)emulator.Cpu.IM,
                Interrupt = (byte)((emulator.Cpu.IFF2 ? 0x04 : 0x00) | (emulator.Cpu.IFF1 ? 0x02 : 0x00)),

                BorderColor = Palette.BorderColors.Where(c => c.Value == emulator.ScreenBuffer.LastBorderColor)
                    .Select(c => c.Key).FirstOrDefault()
            }
        };

        if (emulator.ComputerType == ComputerType.Spectrum128K)
        {
            var memory = (Memory128K)emulator.Memory;

            snapshot.Header128 = new SnaHeader128
            {
                PageMode = memory.LastPortValue,
                PC = emulator.Cpu.Registers.PC,
            };
        }

        // TODO: Populate snapshot with emulator state

        snapshot.Save(fileName);
    }

    private static void LoadMemory(SnaFile snapshot, Memory48K memory)
    {
        for (var i = 0x4000; i <= 0xFFFF; i++)
        {
            memory.Write((Word)i, snapshot.Ram48[i - 0x4000]);
        }
    }

    private static void LoadMemory(SnaFile snapshot, Memory128K memory)
    {
        var loadedPages = new bool[8];

        // Bank 5
        var bank = 5;
        for (var i = 0x4000; i <= 0x7FFF; i++)
        {
            memory.Banks[bank][i - 0x4000] = snapshot.Ram48[i - 0x4000];
        }
        loadedPages[bank] = true;

        // Bank 2
        bank = 2;
        for (var i = 0x8000; i <= 0xBFFF; i++)
        {
            memory.Banks[bank][i - 0x8000] = snapshot.Ram48[i - 0x4000];
        }
        loadedPages[bank] = true;

        // Bank n, skip if already loaded e.g. 5 or 2
        bank = snapshot.Header128!.PageMode & 0x07;
        if (!loadedPages[bank])
        {
            for (var i = 0xC000; i <= 0xFFFF; i++)
            {
                memory.Banks[bank][i - 0xC000] = snapshot.Ram48[i - 0x4000];
            }

            loadedPages[bank] = true;
        }

        // Remaining banks
        var currentSnaBank = 0;
        for (bank = 0; bank < 8; bank++)
        {
            if (loadedPages[bank])
            {
                continue;
            }

            for (var j = 0; j < 0x4000; j++)
            {
                memory.Banks[bank][j] = snapshot.RamBanks![currentSnaBank][j];
            }
            loadedPages[bank] = true;

            currentSnaBank += 1;
        }

        memory.SetPagingMode(snapshot.Header128.PageMode);
    }
}