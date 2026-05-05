namespace OldBit.Spectron.Emulation.State;

public interface IStateSnapshotStore
{
    void Save(string filePath, StateSnapshot snapshot);
    StateSnapshot? Load(string filePath);
    StateSnapshot? Load(Stream stream);
}