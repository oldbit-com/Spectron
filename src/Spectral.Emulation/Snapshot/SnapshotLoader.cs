using OldBit.Spectral.Emulation.File;

namespace OldBit.Spectral.Emulation.Snapshot;

public static class SnapshotLoader
{
    public static Emulator Load(string fileName)
    {
        var fileType = FileTypeHelper.GetFileType(fileName);

        switch (fileType)
        {
            case FileType.Sna:
                return Sna.Load(fileName);

            case FileType.Szx:
                return Szx.Load(fileName);

            case FileType.Z80:
                return Z80.Load(fileName);

            default:
                throw new NotSupportedException($"The file extension '{Path.GetExtension(fileName)}' is not supported.");
        }
    }
}