using OldBit.Spectron.Emulation.Files;
using OldBit.Spectron.Emulation.Snapshot;
using OldBit.Spectron.Files.Rzx;

namespace OldBit.Spectron.Emulation.Rzx;

public class RzxPlayer(SnapshotManager snapshotManager)
{
    private RzxFile? _rzxFile;

    public Emulator Load(Stream stream)
    {
        _rzxFile = RzxFile.Load(stream);

        if (_rzxFile.Snapshots.Count == 0)
        {
            throw new NotSupportedException("RZX file must contain at least one snapshot");
        }

        var snapshot = _rzxFile.Snapshots.First();
        using var memoryStream = new MemoryStream(snapshot.Data ?? []);

        Emulator emulator;

        switch (snapshot.Extension.ToLowerInvariant())
        {
            case "sna":
                emulator = snapshotManager.Load(memoryStream, FileType.Sna);
                break;

            case "szx":
                emulator = snapshotManager.Load(memoryStream, FileType.Szx);
                break;

            case "z80":
                emulator = snapshotManager.Load(memoryStream, FileType.Z80);
                break;

            default:
                throw new NotSupportedException($"Unsupported RZX snapshot format: {snapshot.Extension}");
        }

        emulator.PlayRzx(_rzxFile);

        return emulator;
    }
}