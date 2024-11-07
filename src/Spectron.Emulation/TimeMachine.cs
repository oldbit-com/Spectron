using System.IO.Compression;
using OldBit.Spectron.Emulation.Snapshot;
using OldBit.ZX.Files.Szx;

namespace OldBit.Spectron.Emulation;

public record TimeMachineEntry(DateTimeOffset Timestamp, SzxFile Snapshot);

public sealed class TimeMachine
{
    public bool IsEnabled { get; set; }
    public TimeSpan SnapshotInterval { get; set; } = TimeSpan.FromSeconds(1);
    public TimeSpan MaxDuration { get; set; } = TimeSpan.FromMinutes(1);
    public List<TimeMachineEntry> Entries { get; } = [];

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
        Entries.Add(new TimeMachineEntry(now, snapshot));

        while (Entries.Count > MaxSnapshots)
        {
            Entries.RemoveAt(0);
        }
    }

    private int MaxSnapshots => (int)(MaxDuration.TotalMilliseconds / SnapshotInterval.TotalMilliseconds);
}