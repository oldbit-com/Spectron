using System.Reflection;
using OldBit.Spectron.Emulation.Devices.Beta128.Controller;
using OldBit.Spectron.Emulation.Devices.Beta128.Image;

namespace OldBit.Spectron.Emulator.Tests.Devices.Beta128.Image;

public class TrdDiskImageTests
{
    private readonly string _testFilePath;
    private readonly TrdDiskImage _diskImage = new();

    public TrdDiskImageTests()
    {
        var binFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        _testFilePath = Path.Combine(binFolder, "TestData", "Test Disk.trd");
    }

    [Fact]
    public void Disk_ShouldLoadFromFile()
    {
        var disk = _diskImage.Read(_testFilePath);

        disk.TotalCylinders.ShouldBe(80);
        disk.TotalSides.ShouldBe(2);
        disk.Label.ShouldBe("Test    ");
        disk.DiskType.ShouldBe(0x16);
        disk.TotalFiles.ShouldBe(0);
        disk.TotalFreeSectors.ShouldBe(2544);

        var systemSector = disk.GetTrack(0, 0)[9];
        systemSector[0xE3].ShouldBe(0x16);
        systemSector[0xE7].ShouldBe(0x10);

        for (var trackNo = 0; trackNo < 80; trackNo++)
        {
            for (var sideNo = 0; sideNo < 2; sideNo++)
            {
                var track = disk.GetTrack(trackNo, sideNo);

                for (var sectorNo = 1; sectorNo <= 16; sectorNo++)
                {
                    var sector = track[sectorNo];

                    sector.SectorNo.ShouldBe(sectorNo);
                    sector.CylinderNo.ShouldBe(trackNo);
                    sector.SideNo.ShouldBe(sideNo);
                    sector.Length.ShouldBe(256);
                    sector.DataAddressMark.ShouldBe(DataAddressMark.Normal);

                    var data = sector.GetData();
                    data.Length.ShouldBe(256);
                }
            }
        }
    }

    [Fact]
    public void Disk_ShouldWriteToStream()
    {
        var data = File.ReadAllBytes(_testFilePath);
        var disk = _diskImage.Read(data);

        using var stream = new MemoryStream();
        _diskImage.Write(disk, stream);

        var writtenData = stream.ToArray();

        writtenData.Length.ShouldBe(data.Length);
        writtenData.ShouldBe(data);
    }
}