using System.Reflection;
using OldBit.Spectron.Files.Rzx;
using OldBit.Spectron.Files.Rzx.Blocks;
using OldBit.Spectron.Files.Szx;

namespace OldBit.Spectron.Files.Tests.Rzx;

public class RzxFileTests
{
    [Fact]
    public void RzxFile_ShouldLoad()
    {
        using var file = LoadTestFile("test.rzx");
        var rzxFile = RzxFile.Load(file);

        rzxFile.Creator?.CreatorName.ShouldStartWith("Spectaculator");
        rzxFile.Creator?.MajorVersion.ShouldBe(52);
        rzxFile.Creator?.MinorVersion.ShouldBe(371);

        rzxFile.Snapshots.ShouldHaveSingleItem();
        rzxFile.Snapshots[0].Extension.ShouldBe("z80");

        rzxFile.Snapshots[0].Recording.ShouldNotBeNull();
        rzxFile.Snapshots[0].Recording!.Frames.Count.ShouldBe(21381);
    }

    [Fact]
    public void RzxFile_ShouldSerializeToBytes()
    {
        var rzxFile = new RzxFile
        {
            Creator = new CreatorBlock
            {
                CreatorName = "Spectron",
                MajorVersion = 1,
                MinorVersion = 2,
                Data = [0xAA, 0x55]
            }
        };

        var snapshot = new SnapshotBlock("szx", [0x01, 0x02, 0x03, 0x04]);
        rzxFile.Snapshots.Add(snapshot);

        var frame = new RecordingFrame { FetchCounter = 0x7F };
        frame.InValues.AddRange([0x10, 0x20, 0x30, 0x40]);

        snapshot.Recording.TStatesCounter = 0x01020304;
        snapshot.Recording.Frames.Add(frame);
        snapshot.Recording.Frames.Add(frame); // repeated frame
        snapshot.Recording.Frames.Add(frame); // repeated frame

        using var stream = new MemoryStream();
        rzxFile.Save(stream);

        var result = stream.ToArray();

        // Header
        result[..10].ShouldBe([0x52, 0x5A, 0x58, 0x21, 0x00, 0x0D, 0x00, 0x00, 0x00, 0x00]);

        // Creator
        result[10..41].ShouldBe(
        [
            0x10, 0x1F, 0x00, 0x00, 0x00,
            0x53, 0x70, 0x65, 0x63, 0x74, 0x72, 0x6F, 0x6E,
            0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
            0x20, 0x20, 0x20, 0x20, 0x01, 0x00, 0x02, 0x0,
            0xAA, 0x55
        ]);

        // Snapshot
        result[41..46].ShouldBe([0x30, 0x1D, 0x00, 0x00, 0x00]);
        result[46..50].ShouldBe([0x02, 0x00, 0x00, 0x00]);
        result[50..54].ShouldBe([0x73, 0x7A, 0x78, 0x00]);
        result[54..58].ShouldBe([0x04, 0x00, 0x00, 0x00]);
        ZLibHelper.Decompress(result[58..70]).ShouldBe([0x01, 0x02, 0x03, 0x04]);

        // Recording
        result[70..75].ShouldBe([0x80, 0x27, 0x00, 0x00, 0x00]);
        result[75..80].ShouldBe([0x03, 0x00, 0x00, 0x00, 0x00]);
        result[80..84].ShouldBe([0x04, 0x03, 0x02, 0x01]);
        result[84..88].ShouldBe([0x02, 0x00, 0x00, 0x00]);
        ZLibHelper.Decompress(result[88..109]).ShouldBe(
        [
            0x7F, 0x00, 0x04, 0x00, 0x10, 0x20, 0x30, 0x40,
            0x7F, 0x00, 0xFF, 0xFF, 0x7F, 0x00, 0xFF, 0xFF
        ]);
    }

    private static FileStream LoadTestFile(string fileName)
    {
        var location = typeof(RzxFileTests).GetTypeInfo().Assembly.Location;
        var dir = Path.GetDirectoryName(location) ?? throw new InvalidOperationException();
        var path =  Path.Combine(dir, "TestFiles", fileName);

        return File.OpenRead(path);
    }
}