using System.Reflection;

namespace OldBit.Spectron.Emulation.Rom;

internal static class RomReader
{
    // Original ROMs
    private const string Rom48ResourceName = "OldBit.Spectron.Emulation.Rom.Files.48.rom";
    private const string Rom128Bank0ResourceName = "OldBit.Spectron.Emulation.Rom.Files.128-0.rom";
    private const string Rom128Bank1ResourceName = "OldBit.Spectron.Emulation.Rom.Files.128-1.rom";
    private const string RomTimex2048ResourceName = "OldBit.Spectron.Emulation.Rom.Files.TC2048.rom";

    // Customs ROMs
    private const string GoshWonderfulRomResourceName = "OldBit.Spectron.Emulation.Rom.Files.gw03.rom";
    private const string BusySoftRomResourceName = "OldBit.Spectron.Emulation.Rom.Files.bsrom140.rom";
    private const string HarstonRomResourceName = "OldBit.Spectron.Emulation.Rom.Files.JGH077.ROM";

    // Diagnostic ROMs
    private const string RetroleumRomResourceName = "OldBit.Spectron.Emulation.Rom.Files.DiagROMv.171";
    private const string BrendanAlfordRomResourceName = "OldBit.Spectron.Emulation.Rom.Files.testrom1.37.bin";

    // DivMMC ROM
    private const string DivMmcRomResourceName = "OldBit.Spectron.Emulation.Rom.Files.ESXMMC.0.8.9.BIN";

    // Interface 1 ROMs
    private const string Interface1V1RomResourceName = "OldBit.Spectron.Emulation.Rom.Files.interface1-v1.rom";
    private const string Interface1V2RomResourceName = "OldBit.Spectron.Emulation.Rom.Files.interface1-v2.rom";

    // TR-DOS
    private const string TrDosRomResourceName = "OldBit.Spectron.Emulation.Rom.Files.trdos-503.rom";

    internal static byte[] ReadRom(RomType romType)
    {
        var resourceName = romType switch
        {
            RomType.Original48 => Rom48ResourceName,
            RomType.Original128Bank0 => Rom128Bank0ResourceName,
            RomType.Original128Bank1 => Rom128Bank1ResourceName,
            RomType.Timex2048 => RomTimex2048ResourceName,
            RomType.Retroleum => RetroleumRomResourceName,
            RomType.GoshWonderful => GoshWonderfulRomResourceName,
            RomType.BusySoft => BusySoftRomResourceName,
            RomType.Harston => HarstonRomResourceName,
            RomType.BrendanAlford => BrendanAlfordRomResourceName,
            RomType.DivMmc => DivMmcRomResourceName,
            RomType.Interface1V1 => Interface1V1RomResourceName,
            RomType.Interface1V2 => Interface1V2RomResourceName,
            RomType.TrDos => TrDosRomResourceName,
            _ => throw new ArgumentOutOfRangeException(nameof(romType), romType, null)
        };

        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new BinaryReader(stream);

        return reader.ReadBytes((int)stream.Length);
    }
}