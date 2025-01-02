using OldBit.Spectron.Emulation.Storage;
using OldBit.Spectron.Files.Szx;

namespace OldBit.Spectron.Emulation.Snapshot;

public sealed class SnapshotLoader(SnaSnapshot snaSnapshot, SzxSnapshot szxSnapshot, Z80Snapshot z80Snapshot)
{
    public Emulator Load(Stream stream, FileType fileType)
    {
        return fileType switch
        {
            FileType.Sna => snaSnapshot.Load(stream),
            FileType.Szx => szxSnapshot.Load(stream),
            FileType.Z80 => z80Snapshot.Load(stream),

            _ => throw new NotSupportedException($"The file type '{fileType}' is not supported.")
        };
    }

    public Emulator Load(SzxFile snapshot) => szxSnapshot.CreateEmulator(snapshot);

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