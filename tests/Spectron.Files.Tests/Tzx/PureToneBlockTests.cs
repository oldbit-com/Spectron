using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Serialization;
using OldBit.Spectron.Files.Tzx.Blocks;

namespace OldBit.Spectron.Files.Tests.Tzx;

public class PureToneBlockTests
{

    [Fact]
    public void PureToneBlock_NewInstance_ShouldCreateWithDefaultValues()
    {
        var block = new PureToneBlock();

        block.PulseLength.ShouldBe(0);
        block.PulseCount.ShouldBe(0);
    }

    [Fact]
    public void PureToneBlock_ShouldDeserializeFromStream()
    {
        using var stream = new MemoryStream([0x01, 0x02, 0x02, 0x04]);
        var block = new PureToneBlock(new ByteStreamReader(stream));

        block.PulseLength.ShouldBe(0x0201);
        block.PulseCount.ShouldBe(0x0402);
    }

    [Fact]
    public void PureToneBlock_ShouldSerializeToBytes()
    {
        var block = new PureToneBlock { PulseCount = 10, PulseLength = 1923 };

        var result = FileDataSerializer.Serialize(block);

        result.ShouldBeEquivalentTo(new byte[] { 0x12, 0x83, 0x07, 0x0A, 0x00 });
    }
}