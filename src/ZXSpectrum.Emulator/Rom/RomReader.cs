using System.Reflection;

namespace OldBit.ZXSpectrum.Emulator.Rom;

public static class RomReader
{
    private const string Rom48ResourceName = "OldBit.ZXSpectrum.Emulator.Rom.48.rom";

    public static byte[] Read48Rom()
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(Rom48ResourceName)!;
        using var reader = new BinaryReader(stream);

        return reader.ReadBytes((int)stream.Length);
    }
}