using System.IO.Compression;
using OldBit.Spectron.Emulation.Snapshot;
using OldBit.ZXTape.Szx;

namespace OldBit.Spectron.Emulation.TimeMachine;

internal record TimeMachineState(DateTimeOffset Time, SzxFile Snapshot);

internal sealed class TimeMachineManager(TimeSpan interval, TimeSpan duration)
{
    private readonly int _maxSnapshots = (int)(duration.TotalMilliseconds / interval.TotalMilliseconds);
    private DateTimeOffset _lastSnapshotTime;

    internal List<TimeMachineState> Snapshots { get; } = [];

    internal void Update(Emulator emulator)
    {
        var now = DateTimeOffset.UtcNow;
        if (now - _lastSnapshotTime < interval)
        {
            return;
        }

        var snapshot = SzxSnapshot.CreateSnapshot(emulator, CompressionLevel.NoCompression);
        Snapshots.Add(new TimeMachineState(now, snapshot));

        if (Snapshots.Count >_maxSnapshots)
        {
            Snapshots.RemoveAt(0);
        }

        _lastSnapshotTime = now;
    }
}