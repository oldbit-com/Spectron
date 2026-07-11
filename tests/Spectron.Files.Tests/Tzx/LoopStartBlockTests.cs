using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Serialization;
using OldBit.Spectron.Files.Tzx.Blocks;

namespace OldBit.Spectron.Files.Tests.Tzx;

public class LoopStartBlockTests
{
    [Fact]
    public void LoopStartBlock_NewInstance_ShouldCreateWithDefaultValues()
    {
        var block = new LoopStartBlock();

        block.Count.ShouldBe(0);
    }

    [Fact]
    public void LoopStartBlock_ShouldDeserializeFromStream()
    {
        var bytes = new byte[] { 0x05, 0x00 };
        using var stream = new MemoryStream(bytes);
        var block = new LoopStartBlock(new ByteStreamReader(stream));

        block.Count.ShouldBe(5);
    }

    [Fact]
    public void LoopStartBlock_ShouldSerializeToBytes()
    {
        var block = new LoopStartBlock{ Count = 2 };

        var result = FileDataSerializer.Serialize(block);

        result.ShouldBeEquivalentTo(new byte[] { 0x24, 0x02, 0x00 });
    }
}