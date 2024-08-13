using OldBit.Spectral.Emulation.Screen;
using OldBit.Z80Cpu;
using OldBit.Z80Cpu.Registers;
using OldBit.ZXTape.Z80;

namespace OldBit.Spectral.Emulation.Loaders;

internal class Z80FileLoader(Emulator emulator)
{
    public void Load(string fileName)
    {
        var file = Z80File.Load(fileName);
        var (z80, memory, screenBuffer) = (emulator.Z80, emulator.Memory, emulator.ScreenBuffer);

        z80.Registers.A = file.Header.A;
        z80.Registers.F = (Flags)file.Header.F;
        z80.Registers.BC = file.Header.BC;
        z80.Registers.DE = file.Header.DE;
        z80.Registers.HL = file.Header.HL;

        z80.Registers.Prime.A = file.Header.APrime;
        z80.Registers.Prime.F = (Flags)file.Header.FPrime;
        z80.Registers.Prime.BC = file.Header.BCPrime;
        z80.Registers.Prime.DE = file.Header.DEPrime;
        z80.Registers.Prime.HL = file.Header.HLPrime;

        z80.Registers.IX = file.Header.IX;
        z80.Registers.IY = file.Header.IY;
        z80.Registers.SP = file.Header.SP;

        z80.Registers.I = file.Header.I;
        z80.Registers.R = (byte)(file.Header.R & 0x7F | (file.Header.RBit7 == 0 ? 0x00 : 0x80));
        z80.IM = (InterruptMode)(file.Header.InterruptMode);
        z80.IFF1 = (file.Header.IFF1 & 0x01) != 0;
        z80.IFF2 = (file.Header.IFF2 & 0x01) != 0;

        screenBuffer.Reset();
        var borderColor = Palette.BorderColors[(byte)(file.Header.BorderColor & 0x07)];
        screenBuffer.UpdateBorder(borderColor);

        throw new NotImplementedException();
    }
}