using FluentAssertions;
using OldBit.ZXSpectrum.Emulator.Screen;

namespace ZXSpectrum.Emulator.UnitTests;

public class BorderColorTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(3847)]
    [InlineData(5678)]
    [InlineData(63626)]
    public void BorderColor_ShouldBeCyan(int clockCycle)
    {
        var borderState = new Border();
        borderState.ChangeBorderColor(5, 0);

        var color = borderState.GetBorderColor(clockCycle);

        color.Should().Be(Colors.Cyan);
    }

    [Theory]
    [InlineData(3847, "Cyan")]
    [InlineData(4332, "Cyan")]
    [InlineData(4333, "Red")]
    [InlineData(4334, "Red")]
    [InlineData(7362, "Red")]
    [InlineData(7363, "Blue")]
    [InlineData(7364, "Blue")]
    [InlineData(18222, "Magenta")]
    public void BorderColor_ShouldBeExpectedColor(int clockCycle, string expectedColor)
    {
        var borderState = new Border();
        borderState.ChangeBorderColor(5, 0);
        borderState.ChangeBorderColor(2, 4333);
        borderState.ChangeBorderColor(1, 7363);
        borderState.ChangeBorderColor(3, 18222);

        var color = borderState.GetBorderColor(clockCycle);

        color.Should().Be(ColorNames[expectedColor]);
    }

    private static readonly Dictionary<string, Color> ColorNames = new()
    {
        { "Cyan", Colors.Cyan },
        { "Black", Colors.Black },
        { "Blue", Colors.Blue },
        { "Red", Colors.Red },
        { "Magenta", Colors.Magenta },
        { "Green", Colors.Green },
        { "Yellow", Colors.Yellow },
        { "White", Colors.White }
    };
}