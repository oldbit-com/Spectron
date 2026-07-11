using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Serialization;
using OldBit.Spectron.Files.Tzx.Blocks;

namespace OldBit.Spectron.Files.Tests.Tzx;

public class C64RomTypeDataBlockTests
{
    [Fact]
    public void C64RomTypeDataBlock_ShouldDeserializeFromStream()
    {
        var data = new byte[]
        {
            0xEE, 0x00, 0x00, 0x00, 0x68, 0x02, 0x00, 0x6A, 0x98, 0x04, 0x80, 0x03, 0x68,
            0x02, 0x80, 0x03, 0x80, 0x03, 0x68, 0x02, 0x01, 0x98, 0x04, 0x80, 0x03, 0x98,
            0x04, 0x68, 0x02, 0x68, 0x02, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00, 0xCA, 0x00,
            0x00, 0x89, 0x88, 0x87, 0x86, 0x85, 0x84, 0x83, 0x82, 0x81, 0x01, 0x01, 0x08,
            0xF0, 0x9F, 0x54, 0x49, 0x54, 0x41, 0x4E, 0x49, 0x43
        };

        var newData = new byte[0xEE + 4]; // block length + first 4 bytes
        Array.Copy(data, newData, data.Length);
        for (var i = data.Length; i < newData.Length; i++)
        {
            newData[i] = 0x20;
        }
        data = newData;

        using var stream = new MemoryStream(data);
        var block = new C64RomTypeDataBlock(new ByteStreamReader(stream));

        block.Length.ShouldBe(0xEE);
        block.PilotTonePulseLength.ShouldBe(616);
        block.NumberOfWavesInPilotTone.ShouldBe(27136);
        block.FirstWaveSyncPulseLength.ShouldBe(1176);
        block.SecondWaveSyncPulseLength.ShouldBe(896);
        block.ZeroBitFirstWavePulseLength.ShouldBe(616);
        block.ZeroBitSecondWavePulseLength.ShouldBe(896);
        block.OneBitFirstWavePulseLength.ShouldBe(896);
        block.OneBitSecondWavePulseLength.ShouldBe(616);
        block.Checksum.ShouldBe(1);
        block.FinishByteFirstWavePulseLength.ShouldBe(1176);
        block.FinishByteSecondWavePulseLength.ShouldBe(896);
        block.FinishDataFirstWavePulseLength.ShouldBe(1176);
        block.FinishDataSecondWavePulseLength.ShouldBe(616);
        block.TrailingTonePulseLength.ShouldBe(616);
        block.NumberOfWavesInTrailingTone.ShouldBe(0);
        block.UsedBitsInLastByte.ShouldBe(8);
        block.GeneralPurpose.ShouldBe(0);
        block.PauseDuration.ShouldBe(0);
        block.DataLength.ShouldBe(202);
        block.Data[..24].ShouldBeEquivalentTo(new List<byte>
        {
            0x89, 0x88, 0x87, 0x86, 0x85, 0x84, 0x83, 0x82, 0x81, 0x01, 0x01, 0x08,
            0xF0, 0x9F, 0x54, 0x49, 0x54, 0x41, 0x4E, 0x49, 0x43, 0x20, 0x20, 0x20
        });
    }

    [Fact]
    public void CC64RomTypeDataBlock__ShouldSerializeToBytes()
    {
        var block = new C64RomTypeDataBlock
        {
            PilotTonePulseLength = 616,
            NumberOfWavesInPilotTone = 27136,
            FirstWaveSyncPulseLength = 1176,
            SecondWaveSyncPulseLength = 896,
            ZeroBitFirstWavePulseLength = 616,
            ZeroBitSecondWavePulseLength = 896,
            OneBitFirstWavePulseLength = 896,
            OneBitSecondWavePulseLength = 616,
            Checksum = 1,
            FinishByteFirstWavePulseLength = 1176,
            FinishByteSecondWavePulseLength = 896,
            FinishDataFirstWavePulseLength = 1176,
            FinishDataSecondWavePulseLength = 616,
            TrailingTonePulseLength = 616,
            NumberOfWavesInTrailingTone = 0,
            UsedBitsInLastByte = 8,
            GeneralPurpose = 0,
            PauseDuration = 0,
            Data =
            [
                0x89, 0x88, 0x87, 0x86, 0x85, 0x84, 0x83, 0x82, 0x81, 0x01, 0x01, 0x08,
                0xF0, 0x9F, 0x54, 0x49, 0x54, 0x41, 0x4E, 0x49, 0x43, 0x20, 0x20, 0x20
            ]
        };

        var result = FileDataSerializer.Serialize(block);

        result.ShouldBeEquivalentTo(new byte[]
        {
            0x16,
            0x3C, 0x00, 0x00, 0x00, 0x68, 0x02, 0x00, 0x6A, 0x98, 0x04, 0x80, 0x03, 0x68,
            0x02, 0x80, 0x03, 0x80, 0x03, 0x68, 0x02, 0x01, 0x98, 0x04, 0x80, 0x03, 0x98,
            0x04, 0x68, 0x02, 0x68, 0x02, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00, 0x18, 0x00,
            0x00, 0x89, 0x88, 0x87, 0x86, 0x85, 0x84, 0x83, 0x82, 0x81, 0x01, 0x01, 0x08,
            0xF0, 0x9F, 0x54, 0x49, 0x54, 0x41, 0x4E, 0x49, 0x43, 0x20, 0x20, 0x20
        });
    }
}