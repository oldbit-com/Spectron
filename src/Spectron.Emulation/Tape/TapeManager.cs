using OldBit.Spectron.Emulation.Files;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Z80Cpu;
using OldBit.Spectron.Files.Tzx;

namespace OldBit.Spectron.Emulation.Tape;

public class TapeChangedEventArgs(TapeAction action) : EventArgs
{
    public TapeAction Action { get; } = action;
}

public sealed class TapeManager
{
    private readonly LoaderDetector _loaderDetector = new();
    private DirectAccess? _directAccess;
    private bool _isFastLoading;

    public Cassette Cassette { get; private set; } = new();

    public bool IsTapeLoaded { get; private set; }
    public bool IsTapeSaveEnabled { get; set; }
    public bool IsCustomLoaderDetectionEnabled  { get; set; }

    public TapeSpeed TapeLoadSpeed { get; set; }
    public TapeSpeed TapeSaveSpeed { get; set; }

    public double BlockReadProgressPercentage => CassettePlayer?.BlockReadProgressPercentage ?? 0;

    internal CassettePlayer? CassettePlayer { get; private set; }

    public bool IsPlaying => CassettePlayer?.IsPlaying ?? false;

    public delegate void TapeChangedEvent(TapeChangedEventArgs e);
    public event TapeChangedEvent? TapeChanged;

    internal void Attach(Z80 cpu, IMemory memory, HardwareSettings hardware)
    {
        CassettePlayer = new CassettePlayer(Cassette, cpu.Clock, hardware);
        _directAccess = new DirectAccess(cpu, memory);
    }

    internal void DetectLoader(int ticks, Word pc)
    {
        if (!IsCustomLoaderDetectionEnabled || IsPlaying ||
            !_isFastLoading || Cassette.IsEndOfTape || pc < RomRoutines.LD_EDGE_2)
        {
            return;
        }

        if (_loaderDetector.Process(ticks))
        {
            _isFastLoading = false;
            PlayTape();
        }
    }

    public void NewTape()
    {
        Cassette = new Cassette();

        TapeChanged?.Invoke(new TapeChangedEventArgs(TapeAction.Inserted));

        IsTapeLoaded = true;
    }

    public void FastLoad()
    {
        _directAccess?.FastLoad(Cassette);
        _isFastLoading = true;
    }

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

        _directAccess?.FastSave(Cassette);
    }

    public void InsertTape(Stream stream, FileType fileType, bool autoPlay = false)
    {
        StopTape();

        Cassette.SetContent(stream, fileType);
        InsertTape();

        if (autoPlay)
        {
            PlayTape();
        }
    }

    public void InsertTape(string fileName)
    {
        var fileType = FileTypes.GetFileType(fileName);
        var stream = File.OpenRead(fileName);

        InsertTape(stream, fileType);
    }

    public void InsertTape(TzxFile tzxFile, int currentBlockIndex)
    {
        Cassette.SetContent(tzxFile, currentBlockIndex);
        InsertTape();
    }

    private void InsertTape()
    {
        CassettePlayer?.LoadTape(Cassette);
        TapeChanged?.Invoke(new TapeChangedEventArgs(TapeAction.Inserted));

        IsTapeLoaded = true;
    }

    public void StopTape()
    {
        CassettePlayer?.Stop();
        TapeChanged?.Invoke(new TapeChangedEventArgs(TapeAction.Stopped));
    }

    public void PlayTape()
    {
        CassettePlayer?.Play();
        TapeChanged?.Invoke(new TapeChangedEventArgs(TapeAction.Started));
   }

    public void EjectTape()
    {
        StopTape();
        TapeChanged?.Invoke(new TapeChangedEventArgs(TapeAction.Ejected));

        Cassette = new Cassette();
        IsTapeLoaded = false;
    }

    public void RewindTape() => CassettePlayer?.Rewind();
}