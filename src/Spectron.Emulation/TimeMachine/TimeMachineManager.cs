using System.IO.Compression;
using OldBit.Spectron.Emulation.Snapshot;
using OldBit.ZXTape.Szx;

namespace OldBit.Spectron.Emulation.TimeMachine;

public record TimeMachineState(DateTimeOffset Time, SzxFile Snapshot);

public sealed class TimeMachineManager(TimeSpan interval, TimeSpan duration)
{
    private readonly List<TimeMachineState> _snapshots = [];
    private readonly int _maxSnapshots = (int)(duration.TotalMilliseconds / interval.TotalMilliseconds);
    private DateTimeOffset _lastSnapshotTime;

    public IReadOnlyList<TimeMachineState> Snapshots => _snapshots;

    internal void Update(Emulator emulator)
    {
        var now = DateTimeOffset.UtcNow;
        if (now - _lastSnapshotTime < interval)
        {
            return;
        }

        var snapshot = SzxSnapshot.CreateSnapshot(emulator, CompressionLevel.NoCompression);
        _snapshots.Add(new TimeMachineState(now, snapshot));

        if (_snapshots.Count >_maxSnapshots)
        {
            _snapshots.RemoveAt(0);
        }

        _lastSnapshotTime = now;
    }
}