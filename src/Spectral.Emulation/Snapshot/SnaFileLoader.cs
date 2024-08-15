using OldBit.Spectral.Emulation.Rom;
using OldBit.Spectral.Emulation.Screen;
using OldBit.Z80Cpu;
using OldBit.ZXTape.Sna;

namespace OldBit.Spectral.Emulation.Snapshot;

internal static class SnaFileLoader
{
    internal static Emulator Load(string fileName)
    {
        var snapshot = SnaFile.Load(fileName);

        var emulator = EmulatorFactory.Create(snapshot.Header128 != null ?
            ComputerType.Spectrum128K : ComputerType.Spectrum48K, RomType.Original);
        var (cpu, memory, screenBuffer) = (emulator.Cpu, emulator.Memory, emulator.ScreenBuffer);

        for (var i = 16384; i < 65536; i++)
        {
            memory.Write((Word)i, snapshot.Ram48[i - 16384]);
        }

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
        cpu.IFF2 = (snapshot.Header.Interrupt & 0x04) != 0;

        cpu.Registers.PC = (Word)(memory.Read((Word)(cpu.Registers.SP + 1)) << 8 |
                                  memory.Read(cpu.Registers.SP));
        cpu.Registers.SP += 2;

        screenBuffer.Reset();
        var borderColor = Palette.BorderColors[(byte)(snapshot.Header.BorderColor & 0x07)];
        screenBuffer.UpdateBorder(borderColor);

        return emulator;
    }
}