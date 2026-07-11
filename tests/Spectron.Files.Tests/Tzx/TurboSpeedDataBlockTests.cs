using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Serialization;
using OldBit.Spectron.Files.Tzx.Blocks;

namespace OldBit.Spectron.Files.Tests.Tzx;

public class TurboSpeedDataBlockTests
{
    [Fact]
    public void TurboSpeedDataBlock_NewInstance_ShouldCreateWithDefaultValues()
    {
        var block = new TurboSpeedDataBlock();

        block.PilotPulseLength.ShouldBe(0);
        block.FirstSyncPulseLength.ShouldBe(0);
        block.SecondSyncPulseLength.ShouldBe(0);
        block.ZeroBitPulseLength.ShouldBe(0);
        block.OneBitPulseLength.ShouldBe(0);
        block.PilotToneLength.ShouldBe(0);
        block.UsedBitsInLastByte.ShouldBe(0);
        block.PauseDuration.ShouldBe(0);
        block.Data.Count.ShouldBe(0);
    }

    [Fact]
    public void TurboSpeedDataBlock_ShouldDeserializeFromStream()
    {
        var bytes = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x03, 0x00, 0x00, 0xA0, 0xA1, 0xA2 };
        using var stream = new MemoryStream(bytes);
        var block = new TurboSpeedDataBlock(new ByteStreamReader(stream));

        block.PilotPulseLength.ShouldBe(0x0201);
        block.FirstSyncPulseLength.ShouldBe(0x0403);
        block.SecondSyncPulseLength.ShouldBe(0x0605);
        block.ZeroBitPulseLength.ShouldBe(0x0807);
        block.OneBitPulseLength.ShouldBe(0x0A09);
        block.PilotToneLength.ShouldBe(0x0C0B);
        block.UsedBitsInLastByte.ShouldBe(0x0D);
        block.Data.Count.ShouldBe(3);
        block.Data[0].ShouldBe(0xA0);
        block.Data[1].ShouldBe(0xA1);
        block.Data[2].ShouldBe(0xA2);
    }

    [Fact]
    public void TurboSpeedDataBlock_ShouldSerializeToBytes()
    {
        var block = new TurboSpeedDataBlock
        {
            PilotPulseLength = 1000,
            FirstSyncPulseLength = 2000,
            SecondSyncPulseLength = 3000,
            ZeroBitPulseLength = 4000,
            OneBitPulseLength = 5000,
            PilotToneLength = 6000,
            UsedBitsInLastByte = 5,
            PauseDuration = 7000,
            Data = [0x01, 0x02, 0x03, 0x04]
        };

        var result = FileDataSerializer.Serialize(block);

        result.ShouldBeEquivalentTo(new byte []
        {
            0x11, 0xE8, 0x03, 0xD0, 0x07, 0xB8, 0x0B, 0xA0, 0x0F, 0x88,
            0x13, 0x70, 0x17, 0x05, 0x58, 0x1B, 0x04, 0x00, 0x00, 0x01, 0x02, 0x03, 0x04
        });
    }
}