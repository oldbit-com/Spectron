namespace OldBit.Spectron.Emulation.State;

public interface IStateSnapshotStore
{
    void Save(string filePath, StateSnapshot snapshot);
    void Save(Stream stream, StateSnapshot snapshot);
    StateSnapshot? Load(Stream stream);
}