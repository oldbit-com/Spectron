using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Serialization;
using OldBit.Spectron.Files.Tzx.Blocks;

namespace OldBit.Spectron.Files.Tests.Tzx;

public class PureDataBlockTests
{
    [Fact]
    public void PureDataBlock_NewInstance_ShouldCreateWithDefaultValues()
    {
        var block = new PureDataBlock();

        block.ZeroBitPulseLength.ShouldBe(0);
        block.OneBitPulseLength.ShouldBe(0);
        block.UsedBitsInLastByte.ShouldBe(0);
        block.PauseDuration.ShouldBe(0);
        block.Data.Count.ShouldBe(0);
    }

    [Fact]
    public void PureDataBlock_ShouldDeserializeFromStream()
    {
        using var stream = new MemoryStream([
            0x07, 0x08, 0x09, 0x0A, 0x0D, 0x0E, 0x0F, 0x03, 0x00, 0x00, 0xA0, 0xA1, 0xA2
        ]);
        var block = new PureDataBlock(new ByteStreamReader(stream));

        block.ZeroBitPulseLength.ShouldBe(0x0807);
        block.OneBitPulseLength.ShouldBe(0x0A09);
        block.UsedBitsInLastByte.ShouldBe(0x0D);
        block.Data.Count.ShouldBe(3);
        block.Data[0].ShouldBe(0xA0);
        block.Data[1].ShouldBe(0xA1);
        block.Data[2].ShouldBe(0xA2);
    }

    [Fact]
    public void PureDataBlock_ShouldSerializeToBytes()
    {
        var block = new PureDataBlock
        {
            ZeroBitPulseLength = 1000,
            OneBitPulseLength = 2000,
            UsedBitsInLastByte = 1,
            PauseDuration = 3000,
            Data = [0x01, 0x02, 0x03, 0x04]
        };

        var result = FileDataSerializer.Serialize(block);

        result.ShouldBeEquivalentTo(new byte[]
        {
            0x14, 0xE8, 0x03, 0xD0, 0x07, 0x01, 0xB8, 0x0B,
            0x04, 0x00, 0x00, 0x01, 0x02, 0x03, 0x04
        });
    }
}