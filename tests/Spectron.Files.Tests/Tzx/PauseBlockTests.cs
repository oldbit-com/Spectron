using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Serialization;
using OldBit.Spectron.Files.Tzx.Blocks;

namespace OldBit.Spectron.Files.Tests.Tzx;

public class PauseBlockTests
{
    [Fact]
    public void PauseBlock_NewInstance_ShouldCreateWithDefaultValues()
    {
        var block = new PauseBlock();

        block.Duration.ShouldBe(0);
    }

    [Fact]
    public void PauseBlock_ShouldDeserializeFromStream()
    {
        var bytes = new byte[] { 0x01, 0x02 };
        using var stream = new MemoryStream(bytes);
        var block = new PauseBlock(new ByteStreamReader(stream));

        block.Duration.ShouldBe(0x0201);
    }

    [Fact]
    public void PauseBlock_ShouldSerializeToBytes()
    {
        var block = new PauseBlock{ Duration = 1234 };

        var result = FileDataSerializer.Serialize(block);

        result.ShouldBeEquivalentTo(new byte[] { 0x20, 0xD2, 0x04 });
    }
}