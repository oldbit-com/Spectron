using OldBit.Spectron.Emulation.Storage;

namespace OldBit.Spectron.Emulation.Snapshot;

public sealed class SnapshotFile(SnaSnapshot snaSnapshot, SzxSnapshot szxSnapshot, Z80Snapshot z80Snapshot)
{
    public Emulator Load(string fileName)
    {
        var fileType = FileTypeHelper.GetFileType(fileName);

        return fileType switch
        {
            FileType.Sna => snaSnapshot.Load(fileName),
            FileType.Szx => szxSnapshot.Load(fileName),
            FileType.Z80 => z80Snapshot.Load(fileName),
            _ => throw new NotSupportedException($"The file extension '{Path.GetExtension(fileName)}' is not supported.")
        };
    }

    public static void Save(string fileName, Emulator emulator)
    {
        var fileType = FileTypeHelper.GetFileType(fileName);

        switch (fileType)
        {
            case FileType.Sna:
                SnaSnapshot.Save(fileName, emulator);
                break;

            case FileType.Szx:
                SzxSnapshot.Save(fileName, emulator);
                break;

            case FileType.Z80:
                Z80Snapshot.Save(fileName, emulator);
                break;

            default:
                throw new NotSupportedException($"The file extension '{Path.GetExtension(fileName)}' is not supported.");
        }
    }
}