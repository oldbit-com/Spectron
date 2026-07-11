using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Serialization;
using OldBit.Spectron.Files.Tzx.Blocks;

namespace OldBit.Spectron.Files.Tests.Tzx;

public class GlueBlockTests
{
    [Fact]
    public void GlueBlock_NewInstance_ShouldCreateWithDefaultValues()
    {
        var block = new GlueBlock();

        block.Data.Count.ShouldBe(9);
    }

    [Fact]
    public void GlueBlock_ShouldDeserializeFromStream()
    {
        using var stream = new MemoryStream([
            0x58, 0x54, 0x61, 0x70, 0x65, 0x21, 0x1A, 0x01, 0x02
        ]);
        var block = new GlueBlock(new ByteStreamReader(stream));

        block.Data.Count.ShouldBe(9);
    }

    [Fact]
    public void GlueBlock_ShouldSerializeToBytes()
    {
        var block = new GlueBlock();

        var result = FileDataSerializer.Serialize(block);

        result.ShouldBeEquivalentTo(new byte[] { 0x5A, 0x58, 0x54, 0x61, 0x70, 0x65, 0x21, 0x1A, 0x01, 0x14 });
    }
}