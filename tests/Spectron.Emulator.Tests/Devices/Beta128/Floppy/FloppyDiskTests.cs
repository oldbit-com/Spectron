using OldBit.Spectron.Emulation.Devices.Beta128.Floppy;

namespace OldBit.Spectron.Emulator.Tests.Devices.Beta128.Floppy;

public class FloppyDiskTests
{
    [Theory]
    [InlineData(80, 2)]
    [InlineData(80, 1)]
    [InlineData(40, 2)]
    [InlineData(40, 1)]
    public void FloppyDisk_ShouldBeCreatedAndInitialized(int totalCylinders, int totalSides)
    {
        var disk = new FloppyDisk(totalCylinders, totalSides);

        for (var cylinderNo = 0; cylinderNo < totalCylinders; cylinderNo++)
        {
            for (var sideNo = 0; sideNo < totalSides; sideNo++)
            {
                var track = disk.GetTrack(cylinderNo, sideNo);

                for (var sectorNo = 1; sectorNo <= 16; sectorNo++)
                {
                    var sector = track[sectorNo];

                    sector.CylinderNo.ShouldBe(cylinderNo);

                    var idCrc = sector.CalculateIdCrc();
                    sector.IdCrc.ShouldBe(idCrc);

                    var idPosition = CalculateIdPosition(sectorNo);
                    sector.IdPosition.ShouldBe(idPosition);
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
        const int basePos = 152;
        const int step = 358;

        var block = (sectorNo - 1) / 8;
        var offset = (sectorNo - 1) % 8;

        return basePos + 2 * step * offset + step * block;
    }
}