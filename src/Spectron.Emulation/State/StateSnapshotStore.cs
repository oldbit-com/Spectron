using MemoryPack;

namespace OldBit.Spectron.Emulation.State;

public class StateSnapshotStore : IStateSnapshotStore
{
    public void Save(string filePath, StateSnapshot snapshot)
    {
        using var stream = File.Create(filePath);
        var data = MemoryPackSerializer.Serialize(snapshot);

        stream.Write(data);
    }

    public StateSnapshot? Load(string filePath)
    {
        var data = File.ReadAllBytes(filePath);

        return MemoryPackSerializer.Deserialize<StateSnapshot>(data);
    }

    public StateSnapshot? Load(Stream stream)
    {
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);

        return MemoryPackSerializer.Deserialize<StateSnapshot>(memoryStream.ToArray());
    }
}