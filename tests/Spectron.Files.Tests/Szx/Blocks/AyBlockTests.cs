using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Szx.Blocks;

namespace OldBit.Spectron.Files.Tests.Szx.Blocks;

public class AyBlockTests
{
    [Fact]
    public void Ay_ShouldConvertToBytes()
    {
        var ay = GetAyBlock();
        using var writer = new MemoryStream();

        ay.Write(writer);

        var data = writer.ToArray();
        data.Length.ShouldBe(8 + 18);

        // Header
        BitConverter.ToUInt32(data[..4].ToArray()).ShouldBe((DWord)0x00005941);
        BitConverter.ToUInt32(data[4..8].ToArray()).ShouldBe((DWord)18);

        // Data
        data[8].ShouldBe(AyBlock.Flags128Ay);
        data[9].ShouldBe(0x0F);
        data[10..26].ToArray().ShouldBeEquivalentTo(Enumerable.Range(1, 16).Select(i => (byte)i).ToArray());
    }

    [Fact]
    public void Ay_ShouldConvertFromBytes()
    {
        var ayData = GetAyBlockData();
        using var memoryStream = new MemoryStream(ayData);
        var reader = new ByteStreamReader(memoryStream);

        var ay = AyBlock.Read(reader, ayData.Length);

        ay.Flags.ShouldBe(AyBlock.Flags128Ay);
        ay.CurrentRegister.ShouldBe(0x0F);
        ay.Registers.ShouldBeEquivalentTo(Enumerable.Range(1, 16).Select(i => (byte)i).ToArray());
    }

    private static byte[] GetAyBlockData()
    {
        var ay = GetAyBlock();
        using var writer = new MemoryStream();

        ay.Write(writer);

        return writer.ToArray()[8..].ToArray();
    }

    private static AyBlock GetAyBlock()
    {
        var ay = new AyBlock
        {
            Flags = AyBlock.Flags128Ay,
            CurrentRegister = 0x0F,
        };

        for (byte i = 1; i <= 16; i++)
        {
            ay.Registers[i - 1] = i;
        }

        return ay;
    }
}