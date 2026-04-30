using OldBit.Spectron.Files.Rzx;
using OldBit.Spectron.Files.Rzx.Blocks;

namespace OldBit.Spectron.Emulation.Rzx;

internal sealed class RzxHandler(RzxFile rzxFile)
{
    private int _currentFrame;
    private int _currentSnapshot;
    private int _inReadCounter;

    private SnapshotBlock CurrentSnapshot => rzxFile.Snapshots[_currentSnapshot];
    private RecordingFrame CurrentFrame => CurrentSnapshot.Recording.Frames[_currentFrame];

    internal int FetchCounter { get; set; }

    internal bool IsFrameComplete => FetchCounter >= CurrentFrame.FetchCounter;

    internal void NextFrame()
    {
        if (_currentFrame < CurrentSnapshot.Recording.FrameCount - 1)
        {
            _currentFrame += 1;
            _inReadCounter = 0;
            FetchCounter = 0;
        }
        else
        {
            // TODO: Handle next snapshot if there is one
            // Or end of RZX
        }
    }

    internal byte ReadPort()
    {
        var inValues = CurrentSnapshot.Recording.Frames[_currentFrame].InValues;

        byte inValue = 0;

        if (inValues.Length > _inReadCounter)
        {
            inValue = inValues[_inReadCounter];
        }

        _inReadCounter += 1;

        return inValue;
    }
}