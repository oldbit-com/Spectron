using OldBit.Spectron.Emulation.Snapshot;
using OldBit.Spectron.Files.Rzx;
using OldBit.Spectron.Files.Rzx.Blocks;

namespace OldBit.Spectron.Emulation.Rzx;

public sealed class RzxController
{
    private readonly RzxFile _rzxFile;

    private int _currentSnapshotIndex;
    private int _currentFrameIndex;
    private int _inReadCounter;
    private bool _isPlaybackComplete;

    private int _totalFramesCount;
    private int _runningFramesCount;

    private SnapshotBlock? CurrentSnapshot => _rzxFile?.Snapshots[_currentSnapshotIndex];

    internal RecordingFrame? CurrentFrame => CurrentSnapshot?.Recording.Frames.Count > 0
        ? CurrentSnapshot?.Recording.Frames[_currentFrameIndex]
        : null;

    internal bool IsPlaybackActive => !_isPlaybackComplete;

    public Emulator Emulator { get; private set; }

    public event EventHandler? PlaybackCompleted;
    public event EventHandler? PlaybackProgressChanged;

    public RzxController(SnapshotManager snapshotManager, Stream stream)
    {
        var rzxLoader = new RzxLoader(snapshotManager);
        _rzxFile = RzxFile.Load(stream);

        Emulator = rzxLoader.CreateEmulator(_rzxFile);
        Emulator.Bus.AddDevice(new RzxDevice(ReadPort));
        Emulator.RzxController = this;

        _currentSnapshotIndex = 0;
        _totalFramesCount = _rzxFile.Snapshots.Sum(s => s.Recording.Frames.Count);
    }

    internal void NextFrame()
    {
        _inReadCounter = 0;

        if (_currentFrameIndex < CurrentSnapshot?.Recording.FrameCount - 1)
        {
            _currentFrameIndex += 1;
            _runningFramesCount += 1;
        }
        else if (_currentSnapshotIndex < _rzxFile.Snapshots.Count - 1)
        {
            _currentSnapshotIndex += 1;
            _currentFrameIndex = 0;

            RzxLoader.UpdateEmulator(Emulator, _rzxFile, _currentSnapshotIndex);
        }
        else
        {
            _isPlaybackComplete = true;

            PlaybackCompleted?.Invoke(this, EventArgs.Empty);
        }
    }

    private byte ReadPort()
    {
        byte inValue = 0xFF;

        if (CurrentFrame == null)
        {
            return inValue;
        }

        var inValues = CurrentFrame.InValues;

        if (inValues.Length > _inReadCounter)
        {
            inValue = inValues[_inReadCounter];
        }

        _inReadCounter += 1;

        return inValue;
    }
}