using OldBit.Spectron.Emulation.Screen;
using OldBit.Spectron.Extensions;

namespace OldBit.Spectron.Tests.Extensions;

public class ColorExtensionsTests
{
    [Theory]
    [InlineData("#FF8000", 0xFF, 0x80, 0x00)]
    [InlineData("#ff8000", 0xFF, 0x80, 0x00)]
    [InlineData("FF8000", 0xFF, 0x80, 0x00)]
    [InlineData("  #FF8000  ", 0xFF, 0x80, 0x00)]
    [InlineData("#F80", 0xFF, 0x88, 0x00)]
    [InlineData("#0F80", 0xFF, 0x88, 0x00)]
    [InlineData("#FFFF8000", 0xFF, 0x80, 0x00)]
    [InlineData("#00FF8000", 0xFF, 0x80, 0x00)]
    public void TryParseHex_ValidHex_ReturnsOpaqueColor(string text, byte red, byte green, byte blue)
    {
        var result = ColorExtensions.TryParseHex(text, out var color);

        result.ShouldBeTrue();
        color.ShouldBe(Avalonia.Media.Color.FromRgb(red, green, blue));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("#")]
    [InlineData("#FF8000F")]
    [InlineData("#GG8000")]
    [InlineData("red")]
    [InlineData("#red")]
    public void TryParseHex_InvalidHex_ReturnsFalse(string? text)
    {
        var result = ColorExtensions.TryParseHex(text, out var color);

        result.ShouldBeFalse();
        color.ShouldBe(default);
    }

    [Fact]
    public void ToHex_ReturnsRgbHex()
    {
        var color = new Color(0xD7, 0x00, 0x1A);

        color.ToHex().ShouldBe("#D7001A");
    }

    [Fact]
    public void FromHex_ValidHex_RoundTripsWithToHex()
    {
        var color = Color.FromHex("#FFD7001A");

        color.ShouldBe(new Color(0xD7, 0x00, 0x1A));
        color.ToHex().ShouldBe("#D7001A");
    }

    [Fact]
    public void FromHex_InvalidHex_ReturnsBlack()
    {
        var color = Color.FromHex("not a colour");

        color.ShouldBe(new Color(0, 0, 0));
    }
}
