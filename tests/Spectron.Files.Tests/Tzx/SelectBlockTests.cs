using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Serialization;
using OldBit.Spectron.Files.Tzx.Blocks;

namespace OldBit.Spectron.Files.Tests.Tzx;

public class SelectBlockTests
{
    [Fact]
    public void SelectBlock_NewInstance_ShouldCreateWithDefaultValues()
    {
        var block = new SelectBlock();

        block.Selections.Count().ShouldBe(0);
    }

    [Fact]
    public void SelectBlock_ShouldDeserializeFromStream()
    {
        using var stream = new MemoryStream([
            0x0B, 0x00, 0x02, 0xFE, 0xFF, 0x02, 0x5A, 0x58, 0x01, 0x00, 0x02, 0x38, 0x31
        ]);
        var block = new SelectBlock(new ByteStreamReader(stream));

        block.Selections.Count().ShouldBe(2);
        block.Selections[0].Offset.ShouldBe(-2);
        block.Selections[0].Length.ShouldBe(2);
        block.Selections[0].Description.ShouldBe("ZX");
        block.Selections[1].Offset.ShouldBe(1);
        block.Selections[1].Length.ShouldBe(2);
        block.Selections[1].Description.ShouldBe("81");
    }

    [Fact]
    public void SelectBlock_ShouldSerializeToBytes()
    {
        var block = new SelectBlock
        {
            Selections =
            [
                new SelectBlock.Selection { Offset = 10, Description = "Selection 1" },
                new SelectBlock.Selection { Offset = 20, Description = "Selection 2" }
            ]
        };

        var result = FileDataSerializer.Serialize(block);

        result.ShouldBeEquivalentTo(new byte[]
        {
            0x28, 0x1D, 0x00, 0x02, 0x0A, 0x00, 0x0B, 0x53, 0x65, 0x6c,
            0x65, 0x63, 0x74, 0x69, 0x6f, 0x6e, 0x20, 0x31, 0x14, 0x00, 0x0B, 0x53, 0x65,
            0x6c, 0x65, 0x63, 0x74, 0x69, 0x6f, 0x6e, 0x20, 0x32
        });
    }
}