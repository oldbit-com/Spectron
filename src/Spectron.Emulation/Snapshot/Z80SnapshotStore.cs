using OldBit.Spectron.Files.Z80;

namespace OldBit.Spectron.Emulation.Snapshot;

public interface IZ80SnapshotStore
{
    void Save(string fileName, Z80File snapshot);
    Z80File Load(Stream stream);
}

public class Z80SnapshotStore : IZ80SnapshotStore
{
    public void Save(string fileName, Z80File snapshot) => snapshot.Save(fileName);
    public Z80File Load(Stream stream) => Z80File.Load(stream);
}