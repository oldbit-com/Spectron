using System.Reflection;
using OldBit.Spectron.Emulation.Snapshot;
using OldBit.Spectron.Files.Szx;

namespace OldBit.Spectron.Emulation.Tape.Loader;

/// <summary>
/// The loader is responsible for entering the LOAD "" command into the emulator.
/// Basically it loads a snapshot that it is the state after the LOAD "" command has been entered.
/// </summary>
public sealed class Loader(SzxSnapshot szxSnapshot)
{
    private const string Loader16ResourceName = "OldBit.Spectron.Emulation.Tape.Loader.Files.16.szx";
    private const string Loader48ResourceName = "OldBit.Spectron.Emulation.Tape.Loader.Files.48.szx";
    private const string Loader128ResourceName = "OldBit.Spectron.Emulation.Tape.Loader.Files.128.szx";

    public Emulator EnterLoadCommand(ComputerType computerType)
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