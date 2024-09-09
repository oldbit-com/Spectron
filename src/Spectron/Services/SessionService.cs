using System;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Snapshot;
using OldBit.Spectron.Emulation.TimeTravel;
using OldBit.Spectron.Settings;
using OldBit.ZXTape.Szx;

namespace OldBit.Spectron.Services;

public class SessionService(
    ApplicationDataService applicationDataService,
    ILogger<SessionService> logger)
{
    private readonly ILogger _logger = logger;

    public void SaveSession(Emulator emulator)
    {
        var settings = new SessionSettings();
        var filePath = ApplicationDataService.GetSettingsFilePath(settings);

        if (!ApplicationDataService.TryCreateDirectory(filePath))
        {
            return;
        };

        var snapshot = SzxSnapshot.CreateSnapshot(emulator, CompressionLevel.NoCompression);
        var snapshotBase64 = ToBase64(snapshot);

        settings.LastSnapshot = snapshotBase64;
        foreach (var entry in TimeMachine.Instance.Entries)
        {
            settings.TimeMachineSnapshots.Add(ToBase64(entry.Snapshot));
        }



        var json = JsonSerializer.Serialize(settings);
    }

    private static string ToBase64(SzxFile szxFile)
    {
        using var memoryStream = new MemoryStream();

        using (var gzipStream = new GZipStream(memoryStream, CompressionLevel.SmallestSize))
        {
            szxFile.Save(gzipStream);
        }

        return Convert.ToBase64String(memoryStream.ToArray());
    }
}