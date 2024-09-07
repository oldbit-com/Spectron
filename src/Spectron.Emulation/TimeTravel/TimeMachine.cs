using System.IO.Compression;
using OldBit.Spectron.Emulation.Snapshot;
using OldBit.ZXTape.Szx;

namespace OldBit.Spectron.Emulation.TimeTravel;

public record TimeMachineEntry(DateTimeOffset Timestamp, SzxFile Snapshot);

public sealed class TimeMachine(TimeSpan interval, TimeSpan duration)
{
    private readonly List<TimeMachineEntry> _entries = [];
    private readonly int _maxSnapshots = (int)(duration.TotalMilliseconds / interval.TotalMilliseconds);
    private DateTimeOffset _lastEntryTime;

    public static TimeMachine Instance { get; } = new(TimeSpan.FromSeconds(1), TimeSpan.FromMinutes(5));

    public IReadOnlyList<TimeMachineEntry> Entries => _entries;

    internal void Update(Emulator emulator)
    {
        var now = DateTimeOffset.UtcNow;
        if (now - _lastEntryTime < interval)
        {
            return;
        }

        var snapshot = SzxSnapshot.CreateSnapshot(emulator, CompressionLevel.NoCompression);
        _entries.Add(new TimeMachineEntry(now, snapshot));

        if (_entries.Count >_maxSnapshots)
        {
            _entries.RemoveAt(0);
        }

        _lastEntryTime = now;
    }
}