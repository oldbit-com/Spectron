using System.IO.Compression;
using OldBit.Spectron.Emulation.Snapshot;
using OldBit.Spectron.Files.Szx;

namespace OldBit.Spectron.Emulation;

public record TimeMachineEntry(DateTimeOffset Timestamp, SzxFile Snapshot);

public sealed class TimeMachine
{
    private readonly List<TimeMachineEntry> _entries = [];

    public bool IsEnabled { get; set; }
    public TimeSpan SnapshotInterval { get; set; } = TimeSpan.FromSeconds(1);
    public TimeSpan MaxDuration { get; set; } = TimeSpan.FromMinutes(1);
    public IReadOnlyList<TimeMachineEntry> Entries => _entries;

    internal void AddEntry(Emulator emulator)
    {
        if (!IsEnabled)
        {
            return;
        }

        var lastEntryTime = Entries.LastOrDefault()?.Timestamp ?? DateTimeOffset.MinValue;
        var now = DateTimeOffset.UtcNow;

        if (now - lastEntryTime < SnapshotInterval)
        {
            return;
        }

        var snapshot = SzxSnapshot.CreateSnapshot(emulator, CompressionLevel.NoCompression);
        _entries.Add(new TimeMachineEntry(now, snapshot));

        while (Entries.Count > MaxSnapshots)
        {
            _entries.RemoveAt(0);
        }
    }

    public void Add(TimeMachineEntry entry) => _entries.Add(entry);

    public void Clear() => _entries.Clear();

    private int MaxSnapshots => (int)(MaxDuration.TotalMilliseconds / SnapshotInterval.TotalMilliseconds);
}