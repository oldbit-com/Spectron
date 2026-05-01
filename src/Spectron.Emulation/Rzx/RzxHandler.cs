using OldBit.Spectron.Files.Rzx;
using OldBit.Spectron.Files.Rzx.Blocks;

namespace OldBit.Spectron.Emulation.Rzx;

public sealed class RzxHandler(RzxController rzxController)
{
    private RzxFile? _rzxFile;
    private int _currentFrameIndex;
    private int _currentSnapshotIndex;
    private int _inReadCounter;

    private SnapshotBlock? CurrentSnapshot => _rzxFile?.Snapshots[_currentSnapshotIndex];

    internal RecordingFrame? CurrentFrame => CurrentSnapshot?.Recording.Frames.Count > 0
        ? CurrentSnapshot?.Recording.Frames[_currentFrameIndex]
        : null;

    internal bool IsPlaybackComplete { get; private set; }

    internal bool IsPlaybackActive => _rzxFile != null && !IsPlaybackComplete;

    public void Load(RzxFile rzxFile, int currentSnapshot)
    {
        _rzxFile = rzxFile;
        _currentSnapshotIndex = currentSnapshot;
    }

    internal void NextFrame()
    {
        if (_rzxFile == null)
        {
            return;
        }

        _inReadCounter = 0;

        if (_currentFrameIndex < CurrentSnapshot?.Recording.FrameCount - 1)
        {
            _currentFrameIndex += 1;
        }
        else if (_currentSnapshotIndex < _rzxFile.Snapshots.Count - 1)
        {
            _currentSnapshotIndex += 1;
            _currentFrameIndex = 0;

            rzxController.NextSnapshot(_currentSnapshotIndex);
        }
        else
        {
            // TODO: Handle playback complete
            IsPlaybackComplete = true;
        }
    }

    internal byte ReadPort()
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

    public void Reset()
    {
        _rzxFile = null;

        _currentFrameIndex = 0;
        _currentSnapshotIndex = 0;
        _inReadCounter = 0;

        IsPlaybackComplete = false;
    }
}