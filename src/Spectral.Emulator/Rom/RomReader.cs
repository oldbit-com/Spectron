using System.Reflection;

namespace OldBit.Spectral.Emulator.Rom;

internal static class RomReader
{
    private const string Rom48ResourceName = "OldBit.Spectral.Emulator.Rom.Files.48.rom";
    private const string RetroleumRomResourceName = "OldBit.Spectral.Emulator.Rom.Files.DiagROMv.171";
    private const string GoshWonderfulRomResourceName = "OldBit.Spectral.Emulator.Rom.Files.gw03.rom";
    private const string BusySoftRomResourceName = "OldBit.Spectral.Emulator.Rom.Files.bsrom140.rom";
    private const string HarstonRomResourceName = "OldBit.Spectral.Emulator.Rom.Files.JGH077.ROM";

    internal static byte[] ReadRom(RomType romType)
    {
        var resourceName = romType switch
        {
            RomType.Original48 => Rom48ResourceName,
            RomType.Retroleum => RetroleumRomResourceName,
            RomType.GoshWonderful => GoshWonderfulRomResourceName,
            RomType.BusySoft => BusySoftRomResourceName,
            RomType.Harston => HarstonRomResourceName,
            _ => throw new ArgumentOutOfRangeException(nameof(romType), romType, null)
        };

        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new BinaryReader(stream);

        return reader.ReadBytes((int)stream.Length);
    }
}