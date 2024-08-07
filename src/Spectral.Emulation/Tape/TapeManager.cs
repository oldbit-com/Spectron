using OldBit.Spectral.Emulation.Screen;
using OldBit.Z80Cpu;
using OldBit.ZXTape.Extensions;
using OldBit.ZXTape.Tap;
using OldBit.ZXTape.Tzx;

namespace OldBit.Spectral.Emulation.Tape;

public sealed class TapeManager
{
    internal TapePlayer TapePlayer { get; }
    internal FileLoader FileLoader { get; }

    internal TapeManager(Z80 z80, IMemory memory, ScreenBuffer screenBuffer)
    {
        TapePlayer = new TapePlayer(z80.Clock);
        FileLoader = new FileLoader(z80, memory, screenBuffer, TapePlayer);
    }

    public void LoadAndRun(string fileName) => FileLoader.LoadFile(fileName);

    public bool TryInsertTape(string fileName)
    {
        if (!FileLoader.TryLoadTape(fileName, out var tzxFile))
        {
            return false;
        }

        TapePlayer.LoadTape(tzxFile);

        return true;
    }

    public void PlayTape() => TapePlayer.Play();

    public void EjectTape()
    {
        TapePlayer.Stop();
        TapePlayer.Rewind();
    }

    public void Play() => TapePlayer.Play();
}