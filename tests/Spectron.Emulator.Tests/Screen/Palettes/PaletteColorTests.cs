using OldBit.Spectron.Emulation.Screen;
using OldBit.Spectron.Emulation.Screen.Palettes;

namespace OldBit.Spectron.Emulator.Tests.Screen.Palettes;

public class PaletteColorTests
{
    private static readonly PaletteColor Colors = new(
        new Color(0, 0, 0), new Color(1, 1, 1), new Color(2, 2, 2), new Color(3, 3, 3),
        new Color(4, 4, 4), new Color(5, 5, 5), new Color(6, 6, 6), new Color(7, 7, 7));

    [Fact]
    public void ToArray_ReturnsColorsInSpectrumIndexOrder()
    {
        var colors = Colors.ToArray();

        colors.ShouldBe([
            Colors.Black, Colors.Blue, Colors.Red, Colors.Magenta,
            Colors.Green, Colors.Cyan, Colors.Yellow, Colors.White]);
    }

    [Fact]
    public void From_ToArray_RoundTrips()
    {
        var colors = PaletteColor.From(Colors.ToArray());

        colors.ShouldBe(Colors);
    }

    [Theory]
    [InlineData(7)]
    [InlineData(9)]
    public void From_WrongNumberOfColors_Throws(int count)
    {
        var colors = new Color[count];

        Should.Throw<ArgumentException>(() => PaletteColor.From(colors));
    }
}
