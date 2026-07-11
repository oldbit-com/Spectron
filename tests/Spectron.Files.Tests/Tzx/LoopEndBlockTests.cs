using OldBit.Spectron.Files.Serialization;
using OldBit.Spectron.Files.Tzx.Blocks;

namespace OldBit.Spectron.Files.Tests.Tzx;

public class LoopEndBlockTests
{
    [Fact]
    public void LoopEndBlock_ShouldSerializeToBytes()
    {
        var block = new LoopEndBlock();

        var result = FileDataSerializer.Serialize(block);

        result.ShouldBeEquivalentTo(new byte[] { 0x25 });
    }
}