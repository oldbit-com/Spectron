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

    private readonly int _totalFramesCount;
    private int _runningFramesCount;
    private double _playbackProgress;

    private SnapshotBlock CurrentSnapshot => _rzxFile.Snapshots[_currentSnapshotIndex];

    internal RecordingFrame? CurrentFrame => CurrentSnapshot.Recording.Frames.Count > 0
        ? CurrentSnapshot.Recording.Frames[_currentFrameIndex]
        : null;

    internal bool IsPlaybackActive => !_isPlaybackComplete;

    public Emulator Emulator { get; }

    public event EventHandler? PlaybackCompleted;
    public event EventHandler<RzxProgressChangedEventArgs>? PlaybackProgressChanged;

    public RzxController(SnapshotManager snapshotManager, Stream stream)
    {
        var rzxLoader = new RzxLoader(snapshotManager);
        _rzxFile = RzxFile.Load(stream);

        Emulator = rzxLoader.CreateEmulator(_rzxFile);
        Emulator.Bus.AddDevice(new RzxDevice(ReadPort));
        Emulator.RzxController = this;

        // Disable automatic interrupts trigger during playback
        Emulator.Cpu.Clock.InterruptDuration = 4;

        _currentSnapshotIndex = 0;
        _totalFramesCount = _rzxFile.Snapshots.Sum(s => s.Recording.Frames.Count);
    }

    internal void NextFrame()
    {
        // Trigger INT befor the next frame
        Emulator.Cpu.TriggerInt(0xFF);

        _inReadCounter = 0;

        while (_currentFrameIndex < CurrentSnapshot.Recording.Frames.Count - 1)
        {
            // Handle next frame - skip if fetch counter is not zero
            _currentFrameIndex += 1;
            _runningFramesCount += 1;

            if (CurrentFrame?.FetchCounter == 0)
            {
                continue;
            }

            var playbackProgress = Math.Round((double)_runningFramesCount / _totalFramesCount, 4);

            if (playbackProgress > _playbackProgress)
            {
                _playbackProgress = playbackProgress;
                PlaybackProgressChanged?.Invoke(this, new RzxProgressChangedEventArgs(playbackProgress));
            }

            return;
        }

        if (_currentSnapshotIndex < _rzxFile.Snapshots.Count - 1)
        {
            // Handle next snapshot
            _currentSnapshotIndex += 1;
            _currentFrameIndex = 0;

            RzxLoader.UpdateEmulator(Emulator, _rzxFile, _currentSnapshotIndex);
        }
        else
        {
            // Handle end of playback
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