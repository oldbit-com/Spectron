using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Serialization;
using OldBit.Spectron.Files.Tzx.Blocks;

namespace OldBit.Spectron.Files.Tests.Tzx;

public class DirectRecordingBlockTests
{
    [Fact]
    public void DirectRecordingBlock_NewInstance_ShouldCreateWithDefaultValues()
    {
        var block = new DirectRecordingBlock();

        block.UsedBitsInLastByte.ShouldBe(0);
        block.PauseDuration.ShouldBe(0);
        block.Data.Count.ShouldBe(0);
    }

    [Fact]
    public void DirectRecordingBlock_ShouldDeserializeFromStream()
    {
        using var stream = new MemoryStream([
            0x01, 0x02, 0x03, 0x04, 0x05, 0x03, 0x00, 0x00, 0xA0, 0xA1, 0xA2
        ]);
        var block = new DirectRecordingBlock(new ByteStreamReader(stream));

        block.StatesPerSample.ShouldBe(0x0201);
        block.PauseDuration.ShouldBe(0x0403);
        block.UsedBitsInLastByte.ShouldBe(0x05);
        block.Data.Count.ShouldBe(3);
        block.Data[0].ShouldBe(0xA0);
        block.Data[1].ShouldBe(0xA1);
        block.Data[2].ShouldBe(0xA2);
    }

    [Fact]
    public void DirectRecordingBlock_ShouldSerializeToBytes()
    {
        var block = new DirectRecordingBlock
        {
            PauseDuration = 1000,
            StatesPerSample = 1432,
            UsedBitsInLastByte = 6,
            Data = [0x01, 0x02, 0x03, 0x04]
        };

        var result = FileDataSerializer.Serialize(block);

        result.ShouldBeEquivalentTo(new byte[]
        {
            0x15, 0x98, 0x05, 0xE8, 0x03, 0x06, 0x04,
            0x00, 0x00, 0x01, 0x02, 0x03, 0x04
        });
    }
}