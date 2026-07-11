using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Szx.Blocks;

namespace OldBit.Spectron.Files.Tests.Szx.Blocks;

public class TimexSinclairBlockTests
{
    [Fact]
    public void TimexSinclair_ShouldConvertToBytes()
    {
        var timex = GetTimexSinclairBlock();
        using var writer = new MemoryStream();

        timex.Write(writer);

        var data = writer.ToArray();
        data.Length.ShouldBe(8 + 2);

        // Header
        BitConverter.ToUInt32(data[..4].ToArray()).ShouldBe(0x444C4353);
        BitConverter.ToUInt32(data[4..8].ToArray()).ShouldBe(2);

        // Data
        data[8].ShouldBe(0xF4);
        data[9].ShouldBe(0xFF);
    }

    [Fact]
    public void TimexSinclair_ShouldConvertFromBytes()
    {
        var timexData = GetTimexSinclairBlockData();
        using var memoryStream = new MemoryStream(timexData);
        var reader = new ByteStreamReader(memoryStream);

        var timex = TimexSinclairBlock.Read(reader, timexData.Length);

        timex.PortF4.ShouldBe(0xF4);
        timex.PortFF.ShouldBe(0xFF);
    }

    private static byte[] GetTimexSinclairBlockData()
    {
        var timex = GetTimexSinclairBlock();
        using var writer = new MemoryStream();

        timex.Write(writer);

        return writer.ToArray()[8..].ToArray();
    }

    private static TimexSinclairBlock GetTimexSinclairBlock() => new()
    {
        PortF4 = 0xF4,
        PortFF = 0xFF
    };
}