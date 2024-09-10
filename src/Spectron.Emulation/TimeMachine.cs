using System.IO.Compression;
using OldBit.Spectron.Emulation.Snapshot;
using OldBit.ZXTape.Szx;

namespace OldBit.Spectron.Emulation;

public record TimeMachineEntry(DateTimeOffset Timestamp, SzxFile Snapshot);

public sealed class TimeMachine
{
    private readonly List<TimeMachineEntry> _entries = [];
    private readonly int _maxSnapshots = (int)(DefaultDuration.TotalMilliseconds / DefaultInterval.TotalMilliseconds);

    public static TimeSpan DefaultInterval { get; } = TimeSpan.FromSeconds(1);
    public static TimeSpan DefaultDuration { get; } = TimeSpan.FromMinutes(5);
    public IReadOnlyList<TimeMachineEntry> Entries => _entries;

    internal void AddEntry(Emulator emulator)
    {
        var lastEntryTime = _entries.LastOrDefault()?.Timestamp ?? DateTimeOffset.MinValue;
        var now = DateTimeOffset.UtcNow;
        if (now - lastEntryTime < DefaultInterval)
        {
            return;
        }

        var snapshot = SzxSnapshot.CreateSnapshot(emulator, CompressionLevel.NoCompression);
        _entries.Add(new TimeMachineEntry(now, snapshot));

        if (_entries.Count >_maxSnapshots)
        {
            _entries.RemoveAt(0);
        }
    }
}