using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Szx.Blocks;

namespace OldBit.Spectron.Files.Tests.Szx.Blocks;

public class PaletteBlockTests
{
    [Fact]
    public void Palette_ShouldConvertToBytes()
    {
        var palette = GetPaletteBlock();
        using var writer = new MemoryStream();

        palette.Write(writer);

        var data = writer.ToArray();
        data.Length.ShouldBe(8 + 66);

        // Header
        BitConverter.ToUInt32(data[..4].ToArray()).ShouldBe((DWord)0x54544C50);
        BitConverter.ToUInt32(data[4..8].ToArray()).ShouldBe((DWord)66);

        // Data
        data[8].ShouldBe(PaletteBlock.FlagsPaletteEnabled);
        data[9].ShouldBe(0x0F);
        data[10..74].ToArray().ShouldBeEquivalentTo(Enumerable.Range(1, 64).Select(i => (byte)i).ToArray());
    }

    [Fact]
    public void Palette_ShouldConvertFromBytes()
    {
        var paletteData = GetPaletteBlockData();
        using var memoryStream = new MemoryStream(paletteData);
        var reader = new ByteStreamReader(memoryStream);

        var palette = PaletteBlock.Read(reader, paletteData.Length);

        palette.Flags.ShouldBe(PaletteBlock.FlagsPaletteEnabled);
        palette.CurrentRegister.ShouldBe(0x0F);
        palette.Registers.ShouldBeEquivalentTo(Enumerable.Range(1, 64).Select(i => (byte)i).ToArray());
    }

    private static byte[] GetPaletteBlockData()
    {
        var palette = GetPaletteBlock();
        using var writer = new MemoryStream();

        palette.Write(writer);

        return writer.ToArray()[8..].ToArray();
    }

    private static PaletteBlock GetPaletteBlock()
    {
        var palette = new PaletteBlock
        {
            Flags = PaletteBlock.FlagsPaletteEnabled,
            CurrentRegister = 0x0F,
        };

        for (byte i = 1; i <= 64; i++)
        {
            palette.Registers[i - 1] = i;
        }

        return palette;
    }
}