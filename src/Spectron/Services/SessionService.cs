using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Snapshot;
using OldBit.Spectron.Settings;
using OldBit.ZXTape.Szx;

namespace OldBit.Spectron.Services;

public class SessionService(
    ApplicationDataService applicationDataService,
    TimeMachine timeMachine,
    ILogger<SessionService> logger)
{
    private readonly ILogger _logger = logger;

    public async Task SaveAsync(Emulator? emulator)
    {
        if (emulator is null)
        {
            return;
        }

        var session = new SessionSettings();

        var snapshot = SzxSnapshot.CreateSnapshot(emulator, CompressionLevel.NoCompression);
        var snapshotBase64 = SnapshotToBase64(snapshot);

        session.LastSnapshot = snapshotBase64;
        foreach (var entry in timeMachine.Entries)
        {
            snapshotBase64 = SnapshotToBase64(entry.Snapshot);
            session.TimeMachineSnapshots.Add(new TimeMachineSnapshot(snapshotBase64, entry.Timestamp));
        }

        await applicationDataService.SaveAsync(session);
    }

    public async Task<SzxFile?> LoadAsync()
    {
        var session = await applicationDataService.LoadAsync<SessionSettings>();

        try
        {
            timeMachine.Entries.Clear();

            foreach (var timeMachineSnapshot in session.TimeMachineSnapshots)
            {
                var snapshot = SnapshotFromBase64(timeMachineSnapshot.Snapshot);
                timeMachine.Entries.Add(new TimeMachineEntry(timeMachineSnapshot.Timestamp, snapshot));
            }

            if (session.LastSnapshot != null)
            {
                return SnapshotFromBase64(session.LastSnapshot);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load session");
        }

        return null;
    }

    private static string SnapshotToBase64(SzxFile szxFile)
    {
        using var memoryStream = new MemoryStream();

        using (var gzipStream = new GZipStream(memoryStream, CompressionLevel.SmallestSize))
        {
            szxFile.Save(gzipStream);
        }

        return Convert.ToBase64String(memoryStream.ToArray());
    }

    private static SzxFile SnapshotFromBase64(string base64)
    {
        var bytes = Convert.FromBase64String(base64);

        using var memoryStream = new MemoryStream(bytes);
        using var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress);

        return SzxFile.Load(gzipStream);
    }
}