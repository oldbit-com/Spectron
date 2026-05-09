namespace OldBit.Spectron.Emulation.State;

public interface IStateSnapshotStore
{
    void Save(string filePath, StateSnapshot snapshot);
    StateSnapshot? Load(Stream stream);
}