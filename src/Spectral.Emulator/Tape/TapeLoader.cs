using OldBit.Spectral.Emulator.Screen;
using OldBit.Z80Cpu;
using OldBit.ZXTape.Sna;
using OldBit.ZXTape.Tap;
using OldBit.ZXTape.Tzx;

namespace OldBit.Spectral.Emulator.Tape;

internal class TapeLoader(Z80 z80, IMemory memory, ScreenRenderer screenRenderer, TapePlayer tapePlayer)
{
    internal void LoadFile(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        switch (ext)
        {
            case ".sna":
                LoadSna(fileName);
                break;

            case ".szx":
                LoadSzx(fileName);
                break;

            case ".z80":
                LoadZ80(fileName);
                break;

            case ".tap":
                LoadTap(fileName);
                break;

            case ".tzx":
                LoadTzx(fileName);
                break;

            default:
                throw new NotSupportedException($"The file extension '{ext}' is not supported.");
        }
    }

    private void LoadTzx(string fileName)
    {
        var tzxFile = TzxFile.Load(fileName);
        var tapFile = tzxFile.ToTap();

        tapePlayer.LoadTape(tapFile);
    }

    private void LoadTap(string fileName)
    {
        var tapFile = TapFile.Load(fileName);

        tapePlayer.LoadTape(tapFile);
    }

    private void LoadZ80(string fileName)
    {
        throw new NotImplementedException();
    }

    private void LoadSna(string fileName)
    {
        var file = SnaFile.Load(fileName);

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

        var borderColor = Colors.BorderColors[(byte)(file.Data.Header.Border & 0x07)];
        screenRenderer.UpdateBorder(borderColor, 0);
    }

    private void LoadSzx(string fileName)
    {
        throw new NotImplementedException();
    }
}