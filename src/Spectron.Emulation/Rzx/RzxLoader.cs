using OldBit.Spectron.Emulation.Files;
using OldBit.Spectron.Emulation.Snapshot;
using OldBit.Spectron.Files.Rzx;

namespace OldBit.Spectron.Emulation.Rzx;

internal sealed class RzxLoader(SnapshotManager snapshotManager)
{
    private const string SnaExtension = "sna";
    private const string SzxExtension = "szx";
    private const string Z80Extension = "z80";

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
            case SnaExtension:
                emulator = snapshotManager.Load(memoryStream, FileType.Sna);
                break;

            case SzxExtension:
                emulator = snapshotManager.Load(memoryStream, FileType.Szx);
                break;

            case Z80Extension:
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
            case SnaExtension:
                SnapshotManager.Update(emulator, memoryStream, FileType.Sna);
                break;

            case SzxExtension:
                SnapshotManager.Update(emulator, memoryStream, FileType.Szx);
                break;

            case Z80Extension:
                SnapshotManager.Update(emulator, memoryStream, FileType.Z80);
                break;

            default:
                throw new NotSupportedException($"Unsupported RZX snapshot format: {snapshot.Extension}");
        }
    }
}