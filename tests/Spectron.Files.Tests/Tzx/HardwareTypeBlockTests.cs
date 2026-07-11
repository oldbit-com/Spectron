using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Serialization;
using OldBit.Spectron.Files.Tzx.Blocks;

namespace OldBit.Spectron.Files.Tests.Tzx;

public class HardwareTypeBlockTests
{
    [Fact]
    public void HardwareTypeBlock_NewInstance_ShouldCreateWithDefaultValues()
    {
        var block = new HardwareTypeBlock();

        block.Infos.Count.ShouldBe(0);
    }

    [Fact]
    public void HardwareTypeBlock_ShouldDeserializeFromStream()
    {
        using var stream = new MemoryStream([
            0x03, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09
        ]);
        var block = new HardwareTypeBlock(new ByteStreamReader(stream));

        block.Infos.Count.ShouldBe(3);
        block.Infos[0].HardwareTypeId.ShouldBe(0x01);
        block.Infos[0].HardwareId.ShouldBe(0x02);
        block.Infos[0].Info.ShouldBe(0x03);
        block.Infos[1].HardwareTypeId.ShouldBe(0x04);
        block.Infos[1].HardwareId.ShouldBe(0x05);
        block.Infos[1].Info.ShouldBe(0x06);
        block.Infos[2].HardwareTypeId.ShouldBe(0x07);
        block.Infos[2].HardwareId.ShouldBe(0x08);
        block.Infos[2].Info.ShouldBe(0x09);
    }

    [Fact]
    public void HardwareTypeBlock_ShouldSerializeToBytes()
    {
        var block = new HardwareTypeBlock
        {
            Infos =
            [
                new HardwareTypeBlock.HardwareInfo { HardwareTypeId = 0x01, HardwareId = 0x02, Info = 0x03 },
                new HardwareTypeBlock.HardwareInfo { HardwareTypeId = 0x04, HardwareId = 0x05, Info = 0x06 },
                new HardwareTypeBlock.HardwareInfo { HardwareTypeId = 0x07, HardwareId = 0x08, Info = 0x09 }
            ]
        };

        var result = FileDataSerializer.Serialize(block);

        result.ShouldBeEquivalentTo(new byte[]
        {
            0x33, 0x03, 0x01, 0x02, 0x03, 0x04, 0x05,
            0x06, 0x07, 0x08, 0x09
        });
    }
}