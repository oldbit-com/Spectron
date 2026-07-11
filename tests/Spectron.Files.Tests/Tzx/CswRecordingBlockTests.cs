using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Serialization;
using OldBit.Spectron.Files.Tzx.Blocks;

namespace OldBit.Spectron.Files.Tests.Tzx;

public class CswRecordingBlockTests
{
    [Fact]
    public void CswRecordingBlock_NewInstance_ShouldCreateWithDefaultValues()
    {
        var block = new CswRecordingBlock();

        block.PauseDuration.ShouldBe(0);
        block.CompressionType.ShouldBe(CompressionType.Rle);
        block.Data.Count.ShouldBe(0);
    }

    [Fact]
    public void CswRecordingBlock_ShouldDeserializeFromStream()
    {
        using var stream = new MemoryStream([
            0x0E, 0x00, 0x00, 0x00, 0x01, 0x02, 0xAA, 0xBB,
            0xCC, 0x02, 0xC1, 0xC2, 0xC3, 0xC4, 0x11, 0x12,
            0x13, 0x14
        ]);
        var block = new CswRecordingBlock(new ByteStreamReader(stream));

        block.PauseDuration.ShouldBe(0x0201);
        block.SamplingRate.ShouldBe(0xCCBBAA);
        block.CompressionType.ShouldBe(CompressionType.ZRle);
        block.StoredPulsesCount.ShouldBe(0xC4C3C2C1);
        block.Data.Count.ShouldBe(4);
        block.Data[0].ShouldBe(0x11);
        block.Data[1].ShouldBe(0x12);
        block.Data[2].ShouldBe(0x13);
        block.Data[3].ShouldBe(0x14);
    }

    [Fact]
    public void CswRecordingBlock_ShouldSerializeToBytes()
    {
        var block = new CswRecordingBlock
        {
            CompressionType = CompressionType.Rle,
            PauseDuration = 1000,
            SamplingRate = 2000,
            StoredPulsesCount = 3000,
            Data = [0x01, 0x02, 0x03, 0x04]
        };

        var result = FileDataSerializer.Serialize(block);

        result.ShouldBeEquivalentTo(new byte[]
        {
            0x18, 0x0E, 0x00, 0x00, 0x00, 0xE8, 0x03,
            0xD0, 0x07, 0x00, 0x01, 0xB8, 0x0B, 0x00, 0x00, 0x01, 0x02, 0x03, 0x04
        });
    }
}