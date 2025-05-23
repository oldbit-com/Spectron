using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Spectron.Emulation.Screen;
using OldBit.Z80Cpu;
using OldBit.Spectron.Files.Sna;

namespace OldBit.Spectron.Emulation.Snapshot;

public sealed class SnaSnapshot(EmulatorFactory emulatorFactory)
{
    internal Emulator Load(Stream stream)
    {
        var snapshot = SnaFile.Load(stream);

        return CreateEmulator(snapshot);
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

                BorderColor = SpectrumPalette.ReverseBorderColors[emulator.ScreenBuffer.LastBorderColor]
            }
        };

        if (emulator.ComputerType == ComputerType.Spectrum128K)
        {
            var memory = (Memory128K)emulator.Memory;

            snapshot.Header128 = new SnaHeader128
            {
                PageMode = memory.LastPagingModeValue,
                PC = emulator.Cpu.Registers.PC,
            };
        }
        else
        {
            switch (emulator.Memory)
            {
                case Memory16K memory16K:
                    snapshot.Ram48 = new byte[49152];
                    Array.Copy(memory16K.Ram.ToArray(), snapshot.Ram48, memory16K.Ram.Length);
                    break;

                case Memory48K memory48K:
                    snapshot.Ram48 = memory48K.Ram.ToArray();
                    break;
            }
        }

        snapshot.Save(fileName);
    }

    private Emulator CreateEmulator(SnaFile snapshot)
    {
        var emulator = emulatorFactory.Create(snapshot.Header128 != null ?
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

        var borderColor = SpectrumPalette.GetBorderColor(snapshot.Header.BorderColor);

        screenBuffer.Reset();
        screenBuffer.UpdateBorder(borderColor);

        return emulator;
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
        // Load Bank 5
        LoadBank(memory.Banks[5], snapshot.Ram48);

        // Load Bank 2
        LoadBank(memory.Banks[2], snapshot.Ram48, 0x4000);

        // Bank n, skip if already loaded e.g. 5 or 2
        var bankN = snapshot.Header128!.PageMode & 0x07;
        if (bankN != 5 && bankN != 2)
        {
            LoadBank(memory.Banks[bankN], snapshot.Ram48, 0x8000);
        }

        // Remaining banks
        var currentSnaBank = 0;
        for (var bank = 0; bank < 8; bank++)
        {
            if (bank == 2 || bank == 5 || bank == bankN)
            {
                continue;
            }

            LoadBank(memory.Banks[bank], snapshot.RamBanks![currentSnaBank]);
            currentSnaBank++;
        }

        memory.SetPagingMode(snapshot.Header128.PageMode);
    }

    private static void LoadBank(byte[] bank, byte[] data, int offset = 0)
    {
        for (var i = 0; i < 0x4000; i++)
        {
            bank[i] = data[i + offset];
        }
    }
}