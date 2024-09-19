using OldBit.Z80Cpu;
using OldBit.ZX.Files.Tzx;

namespace OldBit.Spectron.Emulation.Tape;

public sealed class TapeManager
{
    private DirectAccess? _directAccess;

    public Cassette Cassette { get; set; } = new();

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

    public void NewTape() => Cassette = new Cassette();

    public void LoadDirect() => _directAccess?.LoadBytes(Cassette);

    public void SaveDirect() => _directAccess?.SaveBytes(Cassette);

    public void InsertTape(string fileName, bool autoPlay = false)
    {
        StopTape();

        Cassette.Load(fileName);
        InsertTape();

        if (autoPlay)
        {
            PlayTape();
        }
    }

    public void InsertTape(TzxFile tzxFile, int currentBlockIndex)
    {
        Cassette.Load(tzxFile, currentBlockIndex);
        InsertTape();
    }

    private void InsertTape()
    {
        TapePlayer?.LoadTape(Cassette);

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