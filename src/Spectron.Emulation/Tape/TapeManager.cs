using System.Diagnostics.CodeAnalysis;
using OldBit.Spectron.Emulation.Storage;
using OldBit.ZXTape.Extensions;
using OldBit.ZXTape.Tap;
using OldBit.ZXTape.Tzx;

namespace OldBit.Spectron.Emulation.Tape;

public sealed class TapeManager
{
    internal TapePlayer TapePlayer { get; }
    internal InstantTapeLoader InstantTapeLoader { get; }

    public delegate void TapeInsertedEvent(EventArgs e);
    public event TapeInsertedEvent? TapeInserted;

    public delegate void TapePlayingEvent(EventArgs e);
    public event TapePlayingEvent? TapePlaying;

    public delegate void TapeStoppedEvent(EventArgs e);
    public event TapeStoppedEvent? TapeStopped;

    public delegate void TapeEjectedEvent(EventArgs e);
    public event TapeEjectedEvent? TapeEjected;

    internal TapeManager(Emulator emulator, HardwareSettings hardware)
    {
        TapePlayer = new TapePlayer(emulator.Cpu.Clock, hardware);
        InstantTapeLoader = new InstantTapeLoader(emulator.Cpu, emulator.Memory, TapePlayer);
    }

    public bool TryLoadTape(string fileName)
    {
        if (!TryInsertTape(fileName))
        {
            return false;
        }

        PlayTape();

        return true;
    }

    public bool TryInsertTape(string fileName)
    {
        StopTape();

        if (!TryLoadTape(fileName, out var tzxFile))
        {
            return false;
        }

        TapePlayer.LoadTape(tzxFile);
        TapeInserted?.Invoke(EventArgs.Empty);

        return true;
    }

    public void StopTape()
    {
        TapePlayer.Stop();
        TapeStopped?.Invoke(EventArgs.Empty);
    }

    public void PlayTape()
    {
        TapePlayer.Play();
        TapePlaying?.Invoke(EventArgs.Empty);
    }

    public void EjectTape()
    {
        StopTape();
        TapeEjected?.Invoke(EventArgs.Empty);
    }

    private static bool TryLoadTape(string fileName, [NotNullWhen(true)] out TzxFile? tzxFile)
    {
        var fileType = FileTypeHelper.GetFileType(fileName);
        tzxFile = null;

        switch (fileType)
        {
            case FileType.Tap:
            {
                var tapFile = TapFile.Load(fileName);
                tzxFile = tapFile.ToTzx();
                break;
            }

            case FileType.Tzx:
                tzxFile = TzxFile.Load(fileName);
                break;
        }

        return tzxFile != null;
    }
}