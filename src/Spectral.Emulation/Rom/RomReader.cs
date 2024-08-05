using System.Reflection;

namespace OldBit.Spectral.Emulation.Rom;

internal static class RomReader
{
    private const string Rom48ResourceName = "OldBit.Spectral.Emulation.Rom.Files.48.rom";
    private const string RetroleumRomResourceName = "OldBit.Spectral.Emulation.Rom.Files.DiagROMv.171";
    private const string GoshWonderfulRomResourceName = "OldBit.Spectral.Emulation.Rom.Files.gw03.rom";
    private const string BusySoftRomResourceName = "OldBit.Spectral.Emulation.Rom.Files.bsrom140.rom";
    private const string HarstonRomResourceName = "OldBit.Spectral.Emulation.Rom.Files.JGH077.ROM";
    private const string BrendanAlfordRomResourceName = "OldBit.Spectral.Emulation.Rom.Files.testrom1.37.bin";

    internal static byte[] ReadRom(RomType romType)
    {
        var resourceName = romType switch
        {
            RomType.Original48 => Rom48ResourceName,
            RomType.Retroleum => RetroleumRomResourceName,
            RomType.GoshWonderful => GoshWonderfulRomResourceName,
            RomType.BusySoft => BusySoftRomResourceName,
            RomType.Harston => HarstonRomResourceName,
            RomType.BrendanAlford => BrendanAlfordRomResourceName,
            _ => throw new ArgumentOutOfRangeException(nameof(romType), romType, null)
        };

        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new BinaryReader(stream);

        return reader.ReadBytes((int)stream.Length);
    }
}