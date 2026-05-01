using OldBit.Spectron.Emulation.Snapshot;
using OldBit.Spectron.Files.Rzx;

namespace OldBit.Spectron.Emulation.Rzx;

public sealed class RzxController(SnapshotManager snapshotManager)
{
    private readonly RzxLoader _rzxLoader = new(snapshotManager);

    private RzxHandler? _rzxHandler;
    private RzxFile? _rzxFile;
    private int _currentSnapshotIndex;
    private Emulator? _emulator;

    public Emulator Load(Stream stream)
    {
        _rzxFile = RzxFile.Load(stream);

        _rzxHandler = new RzxHandler(this);
        _rzxHandler.Load(_rzxFile, _currentSnapshotIndex);

        _emulator = _rzxLoader.CreateEmulator(_rzxFile);
        _emulator.Bus.AddDevice(new RzxDevice(_rzxHandler));
        _emulator.RzxHandler = _rzxHandler;

        return _emulator;
    }

    internal void NextSnapshot(int snapshotIndex)
    {
        _currentSnapshotIndex = snapshotIndex;

        RzxLoader.UpdateEmulator(_emulator!, _rzxFile!, _currentSnapshotIndex);
    }
}