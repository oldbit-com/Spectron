using OldBit.Spectron.Files.Serialization;
using OldBit.Spectron.Files.Tzx.Blocks;

namespace OldBit.Spectron.Files.Tests.Tzx;

public class GroupEndBlockTests
{
    [Fact]
    public void GroupEndBlock_ShouldSerializeToBytes()
    {
        var block = new GroupEndBlock();

        var result = FileDataSerializer.Serialize(block);

        result.ShouldBeEquivalentTo(new byte[] { 0x22 });
    }
}