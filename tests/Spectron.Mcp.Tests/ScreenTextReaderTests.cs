using OldBit.Spectron.Mcp.Screen;
using OldBit.Spectron.Mcp.Tests.Fixtures;

namespace OldBit.Spectron.Mcp.Tests;

public class ScreenTextReaderTests
{
    private static readonly byte[] LetterA = [0x00, 0x3C, 0x42, 0x42, 0x7E, 0x42, 0x42, 0x00];

    [Theory]
    [InlineData(0, 0, 0, 0x0000)]
    [InlineData(0, 1, 0, 0x0100)]
    [InlineData(1, 0, 0, 0x0020)]
    [InlineData(8, 0, 0, 0x0800)]
    [InlineData(23, 7, 31, 0x17FF)]
    public void GetBitmapAddress_ShouldFollowSpectrumScreenLayout(int row, int pixelLine, int column, int expected)
    {
        ScreenTextReader.GetBitmapAddress(row, pixelLine, column).ShouldBe((Word)expected);
    }

    [Fact]
    public void Recognize_ShouldMatchNormalAndInverseCharacters()
    {
        var reader = CreateReaderWithLetterA();

        reader.Recognize(LetterA).ShouldBe('A');
        reader.Recognize(LetterA.Select(b => (byte)~b).ToArray()).ShouldBe('A');
    }

    [Fact]
    public void Recognize_ShouldMatchBlankAndBlockGraphics()
    {
        var reader = CreateReaderWithLetterA();

        reader.Recognize(new byte[8]).ShouldBe(' ');
        reader.Recognize([0xF0, 0xF0, 0xF0, 0xF0, 0, 0, 0, 0]).ShouldBe('▘');
    }

    [Fact]
    public void Recognize_ShouldReturnUnknownCharForGraphics()
    {
        var reader = CreateReaderWithLetterA();

        reader.Recognize([0x81, 0x42, 0x24, 0x18, 0x18, 0x24, 0x42, 0x81]).ShouldBe(ScreenTextReader.UnknownChar);
    }

    [Fact]
    public void ReadLines_ShouldReadCopyrightMessageAfterBoot()
    {
        var host = new TestEmulatorHost();
        var memory = host.Emulator!.Memory;

        EmulatorController.RunFrames(host.Emulator, 150);

        var lines = ScreenTextReader.Create(memory).ReadLines(memory);

        lines.Length.ShouldBe(24);
        lines[23].ShouldBe("© 1982 Sinclair Research Ltd");
        lines[..23].ShouldAllBe(line => line == string.Empty);
    }

    private static ScreenTextReader CreateReaderWithLetterA()
    {
        var font = new byte[96 * 8];
        LetterA.CopyTo(font, ('A' - 0x20) * 8);

        return new ScreenTextReader(font);
    }
}
