using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.State;
using OldBit.Spectron.Settings;

namespace OldBit.Spectron.Services;

public class SessionService(
    ApplicationDataService applicationDataService,
    TimeMachine timeMachine,
    ILogger<SessionService> logger)
{
    private readonly ILogger _logger = logger;

    public async Task SaveAsync(Emulator? emulator, ResumeSettings resumeSettings)
    {
        if (emulator is null)
        {
            return;
        }

        var session = new SessionSettings();

        if (resumeSettings.IsResumeEnabled)
        {
            var snapshot = StateManager.CreateSnapshot(emulator);

            session.LastSnapshot = EmulatorStateToBase64(snapshot);
        }

        if (timeMachine.IsEnabled && resumeSettings.ShouldIncludeTimeMachine)
        {
            foreach (var entry in timeMachine.Entries)
            {
                var snapshotBase64 = EmulatorStateToBase64(entry.Snapshot);
                session.TimeMachineSnapshots.Add(new TimeMachineSnapshot(snapshotBase64, entry.Timestamp));
            }
        }

        await applicationDataService.SaveAsync(session);
    }

    public async Task<StateSnapshot?> LoadAsync()
    {
        var session = await applicationDataService.LoadAsync<SessionSettings>();

        try
        {
            timeMachine.Clear();

            foreach (var timeMachineSnapshot in session.TimeMachineSnapshots)
            {
                var snapshot = EmulatorStateFromBase64(timeMachineSnapshot.Snapshot);

                if (snapshot != null)
                {
                    timeMachine.Add(new TimeMachineEntry(timeMachineSnapshot.Timestamp, snapshot));
                }
            }

            if (session.LastSnapshot != null)
            {
                return EmulatorStateFromBase64(session.LastSnapshot);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load emulator state");
        }

        return null;
    }

    private static string EmulatorStateToBase64(StateSnapshot snapshot)
    {
        var serialized = snapshot.Serialize();

        using var memoryStream = new MemoryStream();
        using (var gzipStream = new GZipStream(memoryStream, CompressionLevel.Fastest))
        {
            gzipStream.Write(serialized);
        }

        return Convert.ToBase64String(memoryStream.ToArray());
    }

    private static StateSnapshot? EmulatorStateFromBase64(string base64)
    {
        var bytes = Convert.FromBase64String(base64);

        using var memoryStream = new MemoryStream(bytes);
        using var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress);

        return StateSnapshot.Load(gzipStream);
    }
}