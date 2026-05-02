using OldBit.Spectron.Emulation.Files;
using OldBit.Spectron.Emulation.Snapshot;
using OldBit.Spectron.Files.Rzx;

namespace OldBit.Spectron.Emulation.Rzx;

internal sealed class RzxLoader(SnapshotManager snapshotManager)
{
    internal Emulator CreateEmulator(RzxFile rzxFile)
    {
        if (rzxFile.Snapshots.Count == 0)
        {
            throw new NotSupportedException("RZX file must contain at least one snapshot");
        }

        var snapshot = rzxFile.Snapshots[0];

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

        return emulator;
    }

    internal static void UpdateEmulator(Emulator emulator, RzxFile rzxFile, int currentSnapshot)
    {
        var snapshot = rzxFile.Snapshots[currentSnapshot];

        using var memoryStream = new MemoryStream(snapshot.Data ?? []);

        switch (snapshot.Extension.ToLowerInvariant())
        {
            case "sna":
                SnapshotManager.Update(emulator, memoryStream, FileType.Sna);
                break;

            case "szx":
                SnapshotManager.Update(emulator, memoryStream, FileType.Szx);
                break;

            case "z80":
                SnapshotManager.Update(emulator, memoryStream, FileType.Z80);
                break;

            default:
                throw new NotSupportedException($"Unsupported RZX snapshot format: {snapshot.Extension}");
        }
    }
}