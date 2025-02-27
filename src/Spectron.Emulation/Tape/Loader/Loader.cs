using System.Reflection;
using OldBit.Spectron.Emulation.Snapshot;
using OldBit.Spectron.Files.Szx;

namespace OldBit.Spectron.Emulation.Tape.Loader;

/// <summary>
/// Responsible for simulating LOAD "" instruction entered by a user.
/// Basically it loads a snapshot taken when the LOAD "" instruction has been entered,
/// so it means we don't type the command, but we simulate it.
/// </summary>
public sealed class Loader(SzxSnapshot szxSnapshot)
{
    private const string Loader16ResourceName = "OldBit.Spectron.Emulation.Tape.Loader.Files.16.szx";
    private const string Loader48ResourceName = "OldBit.Spectron.Emulation.Tape.Loader.Files.48.szx";
    private const string Loader128ResourceName = "OldBit.Spectron.Emulation.Tape.Loader.Files.128.szx";

    public Emulator EnterLoadCommand(ComputerType computerType)
    {
        var resourceName = computerType switch
        {
            ComputerType.Spectrum16K => Loader16ResourceName,
            ComputerType.Spectrum48K => Loader48ResourceName,
            ComputerType.Spectrum128K => Loader128ResourceName,
            _ => throw new NotSupportedException($"The computer type '{computerType}' is not supported.")
        };

        var szx = GetSnapshot(resourceName);

        return szxSnapshot.CreateEmulator(szx);
    }

    private static SzxFile GetSnapshot(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(resourceName)!;

        return SzxFile.Load(stream);
    }
}