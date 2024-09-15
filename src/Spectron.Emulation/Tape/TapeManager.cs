using System.Diagnostics.CodeAnalysis;
using OldBit.Spectron.Emulation.Storage;
using OldBit.ZX.Files.Extensions;
using OldBit.ZX.Files.Tap;
using OldBit.ZX.Files.Tzx;

namespace OldBit.Spectron.Emulation.Tape;

public sealed class TapeManager
{
    private readonly InstantLoader _instantLoader;

    public TapeFile Tape { get; set; } = new();

    internal TapePlayer TapePlayer { get; }

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
        _instantLoader = new InstantLoader(emulator.Cpu, emulator.Memory);
    }

    public void NewTape()
    {
    }

    public void InsertTape(string fileName, bool autoPlay = false)
    {
        StopTape();

        Tape.Load(fileName);
        TapePlayer.LoadTape(Tape);

        TapeInserted?.Invoke(EventArgs.Empty);

        if (autoPlay)
        {
            PlayTape();
        }
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

    internal void InstantLoad() => _instantLoader.LoadBytes(Tape);
}