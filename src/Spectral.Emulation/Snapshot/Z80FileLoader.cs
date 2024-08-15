using OldBit.Spectral.Emulation.Screen;
using OldBit.Z80Cpu;
using OldBit.Z80Cpu.Registers;
using OldBit.ZXTape.Z80;

namespace OldBit.Spectral.Emulation.Snapshot;

internal class Z80FileLoader(Emulator emulator)
{
    public void Load(string fileName)
    {
        var file = Z80File.Load(fileName);
        var (cpu, memory, screenBuffer) = (emulator.Cpu, emulator.Memory, emulator.ScreenBuffer);

        cpu.Registers.A = file.Header.A;
        cpu.Registers.F = (Flags)file.Header.F;
        cpu.Registers.BC = file.Header.BC;
        cpu.Registers.DE = file.Header.DE;
        cpu.Registers.HL = file.Header.HL;

        cpu.Registers.Prime.A = file.Header.APrime;
        cpu.Registers.Prime.F = (Flags)file.Header.FPrime;
        cpu.Registers.Prime.BC = file.Header.BCPrime;
        cpu.Registers.Prime.DE = file.Header.DEPrime;
        cpu.Registers.Prime.HL = file.Header.HLPrime;

        cpu.Registers.IX = file.Header.IX;
        cpu.Registers.IY = file.Header.IY;
        cpu.Registers.SP = file.Header.SP;

        cpu.Registers.I = file.Header.I;
        cpu.Registers.R = file.Header.R;
        cpu.IM = (InterruptMode)file.Header.Flags2.InterruptMode;
        cpu.IFF1 = (file.Header.IFF1 & 0x01) != 0;
        cpu.IFF2 = (file.Header.IFF2 & 0x01) != 0;

        screenBuffer.Reset();
        var borderColor = Palette.BorderColors[(byte)(file.Header.Flags1.BorderColor & 0x07)];
        screenBuffer.UpdateBorder(borderColor);

        throw new NotImplementedException();
    }
}