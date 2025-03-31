namespace OldBit.Spectron.Emulation.Rom;

public enum RomType
{
    Original,

    Original48,

    Original128Bank0,

    Original128Bank1,

    Timex2048,

    Custom,

    Retroleum,

    GoshWonderful,

    BusySoft,

    Harston,

    BrendanAlford,

    DivMmc,
}

internal static class RomTypeExtensions
{
    internal static bool IsCustomRom(this RomType romType) => romType > RomType.Timex2048;
}