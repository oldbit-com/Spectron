using OldBit.Spectron.Emulation.State;

namespace OldBit.Spectron.Emulation.TimeTravel;

public class TimeMachineEntry(DateTimeOffset timestamp, StateSnapshot snapshot)
{
    public DateTimeOffset Timestamp { get; } = timestamp;

    public byte[] SerializedSnapshot { get; } = snapshot.Serialize();

    public StateSnapshot? GetSnapshot() => StateSnapshot.Deserialize(SerializedSnapshot);
}