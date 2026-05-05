using OldBit.Spectron.Files.Szx;

namespace OldBit.Spectron.Emulation.Snapshot.Stores;

public interface ISzxSnapshotStore
{
    void Save(string fileName, SzxFile snapshot);
    SzxFile Load(Stream stream);
}

public class SzxSnapshotStore : ISzxSnapshotStore
{
    public void Save(string fileName, SzxFile snapshot) => snapshot.Save(fileName);
    public SzxFile Load(Stream stream) => SzxFile.Load(stream);
}