using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Serialization;
using OldBit.Spectron.Files.Tzx.Blocks;

namespace OldBit.Spectron.Files.Tests.Tzx;

public class GeneralizedDataBlockTests
{
    [Fact]
    public void GeneralizedDataBlock_NewInstance_ShouldCreateWithDefaultValues()
    {
        var block = new GeneralizedDataBlock();

        block.Length.ShouldBe(14);
        block.PauseDuration.ShouldBe(0);
    }

    [Fact]
    public void DirectRecordingBlock_ShouldDeserializeFromStream()
    {
        // A typical Spectrum's standard loading header
        var bytes = new byte[] {
            0x3B, 0x00, 0x00, 0x00, 0xE8, 0x03,
            0x02, 0x00, 0x00, 0x00, 0x02, 0x02,
            0x98, 0x00, 0x00, 0x00, 0x02, 0x02,
            0x00, 0x78, 0x08, 0x00, 0x00, // SYMDEF[0]: (0, 2168, 0)
            0x00, 0x9B, 0x02, 0xDF, 0x02, // SYMDEF[1]: (0, 667, 735)
            0x00, 0x7F, 0x1F,             // PRLE[0]: (0, 8063)
            0x01, 0x01, 0x00,             // PRLE[1]: (1, 1)
            0x00, 0x57, 0x03, 0x57, 0x03, // SYMDEF[0]: (0, 855, 855)
            0x00, 0xAE, 0x06, 0xAE, 0x06, // SYMDEF[1]: (0, 1710, 1710)
            0x00, 0x03, 0x4A, 0x50, 0x53, 0x50, 0x20, 0x20, 0x20, 0x20,
            0x20, 0x20, 0x00, 0x1B, 0x00, 0x40, 0x00, 0x80, 0xC1
        };
        using var stream = new MemoryStream(bytes);
        var block = new GeneralizedDataBlock(new ByteStreamReader(stream));

        block.Length.ShouldBe(59);
        block.PauseDuration.ShouldBe(1000);
        block.PilotSymbols.Count.ShouldBe(2);
        block.PilotSymbols[0].Flags.ShouldBe(0);
        block.PilotSymbols[0].PulseLengths.Count.ShouldBe(2);
        block.PilotSymbols[0].PulseLengths[0].ShouldBe(2168);
        block.PilotSymbols[0].PulseLengths[1].ShouldBe(0);
        block.PilotSymbols[1].Flags.ShouldBe(0);
        block.PilotSymbols[1].PulseLengths.Count.ShouldBe(2);
        block.PilotSymbols[1].PulseLengths[0].ShouldBe(667);
        block.PilotSymbols[1].PulseLengths[1].ShouldBe(735);
        block.PilotDataStream.Count.ShouldBe(2);
        block.PilotDataStream[0].Symbol.ShouldBe(0);
        block.PilotDataStream[0].RepeatCount.ShouldBe(8063);
        block.PilotDataStream[1].Symbol.ShouldBe(1);
        block.PilotDataStream[1].RepeatCount.ShouldBe(1);
        block.DataSymbols.Count.ShouldBe(2);
        block.DataSymbols[0].Flags.ShouldBe(0);
        block.DataSymbols[0].PulseLengths.Count.ShouldBe(2);
        block.DataSymbols[0].PulseLengths[0].ShouldBe(855);
        block.DataSymbols[0].PulseLengths[1].ShouldBe(855);
        block.DataSymbols[1].Flags.ShouldBe(0);
        block.DataSymbols[1].PulseLengths.Count.ShouldBe(2);
        block.DataSymbols[1].PulseLengths[0].ShouldBe(1710);
        block.DataSymbols[1].PulseLengths[1].ShouldBe(1710);
        block.DataStream.Count.ShouldBe(19);
        block.DataStream.ShouldBeEquivalentTo(bytes.Skip(44).ToList());
    }

    [Fact]
    public void GeneralizedDataBlock_ShouldSerializeToBytes()
    {
        var block = new GeneralizedDataBlock
        {
            PauseDuration = 1000,
            PilotSymbols =
            [
                new GeneralizedDataBlock.SymbolDef { Flags = 0, PulseLengths = [2168, 0] },
                new GeneralizedDataBlock.SymbolDef { Flags = 0, PulseLengths = [667, 735] }
            ],
            PilotDataStream =
            [
                new GeneralizedDataBlock.Prle { Symbol = 0, RepeatCount = 8063 },
                new GeneralizedDataBlock.Prle { Symbol = 1, RepeatCount = 1 }
            ],
            DataSymbols =
            [
                new GeneralizedDataBlock.SymbolDef { Flags = 0, PulseLengths = [855, 855] },
                new GeneralizedDataBlock.SymbolDef { Flags = 0, PulseLengths = [1710, 1710] }
            ],
            DataStream =
            [
                0x00, 0x03, 0x4A, 0x50, 0x53, 0x50, 0x20, 0x20, 0x20, 0x20,
                0x20, 0x20, 0x00, 0x1B, 0x00, 0x40, 0x00, 0x80, 0xC1
            ]
        };

        var result = FileDataSerializer.Serialize(block);

        result.ShouldBeEquivalentTo(new byte[]
        {
            0x19,
            0x3B, 0x00, 0x00, 0x00, 0xE8, 0x03,
            0x02, 0x00, 0x00, 0x00, 0x02, 0x02,
            0x98, 0x00, 0x00, 0x00, 0x02, 0x02,
            0x00, 0x78, 0x08, 0x00, 0x00, // SYMDEF[0]: (0, 2168, 0)
            0x00, 0x9B, 0x02, 0xDF, 0x02, // SYMDEF[1]: (0, 667, 735)
            0x00, 0x7F, 0x1F,             // PRLE[0]: (0, 8063)
            0x01, 0x01, 0x00,             // PRLE[1]: (1, 1)
            0x00, 0x57, 0x03, 0x57, 0x03, // SYMDEF[0]: (0, 855, 855)
            0x00, 0xAE, 0x06, 0xAE, 0x06, // SYMDEF[1]: (0, 1710, 1710)
            0x00, 0x03, 0x4A, 0x50, 0x53, 0x50, 0x20, 0x20, 0x20, 0x20,
            0x20, 0x20, 0x00, 0x1B, 0x00, 0x40, 0x00, 0x80, 0xC1
        });
    }
}