using OldBit.Spectron.Emulation.Files;
using OldBit.Spectron.Emulation.State;
using OldBit.Spectron.Files.Sna;
using OldBit.Spectron.Files.Szx;
using OldBit.Spectron.Files.Z80;

namespace OldBit.Spectron.Emulation.Snapshot;

public sealed class SnapshotManager(
    SnaSnapshot snaSnapshot,
    SzxSnapshot szxSnapshot,
    Z80Snapshot z80Snapshot,
    IStateSnapshotStore stateSnapshotStore,
    StateSnapshotManager stateSnapshotManager)
{
    public Emulator Load(Stream stream, FileType fileType)
    {
        return fileType switch
        {
            FileType.Sna => snaSnapshot.Load(stream),
            FileType.Szx => szxSnapshot.Load(stream),
            FileType.Z80 => z80Snapshot.Load(stream),
            FileType.Spectron => stateSnapshotManager.CreateEmulator(stateSnapshotStore.Load(stream)!),
            _ => throw new NotSupportedException($"The file type '{fileType}' is not supported.")
        };
    }

    public void Save(string filePath, Emulator emulator)
    {
        var fileType = FileTypeResolver.FromPath(filePath);

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
                var stateSnapshot = StateSnapshotManager.CreateSnapshot(emulator);
                stateSnapshotStore.Save(filePath, stateSnapshot);
                break;

            default:
                throw new NotSupportedException($"The file extension '{Path.GetExtension(filePath)}' is not supported.");
        }
    }

    public static void Update(Emulator emulator, Stream stream, FileType fileType)
    {
        switch (fileType)
        {
            case FileType.Sna:
                SnaSnapshot.Update(emulator, SnaFile.Load(stream), false);
                break;

            case FileType.Szx:
                SzxSnapshot.Update(emulator, SzxFile.Load(stream), false);
                break;

            case FileType.Z80:
                Z80Snapshot.Update(emulator, Z80File.Load(stream), false);
                break;
        }
    }
}