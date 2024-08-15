using OldBit.Spectral.Emulation.Rom;
using OldBit.Spectral.Emulation.Screen;
using OldBit.Z80Cpu;
using OldBit.ZXTape.Sna;

namespace OldBit.Spectral.Emulation.Snapshot;

internal static class SnaFileLoader
{
    internal static Emulator Load(string fileName)
    {
        var sna = SnaFile.Load(fileName);
        var emulator = EmulatorFactory.Create(sna.Header128 != null ?
            ComputerType.Spectrum128K : ComputerType.Spectrum48K, RomType.Original);

        var (cpu, memory, screenBuffer) = (emulator.Cpu, emulator.Memory, emulator.ScreenBuffer);

        for (var i = 16384; i < 65536; i++)
        {
            memory.Write((Word)i, sna.Ram48[i - 16384]);
        }

        cpu.Registers.AF = sna.Header.AF;
        cpu.Registers.BC = sna.Header.BC;
        cpu.Registers.DE = sna.Header.DE;
        cpu.Registers.HL = sna.Header.HL;

        cpu.Registers.Prime.AF = sna.Header.AFPrime;
        cpu.Registers.Prime.BC = sna.Header.BCPrime;
        cpu.Registers.Prime.DE = sna.Header.DEPrime;
        cpu.Registers.Prime.HL = sna.Header.HLPrime;

        cpu.Registers.IX = sna.Header.IX;
        cpu.Registers.IY = sna.Header.IY;
        cpu.Registers.SP = sna.Header.SP;

        cpu.Registers.I = sna.Header.I;
        cpu.Registers.R = sna.Header.R;
        cpu.IM = (InterruptMode)sna.Header.InterruptMode;
        cpu.IFF2 = (sna.Header.Interrupt & 0x04) != 0;

        cpu.Registers.PC = (Word)(memory.Read((Word)(cpu.Registers.SP + 1)) << 8 |
                                  memory.Read(cpu.Registers.SP));
        cpu.Registers.SP += 2;

        screenBuffer.Reset();
        var borderColor = Palette.BorderColors[(byte)(sna.Header.BorderColor & 0x07)];
        screenBuffer.UpdateBorder(borderColor);

        return emulator;
    }
}