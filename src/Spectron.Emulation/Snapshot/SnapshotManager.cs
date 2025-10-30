using OldBit.Spectron.Emulation.Files;
using OldBit.Spectron.Emulation.State;

namespace OldBit.Spectron.Emulation.Snapshot;

public sealed class SnapshotManager(
    SnaSnapshot snaSnapshot,
    SzxSnapshot szxSnapshot,
    Z80Snapshot z80Snapshot,
    StateManager stateManager)
{
    public Emulator Load(Stream stream, FileType fileType)
    {
        return fileType switch
        {
            FileType.Sna => snaSnapshot.Load(stream),
            FileType.Szx => szxSnapshot.Load(stream),
            FileType.Z80 => z80Snapshot.Load(stream),
            FileType.Spectron => stateManager.CreateEmulator(StateSnapshot.Load(stream)!),
            _ => throw new NotSupportedException($"The file type '{fileType}' is not supported.")
        };
    }

    public static void Save(string filePath, Emulator emulator)
    {
        var fileType = FileTypes.GetFileType(filePath);

        switch (fileType)
        {
            case FileType.Sna:
                SnaSnapshot.Save(filePath, emulator);
                break;

            case FileType.Szx:
                SzxSnapshot.Save(filePath, emulator);
                break;

            case FileType.Z80:
                Z80Snapshot.Save(filePath, emulator);
                break;

            case FileType.Spectron:
                StateManager.CreateSnapshot(emulator).Save(filePath);
                break;

            default:
                throw new NotSupportedException($"The file extension '{Path.GetExtension(filePath)}' is not supported.");
        }
    }
}