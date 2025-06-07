using OldBit.Spectron.Emulation.Files;
using OldBit.Z80Cpu;
using OldBit.Spectron.Files.Tzx;

namespace OldBit.Spectron.Emulation.Tape;

public class TapeStateEventArgs(TapeAction action) : EventArgs
{
    public TapeAction Action { get; } = action;
}

public sealed class TapeManager
{
    private DirectAccess? _directAccess;

    public Cassette Cassette { get; private set; } = new();

    public bool IsTapeLoaded { get; private set; }
    public bool IsTapeSaveEnabled { get; set; }
    public TapeSpeed TapeSaveSpeed { get; set; }
    public double BlockReadProgressPercentage => CassettePlayer?.BlockReadProgressPercentage ?? 0;

    internal CassettePlayer? CassettePlayer { get; private set; }

    public delegate void TapeStateChangedEvent(TapeStateEventArgs e);
    public event TapeStateChangedEvent? TapeStateChanged;

    internal void Attach(Z80 cpu, IMemory memory, HardwareSettings hardware)
    {
        CassettePlayer = new CassettePlayer(Cassette, cpu.Clock, hardware);
        _directAccess = new DirectAccess(cpu, memory);
    }

    public bool IsPlaying => CassettePlayer?.IsPlaying ?? false;

    public void NewTape()
    {
        Cassette = new Cassette();

        TapeStateChanged?.Invoke(new TapeStateEventArgs(TapeAction.TapeInserted));

        IsTapeLoaded = true;
    }

    public void FastLoad() => _directAccess?.FastLoad(Cassette);

    public void FastSave()
    {
        if (!IsTapeSaveEnabled)
        {
            return;
        }

        if (!IsTapeLoaded)
        {
            NewTape();
        }

        _directAccess?.FastSave(Cassette, TapeSaveSpeed);
    }

    public void InsertTape(Stream stream, FileType fileType, bool autoPlay = false)
    {
        StopTape();

        Cassette.Load(stream, fileType);
        InsertTape();

        if (autoPlay)
        {
            PlayTape();
        }
    }

    public void InsertTape(string fileName, bool autoPlay = false)
    {
        var fileType = FileTypes.GetFileType(fileName);
        var stream = File.OpenRead(fileName);

        InsertTape(stream, fileType, autoPlay);
    }

    public void InsertTape(TzxFile tzxFile, int currentBlockIndex)
    {
        Cassette.Load(tzxFile, currentBlockIndex);
        InsertTape();
    }

    private void InsertTape()
    {
        CassettePlayer?.LoadTape(Cassette);
        TapeStateChanged?.Invoke(new TapeStateEventArgs(TapeAction.TapeInserted));

        IsTapeLoaded = true;
    }

    public void StopTape()
    {
        CassettePlayer?.Stop();
        TapeStateChanged?.Invoke(new TapeStateEventArgs(TapeAction.TapeStopped));
    }

    public void PlayTape()
    {
        CassettePlayer?.Play();
        TapeStateChanged?.Invoke(new TapeStateEventArgs(TapeAction.TapeStarted));
   }

    public void EjectTape()
    {
        StopTape();
        TapeStateChanged?.Invoke(new TapeStateEventArgs(TapeAction.TapeEjected));

        Cassette = new Cassette();
        IsTapeLoaded = false;
    }

    public void RewindTape() => CassettePlayer?.Rewind();
}