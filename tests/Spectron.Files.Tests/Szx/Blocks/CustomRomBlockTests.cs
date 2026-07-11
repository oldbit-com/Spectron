using System.IO.Compression;
using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Szx;
using OldBit.Spectron.Files.Szx.Blocks;

namespace OldBit.Spectron.Files.Tests.Szx.Blocks;

public class CustomRomBlockTests
{
    private readonly byte[] _romData;

    public CustomRomBlockTests()
    {
        var random = new Random(1982);
        _romData = Enumerable.Range(0, 16384).Select(_ => (byte)random.Next(0, 8)).ToArray();
    }

    [Theory]
    [InlineData(true, 7546)]
    [InlineData(false, 16390)]
    public void CustomRom_ShouldConvertToBytes(bool isCompressed, DWord expectedSize)
    {
        var customRom = GetCustomRomBlock(isCompressed);
        using var writer = new MemoryStream();

        customRom.Write(writer);

        var data = writer.ToArray();
        data.Length.ShouldBe((int)(8 + expectedSize));

        // Header
        BitConverter.ToUInt32(data[..4].ToArray()).ShouldBe((DWord)0x004D4F52);
        BitConverter.ToUInt32(data[4..8].ToArray()).ShouldBe(expectedSize);

        // Data
        BitConverter.ToUInt16(data[8..10].ToArray()).ShouldBe((Word)(isCompressed ? CustomRomBlock.FlagsCompressed : 0));
        BitConverter.ToUInt32(data[10..14].ToArray()).ShouldBe((DWord)16384);
        if (!isCompressed)
        {
            data[14..].ToArray().ShouldBeEquivalentTo(_romData);
        }
        else
        {
            var compressedData = ZLibHelper.Compress(_romData, CompressionLevel.SmallestSize);
            data[14..].ToArray().ShouldBeEquivalentTo(compressedData);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CustomRom_ShouldConvertFromBytes(bool compress)
    {
        var customRomData = GetCustomRomBlockData(compress);
        using var memoryStream = new MemoryStream(customRomData);
        var reader = new ByteStreamReader(memoryStream);

        var customRom = CustomRomBlock.Read(reader, customRomData.Length);

        customRom.Flags.ShouldBe((Word)(compress ? CustomRomBlock.FlagsCompressed : 0));
        customRom.UncompressedSize.ShouldBe(16384);
        customRom.Data.ShouldBeEquivalentTo(_romData);
    }

    private byte[] GetCustomRomBlockData(bool compress)
    {
        var customRom = GetCustomRomBlock(compress);
        using var writer = new MemoryStream();

        customRom.Write(writer);

        return writer.ToArray()[8..].ToArray();
    }

    private CustomRomBlock GetCustomRomBlock(bool isCompressed) =>
        new(_romData, isCompressed ? CompressionLevel.SmallestSize : CompressionLevel.NoCompression);
}