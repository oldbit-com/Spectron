namespace OldBit.Spectral.Emulation.Tape;

public sealed class TapeManager
{
    internal TapePlayer TapePlayer { get; }
    internal FileLoader FileLoader { get; }
    internal FastFileLoader FastFileLoader { get; }

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
        FileLoader = new FileLoader();
        FastFileLoader = new FastFileLoader(emulator.Cpu, emulator.Memory, TapePlayer);
    }

    public void LoadAndRun(string fileName)
    {
        var fileType = FileTypeHelper.GetFileType(fileName);
        if (fileType.IsSnapshot())
        {
            var emulator = FileLoader.LoadSnapshot(fileName, fileType);


            return;
        }

        if (!TryInsertTape(fileName))
        {
            return;
        }


        PlayTape();
        // TODO: Simulate LOAD ""
    }

    public bool TryInsertTape(string fileName)
    {
        StopTape();

        if (!FileLoader.TryLoadTape(fileName, out var tzxFile))
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

    private static bool IsSnaFile(string fileName) => Path.GetExtension(fileName).Equals(".sna", StringComparison.InvariantCultureIgnoreCase);
}