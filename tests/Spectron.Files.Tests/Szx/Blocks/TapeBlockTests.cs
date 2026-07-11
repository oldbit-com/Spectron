using System.IO.Compression;
using System.Text;
using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Szx;
using OldBit.Spectron.Files.Szx.Blocks;

namespace OldBit.Spectron.Files.Tests.Szx.Blocks;

public class TapeBlockTests
{
private readonly byte[] _tapeData;

    public TapeBlockTests()
    {
        var random = new Random(1970);
        _tapeData = Enumerable.Range(0, 4560).Select(_ => (byte)random.Next(0, 16)).ToArray();
    }

    [Theory]
    [InlineData(true, 2645)]
    [InlineData(false, 4588)]
    public void Tape_ShouldConvertToBytes(bool isCompressed, DWord expectedSize)
    {
        var tape = GetTapeBlock(isCompressed);
        using var writer = new MemoryStream();

        tape.Write(writer);

        var data = writer.ToArray();
        data.Length.ShouldBe((int)(8 + expectedSize));

        // Header
        BitConverter.ToUInt32(data[..4].ToArray()).ShouldBe(0x45504154);
        BitConverter.ToUInt32(data[4..8].ToArray()).ShouldBe(expectedSize);

        // Data
        BitConverter.ToUInt16(data[8..10].ToArray()).ShouldBe(2);
        BitConverter.ToUInt16(data[10..12].ToArray()).ShouldBe((Word)(isCompressed ? TapeBlock.FlagsCompressed + TapeBlock.FlagsEmbedded : TapeBlock.FlagsEmbedded));
        BitConverter.ToUInt32(data[12..16].ToArray()).ShouldBe(4560);
        BitConverter.ToUInt32(data[16..20].ToArray()).ShouldBe((DWord)(isCompressed ? 2617 : 4560));
        Encoding.ASCII.GetString(data[20..36]).Trim('\0').ShouldBe(".tap");
        if (!isCompressed)
        {
            data[36..].ToArray().ShouldBeEquivalentTo(_tapeData);
        }
        else
        {
            var compressedData = ZLibHelper.Compress(_tapeData, CompressionLevel.SmallestSize);
            data[36..].ToArray().ShouldBeEquivalentTo(compressedData);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Tape_ShouldConvertFromBytes(bool compress)
    {
        var tapeData = GetTapeBlockData(compress);
        using var memoryStream = new MemoryStream(tapeData);
        var reader = new ByteStreamReader(memoryStream);

        var tape = TapeBlock.Read(reader, tapeData.Length);

        tape.CurrentBlockNo.ShouldBe(2);
        tape.Flags.ShouldBe((Word)(compress ? TapeBlock.FlagsCompressed + TapeBlock.FlagsEmbedded : TapeBlock.FlagsEmbedded));
        tape.UncompressedSize.ShouldBe(4560);
        tape.CompressedSize.ShouldBe((Word)(compress ? 2617 : tape.Data.Length));
        tape.FileExtension.ShouldBe(".tap");
        tape.Data.ShouldBeEquivalentTo(_tapeData);
    }

    private byte[] GetTapeBlockData(bool compress)
    {
        var tape = GetTapeBlock(compress);
        using var writer = new MemoryStream();

        tape.Write(writer);

        return writer.ToArray()[8..].ToArray();
    }

    private TapeBlock GetTapeBlock(bool isCompressed) =>
        new(_tapeData, isCompressed ? CompressionLevel.SmallestSize : CompressionLevel.NoCompression)
        {
            FileExtension = ".tap",
            CurrentBlockNo = 2,
        };
}