using System.Text;
using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Szx.Blocks;

namespace OldBit.Spectron.Files.Tests.Szx.Blocks;

public class CreatorBlockTests
{
    [Fact]
    public void Creator_ShouldConvertToBytes()
    {
        var creator = GetCreatorBlock();
        using var writer = new MemoryStream();

        creator.Write(writer);

        var data = writer.ToArray();
        data.Length.ShouldBe(8 + 37);

        // Header
        BitConverter.ToUInt32(data[..4].ToArray()).ShouldBe((DWord)0x52545243);
        BitConverter.ToUInt32(data[4..8].ToArray()).ShouldBe((DWord)37);

        // Data
        Encoding.ASCII.GetString(data[8..40]).Trim('\0').ShouldBe("Test creator");
        BitConverter.ToUInt16(data[40..42].ToArray()).ShouldBe(1);
        BitConverter.ToUInt16(data[42..44].ToArray()).ShouldBe(2);
        data[44].ShouldBe(0);
    }

    [Fact]
    public void Creator_ShouldConvertFromBytes()
    {
        var creatorData = GetCreatorBlockData();
        using var memoryStream = new MemoryStream(creatorData);
        var reader = new ByteStreamReader(memoryStream);

        var creator = CreatorBlock.Read(reader, creatorData.Length);

        creator.Name.ShouldBe("Test creator");
        creator.MajorVersion.ShouldBe(1);
        creator.MinorVersion.ShouldBe(2);
    }

    private static byte[] GetCreatorBlockData()
    {
        var creator = GetCreatorBlock();
        using var writer = new MemoryStream();

        creator.Write(writer);

        return writer.ToArray()[8..].ToArray();
    }

    private static CreatorBlock GetCreatorBlock() => new()
    {
        Name = "Test creator",
        MajorVersion = 1,
        MinorVersion = 2,
    };
}