using OldBit.Spectron.Emulation.Rom;

namespace OldBit.Spectron.Emulator.Tests.Rom;

public class RomTrapsTests
{
    [Theory]
    [InlineData(RomType.Original48, true)]
    [InlineData(RomType.Original128Bank0, false)]
    [InlineData(RomType.Original128Bank1, true)]
    [InlineData(RomType.Timex2048, true)]
    [InlineData(RomType.BusySoft, true)]
    [InlineData(RomType.GoshWonderful, true)]
    [InlineData(RomType.PrettyBasic, true)]
    [InlineData(RomType.Harston, false)]
    [InlineData(RomType.HtrSuperBasic, false)]
    [InlineData(RomType.BbcBasic, false)]
    [InlineData(RomType.Retroleum, false)]
    [InlineData(RomType.BrendanAlford, false)]
    [InlineData(RomType.Pentagon128, false)]
    public void IsSupported_ShouldDetectOriginalTapeRoutines(RomType romType, bool expected)
    {
        var rom = RomReader.ReadRom(romType);

        RomTraps.IsSupported(rom).ShouldBe(expected);
    }

    [Fact]
    public void IsSupported_WhenRomIsTooShort_ShouldReturnFalse()
    {
        var rom = RomReader.ReadRom(RomType.Original48)[..0x0500];

        RomTraps.IsSupported(rom).ShouldBeFalse();
    }

    [Fact]
    public void IsSupported_WhenTrapEntryModified_ShouldReturnFalse()
    {
        var rom = RomReader.ReadRom(RomType.Original48);
        rom[RomRoutines.LD_START] = 0x00;

        RomTraps.IsSupported(rom).ShouldBeFalse();
    }
}
