using OldBit.Z80Cpu;
using OldBit.ZX.Files.Tzx;

namespace OldBit.Spectron.Emulation.Tape;

public sealed class TapeManager
{
    private DirectAccess? _directAccess;

    public TapeFile TapeFile { get; set; } = new();

    internal TapePlayer? TapePlayer { get; private set; }

    public delegate void TapeInsertedEvent(EventArgs e);
    public event TapeInsertedEvent? TapeInserted;

    public delegate void TapePlayingEvent(EventArgs e);
    public event TapePlayingEvent? TapePlaying;

    public delegate void TapeStoppedEvent(EventArgs e);
    public event TapeStoppedEvent? TapeStopped;

    public delegate void TapeEjectedEvent(EventArgs e);
    public event TapeEjectedEvent? TapeEjected;

    internal void Attach(Z80 cpu, IMemory memory, HardwareSettings hardware)
    {
        TapePlayer = new TapePlayer(cpu.Clock, hardware);
        _directAccess = new DirectAccess(cpu, memory);
    }

    public void NewTape() => TapeFile = new TapeFile();

    public void LoadDirect() => _directAccess?.LoadBytes(TapeFile);

    public void SaveDirect() => _directAccess?.SaveBytes(TapeFile);

    public void InsertTape(string fileName, bool autoPlay = false)
    {
        StopTape();

        TapeFile.Load(fileName);
        InsertTape();

        if (autoPlay)
        {
            PlayTape();
        }
    }

    public void InsertTape(TzxFile tzxFile)
    {
        TapeFile.Load(tzxFile);
        InsertTape();
    }

    private void InsertTape()
    {
        TapePlayer?.LoadTape(TapeFile);

        TapeInserted?.Invoke(EventArgs.Empty);
    }

    public void StopTape()
    {
        TapePlayer?.Stop();
        TapeStopped?.Invoke(EventArgs.Empty);
    }

    public void PlayTape()
    {
        TapePlayer?.Play();
        TapePlaying?.Invoke(EventArgs.Empty);
    }

    public void EjectTape()
    {
        StopTape();
        TapeEjected?.Invoke(EventArgs.Empty);
    }
}