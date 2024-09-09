using System.IO.Compression;
using System.Text.Json;
using OldBit.Spectron.Emulation.Snapshot;
using OldBit.Spectron.Emulation.TimeTravel;
using OldBit.ZXTape.Szx;

namespace OldBit.Spectron.Emulation.Session;

public class SessionManager(string directory)
{
    public void SaveSession(Emulator emulator)
    {
        if (!TryCreateSessionDirectory())
        {
            return;
        };

        var snapshot = SzxSnapshot.CreateSnapshot(emulator, CompressionLevel.NoCompression);
        var snapshotBase64 = ToBase64(snapshot);

        var state = new SessionState { LastSnapshot = snapshotBase64, };

        foreach (var entry in TimeMachine.Instance.Entries)
        {
            state.TimeMachineSnapshots.Add(ToBase64(entry.Snapshot));
        }

        var fileName = Path.Combine(directory, $"spectron-state.json");

        var json = JsonSerializer.Serialize(state);
    }

    public Emulator LoadSession()
    {
        throw new NotImplementedException();
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

    private bool TryCreateSessionDirectory()
    {
        try
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            return true;
        }
        catch
        {
           return false;
        }
    }
}