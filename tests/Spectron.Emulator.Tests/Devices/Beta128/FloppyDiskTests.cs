using OldBit.Spectron.Emulation.Devices.Beta128.Floppy;

namespace OldBit.Spectron.Emulator.Tests.Devices.Beta128;

public class FloppyDiskTests
{
    [Fact]
    public void Floppy_XXX()
    {
        var floppy = new FloppyDisk(80, 2);
    }

    // [Theory]
    // [InlineData(80, 2)]
    // [InlineData(80, 1)]
    // [InlineData(40, 2)]
    // [InlineData(40, 1)]
    // public void Floppy_ShouldReadDataByTrackSideSectorAndOffset(int numberOfTracks, int numberOfSides)
    // {
    //     var image = CreateFloppyImage(numberOfTracks, numberOfSides);
    //
    //     var floppy = new FloppyDisk(image);
    //
    //     for (var track = 0; track < numberOfTracks; track++)
    //     {
    //         for (var side = 0; side < numberOfSides; side++)
    //         {
    //             for (var sector = 0; sector < 16; sector++)
    //             {
    //                 floppy.GetData(track, side, sector, offset: 0).ShouldBe(track);
    //                 floppy.GetData(track, side, sector, offset: 1).ShouldBe(side);
    //                 floppy.GetData(track, side, sector, offset: 2).ShouldBe(sector);
    //             }
    //         }
    //     }
    // }

    private static byte[] CreateFloppyImage(int numberOfTracks, int numberOfSides)
    {
        var image = new List<byte>();

        for (byte track = 0; track < numberOfTracks; track++)
        {
            for (byte side = 0; side < numberOfSides; side++)
            {
                for (byte sector = 0; sector < 16; sector++)
                {
                    var data = new byte[256];
                    Array.Fill(data, (byte)0xFF);

                    data[0] = track;
                    data[1] = side;
                    data[2] = sector;

                    image.AddRange(data);
                }
            }
        }

        image[0x8E3] = (numberOfTracks, numberOfSides) switch
        {
            (80, 2) => 0x16,
            (80, 1) => 0x18,
            (40, 2) => 0x17,
            (40, 1) => 0x19,
            _ => 0xFF
        };

        return image.ToArray();
    }
}