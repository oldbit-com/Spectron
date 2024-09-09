using OldBit.Spectron.Emulation.Storage;

namespace OldBit.Spectron.Emulation.Snapshot;

public static class SnapshotFile
{
    public static Emulator Load(string fileName)
    {
        var fileType = FileTypeHelper.GetFileType(fileName);

        return fileType switch
        {
            FileType.Sna => SnaSnapshot.Load(fileName),
            FileType.Szx => SzxSnapshot.Load(fileName),
            FileType.Z80 => Z80Snapshot.Load(fileName),
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