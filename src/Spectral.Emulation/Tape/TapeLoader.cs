using OldBit.Spectral.Emulation.Screen;
using OldBit.Z80Cpu;
using OldBit.ZXTape.Sna;
using OldBit.ZXTape.Tap;
using OldBit.ZXTape.Tzx;

namespace OldBit.Spectral.Emulation.Tape;

public sealed class TapeLoader
{
    private readonly Z80 _z80;
    private readonly IMemory _memory;
    private readonly ScreenRenderer _screenRenderer;
    private readonly TapePlayer _tapePlayer;

    internal TapeLoader(Z80 z80, IMemory memory, ScreenRenderer screenRenderer, TapePlayer tapePlayer)
    {
        _z80 = z80;
        _memory = memory;
        _screenRenderer = screenRenderer;
        _tapePlayer = tapePlayer;
    }

    public void LoadFile(string fileName)
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

        _tapePlayer.LoadTape(tapFile);
    }

    private void LoadTap(string fileName)
    {
        var tapFile = TapFile.Load(fileName);

        _tapePlayer.LoadTape(tapFile);
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
            _memory.Write((Word)i, file.Data.Ram48[i - 16384]);
        }

        _z80.Registers.AF = file.Data.Header.AF;
        _z80.Registers.BC = file.Data.Header.BC;
        _z80.Registers.DE = file.Data.Header.DE;
        _z80.Registers.HL = file.Data.Header.HL;

        _z80.Registers.Prime.AF = file.Data.Header.AFPrime;
        _z80.Registers.Prime.BC = file.Data.Header.BCPrime;
        _z80.Registers.Prime.DE = file.Data.Header.DEPrime;
        _z80.Registers.Prime.HL = file.Data.Header.HLPrime;

        _z80.Registers.IX = file.Data.Header.IX;
        _z80.Registers.IY = file.Data.Header.IY;
        _z80.Registers.SP = file.Data.Header.SP;

        _z80.Registers.I = file.Data.Header.I;
        _z80.Registers.R = file.Data.Header.R;
        _z80.IM = (InterruptMode)file.Data.Header.InterruptMode;
        _z80.IFF2 = (file.Data.Header.Interrupt & 0x04) != 0;

        _z80.Registers.PC = (Word)(_memory.Read((Word)(_z80.Registers.SP + 1)) << 8 |
                                   _memory.Read(_z80.Registers.SP));
        _z80.Registers.SP += 2;

        _screenRenderer.Reset();
        var borderColor = Colors.BorderColors[(byte)(file.Data.Header.Border & 0x07)];
        _screenRenderer.UpdateBorder(borderColor);
    }

    private void LoadSzx(string fileName)
    {
        throw new NotImplementedException();
    }
}