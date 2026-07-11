using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Serialization;
using OldBit.Spectron.Files.Tzx.Blocks;

namespace OldBit.Spectron.Files.Tests.Tzx;

public class JumpToBlockTests
{
    [Fact]
    public void JumpToBlock_NewInstance_ShouldCreateWithDefaultValues()
    {
        var block = new JumpToBlock();

        block.Jump.ShouldBe(0);
    }

    [Fact]
    public void JumpToBlock_ShouldDeserializeFromStream()
    {
        using var stream = new MemoryStream([0xE7, 0xFF]);
        var block = new JumpToBlock(new ByteStreamReader(stream));

        block.Jump.ShouldBe(-25);
    }

    [Fact]
    public void JumpToBlock_ShouldSerializeToBytes()
    {
        var block = new JumpToBlock { Jump = -1 };

        var result = FileDataSerializer.Serialize(block);

        result.ShouldBeEquivalentTo(new byte[] { 0x23, 0xFF, 0xFF });
    }
}