using OldBit.Spectron.Emulation.Devices.Beta128.Floppy;

namespace OldBit.Spectron.Emulator.Tests.Devices.Beta128.Floppy;

public class FloppyDiskTests
{
    [Theory]
    [InlineData(80, 2, 0x16)]
    [InlineData(80, 1, 0x18)]
    [InlineData(40, 2, 0x17)]
    [InlineData(40, 1, 0x19)]
    public void FloppyDisk_ShouldBeCreatedAndInitialized(int totalCylinders, int totalSides, byte expectedFormat)
    {
        var disk = new FloppyDisk(totalCylinders, totalSides);

        var systemSector = disk.GetTrack(0, 0)[9];
        systemSector[0xE3].ShouldBe(expectedFormat);
        systemSector[0xE7].ShouldBe(0x10);

        for (var cylinderNo = 0; cylinderNo < totalCylinders; cylinderNo++)
        {
            for (var sideNo = 0; sideNo < totalSides; sideNo++)
            {
                var track = disk.GetTrack(cylinderNo, sideNo);

                for (var sectorNo = 1; sectorNo <= 16; sectorNo++)
                {
                    var sector = track[sectorNo];

                    sector.CylinderNo.ShouldBe(cylinderNo);
                    sector.VerifyIdCrc().ShouldBeTrue();

                    var idPosition = CalculateIdPosition(sectorNo);
                    sector.IdPosition.ShouldBe(idPosition);

                    sector.SectorNo.ShouldBe(sectorNo);
                    sector.SideNo.ShouldBe(sideNo);
                }
            }
        }
    }

    [Fact]
    public void FloppyDisk_ShouldHaveTrdDiskInformation()
    {
        var disk = new FloppyDisk(80, 1);
        var track = disk.GetTrack(0, 0);
        var sector = track[9];

        sector[0xE1].ShouldBe(0x00);
        sector[0xE2].ShouldBe(0x01);
        sector[0xE3].ShouldBe(0x18);
        sector[0xE5].ShouldBe(0xF0);
        sector[0xE6].ShouldBe(0x04);
        sector[0xE7].ShouldBe(0x10);
    }

    private static int CalculateIdPosition(int sectorNo)
    {
        const int basePos = 162;
        const int step = 372;

        var block = (sectorNo - 1) / 8;
        var offset = (sectorNo - 1) % 8;

        return basePos + 2 * step * offset + step * block;
    }
}