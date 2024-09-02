using OldBit.Spectron.Emulation.File;

namespace OldBit.Spectron.Emulation.Snapshot;

public static class SnapshotLoader
{
    public static Emulator Load(string fileName)
    {
        var fileType = FileTypeHelper.GetFileType(fileName);

        switch (fileType)
        {
            case FileType.Sna:
                return SnaSnapshot.Load(fileName);

            case FileType.Szx:
                return SzxSnapshot.Load(fileName);

            case FileType.Z80:
                return Z80Snapshot.Load(fileName);

            default:
                throw new NotSupportedException($"The file extension '{Path.GetExtension(fileName)}' is not supported.");
        }
    }
}