using OldBit.Spectron.Emulation.State;

namespace OldBit.Spectron.Emulation;

public class TimeMachineEntry(DateTimeOffset timestamp, StateSnapshot snapshot)
{
    public DateTimeOffset Timestamp { get; } = timestamp;

    public byte[] SerializedSnapshot { get; } = snapshot.Serialize();

    public StateSnapshot? GetSnapshot() => StateSnapshot.Deserialize(SerializedSnapshot);
}

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

        var snapshot = StateManager.CreateSnapshot(emulator);
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