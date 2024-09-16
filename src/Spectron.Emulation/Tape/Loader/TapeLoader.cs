using System.Reflection;
using OldBit.Spectron.Emulation.Snapshot;
using OldBit.ZX.Files.Szx;

namespace OldBit.Spectron.Emulation.Tape.Loader;

public sealed class TapeLoader(SzxSnapshot szxSnapshot)
{
    // Below resources are simply snapshots created when LOAD "" has been executed from BASIC.
    // This way we can just load these snapshots and it will be in state like LOAD "" has been just executed.
    private const string Loader16ResourceName = "OldBit.Spectron.Emulation.Tape.Loader.Files.16.szx";
    private const string Loader48ResourceName = "OldBit.Spectron.Emulation.Tape.Loader.Files.48.szx";
    private const string Loader128ResourceName = "OldBit.Spectron.Emulation.Tape.Loader.Files.128.szx";

    public Emulator SimulateLoad(ComputerType computerType)
    {
        var romResourceName = computerType switch
        {
            ComputerType.Spectrum16K => Loader16ResourceName,
            ComputerType.Spectrum48K => Loader48ResourceName,
            ComputerType.Spectrum128K => Loader128ResourceName,
            _ => throw new NotSupportedException($"The computer type '{computerType}' is not supported.")
        };

        var szx = GetSnapshot(romResourceName);

        return szxSnapshot.CreateEmulator(szx);
    }

    private static SzxFile GetSnapshot(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(resourceName)!;

        return SzxFile.Load(stream);
    }
}