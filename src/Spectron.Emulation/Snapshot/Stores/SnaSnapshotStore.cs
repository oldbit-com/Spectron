using OldBit.Spectron.Files.Sna;

namespace OldBit.Spectron.Emulation.Snapshot.Stores;

public interface ISnaSnapshotStore
{
    void Save(string fileName, SnaFile snapshot);
    SnaFile Load(Stream stream);
}

public class SnaSnapshotStore : ISnaSnapshotStore
{
    public void Save(string fileName, SnaFile snapshot) => snapshot.Save(fileName);
    public SnaFile Load(Stream stream) => SnaFile.Load(stream);
}