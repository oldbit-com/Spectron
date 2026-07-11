using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Serialization;
using OldBit.Spectron.Files.Tzx.Blocks;

namespace OldBit.Spectron.Files.Tests.Tzx;

public class MessageBlockTests
{
    [Fact]
    public void MessageBlock_NewInstance_ShouldCreateWithDefaultValues()
    {
        var block = new MessageBlock();

        block.Time.ShouldBe(0);
        block.Message.ShouldBeEmpty();
    }

    [Fact]
    public void MessageBlock_ShouldDeserializeFromStream()
    {
        using var stream = new MemoryStream([
            0x0A, 0x08, 0x53, 0x69, 0x6E, 0x63, 0x6C, 0x61, 0x69, 0x72
        ]);
        var block = new MessageBlock(new ByteStreamReader(stream));

        block.Time.ShouldBe(10);
        block.Message.ShouldBe("Sinclair");
    }

    [Fact]
    public void MessageBlock_ShouldSerializeToBytes()
    {
        var block = new MessageBlock
        {
            Time = 20,
            Message = "message"
        };

        var result = FileDataSerializer.Serialize(block);

        result.ShouldBeEquivalentTo(new byte[] { 0x31, 0x14, 0x07, 0x6d, 0x65, 0x73, 0x73, 0x61, 0x67, 0x65 });
    }
}