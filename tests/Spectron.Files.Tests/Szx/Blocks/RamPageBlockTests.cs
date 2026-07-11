using System.IO.Compression;
using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Szx;
using OldBit.Spectron.Files.Szx.Blocks;

namespace OldBit.Spectron.Files.Tests.Szx.Blocks;

public class RamPageBlockTests
{
     private readonly byte[] _ramData;

    public RamPageBlockTests()
    {
        var random = new Random(1982);
        _ramData = Enumerable.Range(0, 16384).Select(_ => (byte)random.Next(8, 16)).ToArray();
    }

    [Theory]
    [InlineData(true, 7544)]
    [InlineData(false, 16387)]
    public void RamPage_ShouldConvertToBytes(bool isCompressed, DWord expectedSize)
    {
        var ramPage = GetRamPageBlock(isCompressed);
        using var writer = new MemoryStream();

        ramPage.Write(writer);

        var data = writer.ToArray();
        data.Length.ShouldBe((int)(8 + expectedSize));

        // Header
        BitConverter.ToUInt32(data[..4].ToArray()).ShouldBe(0x504D4152);
        BitConverter.ToUInt32(data[4..8].ToArray()).ShouldBe(expectedSize);

        // Data
        BitConverter.ToUInt16(data[8..10].ToArray()).ShouldBe((Word)(isCompressed ? RamPageBlock.FlagsCompressed : 0));
        data[10].ShouldBe(5);
        if (!isCompressed)
        {
            data[11..].ToArray().ShouldBeEquivalentTo(_ramData);
        }
        else
        {
            var compressedData = ZLibHelper.Compress(_ramData, CompressionLevel.SmallestSize);
            data[11..].ToArray().ShouldBeEquivalentTo(compressedData);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void  RamPage_ShouldConvertFromBytes(bool compress)
    {
        var ramPageData = GetRamPageBlockData(compress);
        using var memoryStream = new MemoryStream(ramPageData);
        var reader = new ByteStreamReader(memoryStream);

        var ramPage = RamPageBlock.Read(reader, ramPageData.Length);

        ramPage.Flags.ShouldBe((Word)(compress ? RamPageBlock.FlagsCompressed : 0));
        ramPage.PageNumber.ShouldBe(5);
        ramPage.Data.ShouldBeEquivalentTo(_ramData);
    }

    private byte[] GetRamPageBlockData(bool compress)
    {
        var ramPage = GetRamPageBlock(compress);
        using var writer = new MemoryStream();

        ramPage.Write(writer);

        return writer.ToArray()[8..].ToArray();
    }

    private RamPageBlock GetRamPageBlock(bool isCompressed) =>
        new(_ramData, 5, isCompressed ? CompressionLevel.SmallestSize : CompressionLevel.NoCompression);
}