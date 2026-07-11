using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Serialization;
using OldBit.Spectron.Files.Tzx;
using OldBit.Spectron.Files.Tzx.Blocks;

namespace OldBit.Spectron.Files.Tests.Tzx;

public class ArchiveInfoBlockTests
{
    [Fact]
    public void ArchiveInfoBlock_NewInstance_ShouldCreateWithDefaultValues()
    {
        var block = new ArchiveInfoBlock();

        block.Infos.Count.ShouldBe(0);
    }

    [Fact]
    public void ArchiveInfoBlock_ShouldDeserializeFromStream()
    {
        using var stream = new MemoryStream([
            0x15, 0x00, 0x03,
            0x00, 0x05, 0x54, 0x69, 0x74, 0x6c, 0x65,   // Title
            0x03, 0x04, 0x31, 0x39, 0x38, 0x34,         // 1984
            0x06, 0x05, 0xA3, 0x37, 0x2E, 0x39, 0x35    // £7.95
        ]);
        var block = new ArchiveInfoBlock(new ByteStreamReader(stream));

        block.Infos.Count.ShouldBe(3);
        block.Infos[0].Id.ShouldBe(ArchiveInfo.Title);
        block.Infos[0].Length.ShouldBe(5);
        block.Infos[0].Text.ShouldBe("Title");
        block.Infos[1].Id.ShouldBe(ArchiveInfo.Year);
        block.Infos[1].Length.ShouldBe(4);
        block.Infos[1].Text.ShouldBe("1984");
        block.Infos[2].Id.ShouldBe(ArchiveInfo.Price);
        block.Infos[2].Length.ShouldBe(5);
        block.Infos[2].Text.ShouldBe("£7.95");
    }

    [Fact]
    public void ArchiveInfoBlock_ShouldSerializeToBytes()
    {
        var block = new ArchiveInfoBlock();
        block.AddTextInfo(ArchiveInfo.Title, "Title");
        block.AddTextInfo(ArchiveInfo.Year, "1984");
        block.AddTextInfo(ArchiveInfo.Price, "£7.95");

        var result = FileDataSerializer.Serialize(block);

        result.ShouldBeEquivalentTo(new byte[]
        {
            0x32, 0x15, 0x00, 0x03,
            0x00, 0x05, 0x54, 0x69, 0x74, 0x6c, 0x65,
            0x03, 0x04, 0x31, 0x39, 0x38, 0x34,
            0x06, 0x05, 0xA3, 0x37, 0x2E, 0x39, 0x35
        });
    }
}