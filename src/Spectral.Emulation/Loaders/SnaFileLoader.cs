using OldBit.Spectral.Emulation.Screen;
using OldBit.Z80Cpu;
using OldBit.ZXTape.Sna;

namespace OldBit.Spectral.Emulation.Loaders;

internal class SnaFileLoader(Emulator emulator)
{
    public void Load(string fileName)
    {
        var file = SnaFile.Load(fileName);
        var (z80, memory, screenBuffer) = (emulator.Z80, emulator.Memory, emulator.ScreenBuffer);

        for (var i = 16384; i < 65536; i++)
        {
            memory.Write((Word)i, file.Data.Ram48[i - 16384]);
        }

        z80.Registers.AF = file.Data.Header.AF;
        z80.Registers.BC = file.Data.Header.BC;
        z80.Registers.DE = file.Data.Header.DE;
        z80.Registers.HL = file.Data.Header.HL;

        z80.Registers.Prime.AF = file.Data.Header.AFPrime;
        z80.Registers.Prime.BC = file.Data.Header.BCPrime;
        z80.Registers.Prime.DE = file.Data.Header.DEPrime;
        z80.Registers.Prime.HL = file.Data.Header.HLPrime;

        z80.Registers.IX = file.Data.Header.IX;
        z80.Registers.IY = file.Data.Header.IY;
        z80.Registers.SP = file.Data.Header.SP;

        z80.Registers.I = file.Data.Header.I;
        z80.Registers.R = file.Data.Header.R;
        z80.IM = (InterruptMode)file.Data.Header.InterruptMode;
        z80.IFF2 = (file.Data.Header.Interrupt & 0x04) != 0;

        z80.Registers.PC = (Word)(memory.Read((Word)(z80.Registers.SP + 1)) << 8 |
                                  memory.Read(z80.Registers.SP));
        z80.Registers.SP += 2;

        screenBuffer.Reset();
        var borderColor = Palette.BorderColors[(byte)(file.Data.Header.BorderColor & 0x07)];
        screenBuffer.UpdateBorder(borderColor);
    }
}