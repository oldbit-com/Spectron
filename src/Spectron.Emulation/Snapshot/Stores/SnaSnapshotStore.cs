using OldBit.Spectron.Files.Sna;

namespace OldBit.Spectron.Emulation.Snapshot.Stores;

public interface ISnaSnapshotStore
{
    void Save(string fileName, SnaFile snapshot);
    void Save(Stream stream, SnaFile snapshot);
    SnaFile Load(Stream stream);
}

public class SnaSnapshotStore : ISnaSnapshotStore
{
    public void Save(string fileName, SnaFile snapshot) => snapshot.Save(fileName);
    public void Save(Stream stream, SnaFile snapshot) => snapshot.Save(stream);
    public SnaFile Load(Stream stream) => SnaFile.Load(stream);
}