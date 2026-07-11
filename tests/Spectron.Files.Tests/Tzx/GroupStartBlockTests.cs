using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Serialization;
using OldBit.Spectron.Files.Tzx.Blocks;

namespace OldBit.Spectron.Files.Tests.Tzx;

public class GroupStartBlockTests
{
    [Fact]
    public void GroupStartBlock_NewInstance_ShouldCreateWithDefaultValues()
    {
        var block = new GroupStartBlock();

        block.Name.ShouldBeEmpty();
    }

    [Fact]
    public void GroupStartBlock_ShouldDeserializeFromStream()
    {
        using var stream = new MemoryStream([
            0x08, 0x53, 0x70, 0x65, 0x63, 0x74, 0x72, 0x75, 0x6D
        ]);
        var block = new GroupStartBlock(new ByteStreamReader(stream));

        block.Name.ShouldBe("Spectrum");
    }

    [Fact]
    public void GroupStartBlock_ShouldSerializeToBytes()
    {
        var block = new GroupStartBlock
        {
            Name = "Block description"
        };

        var result = FileDataSerializer.Serialize(block);

        result.ShouldBeEquivalentTo(new byte[]
        {
            0x21, 0x11, 0x42, 0x6c, 0x6f, 0x63, 0x6b, 0x20, 0x64, 0x65,
            0x73, 0x63, 0x72, 0x69, 0x70, 0x74, 0x69, 0x6f, 0x6e
        });
    }
}