using System.Diagnostics.CodeAnalysis;
using OldBit.Spectral.Emulation.Screen;
using OldBit.Z80Cpu;
using OldBit.Z80Cpu.Registers;
using OldBit.ZXTape.Extensions;
using OldBit.ZXTape.Sna;
using OldBit.ZXTape.Tap;
using OldBit.ZXTape.Tzx;
using OldBit.ZXTape.Z80;

namespace OldBit.Spectral.Emulation.Tape;

public sealed class FileLoader
{
    private readonly Z80 _z80;
    private readonly IMemory _memory;
    private readonly ScreenBuffer _screenBuffer;
    private readonly TapePlayer _tapePlayer;

    internal FileLoader(Z80 z80, IMemory memory, ScreenBuffer screenBuffer, TapePlayer tapePlayer)
    {
        _z80 = z80;
        _memory = memory;
        _screenBuffer = screenBuffer;
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

    public static bool TryLoadTape(string fileName, [NotNullWhen(true)] out TzxFile? tzxFile)
    {
        tzxFile = null;

        if (IsTapFile(fileName))
        {
            var tapFile = TapFile.Load(fileName);
            tzxFile = tapFile.ToTzx();
        }
        else if (IsTzxFile(fileName))
        {
            tzxFile = TzxFile.Load(fileName);
        }

        return tzxFile != null;
    }

    private void LoadTzx(string fileName)
    {
        var tzxFile = TzxFile.Load(fileName);

        //_tapePlayer.LoadTape(tzxFile);
    }

    private void LoadTap(string fileName)
    {
        var tapFile = TapFile.Load(fileName);

        //_tapePlayer.LoadTape(tapFile.ToTzx());
    }

    private void LoadZ80(string fileName)
    {
        var file = Z80File.Load(fileName);

        _z80.Registers.A = file.Header.A;
        _z80.Registers.F = (Flags)file.Header.F;
        _z80.Registers.BC = file.Header.BC;
        _z80.Registers.DE = file.Header.DE;
        _z80.Registers.HL = file.Header.HL;

        _z80.Registers.Prime.A = file.Header.APrime;
        _z80.Registers.Prime.F = (Flags)file.Header.FPrime;
        _z80.Registers.Prime.BC = file.Header.BCPrime;
        _z80.Registers.Prime.DE = file.Header.DEPrime;
        _z80.Registers.Prime.HL = file.Header.HLPrime;

        _z80.Registers.IX = file.Header.IX;
        _z80.Registers.IY = file.Header.IY;
        _z80.Registers.SP = file.Header.SP;

        _z80.Registers.I = file.Header.I;
        _z80.Registers.R = file.Header.R;
        _z80.IM = (InterruptMode)(file.Header.Flags & 0x03);
        _z80.IFF1 = (file.Header.IFF1 & 0x01) != 0;
        _z80.IFF2 = (file.Header.IFF2 & 0x01) != 0;

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

        _screenBuffer.Reset();
        var borderColor = Palette.BorderColors[(byte)(file.Data.Header.Border & 0x07)];
        _screenBuffer.UpdateBorder(borderColor);
    }

    private void LoadSzx(string fileName)
    {
        throw new NotImplementedException();
    }

    private static bool IsTapFile(string fileName) => Path.GetExtension(fileName).Equals(".tap", StringComparison.InvariantCultureIgnoreCase);
    private static bool IsTzxFile(string fileName) => Path.GetExtension(fileName).Equals(".tzx", StringComparison.InvariantCultureIgnoreCase);
}