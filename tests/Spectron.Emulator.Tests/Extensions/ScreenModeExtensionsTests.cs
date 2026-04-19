using OldBit.Spectron.Emulation.Extensions;
using OldBit.Spectron.Emulation.Screen;

namespace OldBit.Spectron.Emulator.Tests.Extensions;

public class ScreenModeExtensionsTests
{
    [Theory]
    [InlineData(ScreenMode.TimexHiRes, true)]
    [InlineData(ScreenMode.TimexHiResAttr, true)]
    [InlineData(ScreenMode.TimexHiResAttrAlt, true)]
    [InlineData(ScreenMode.TimexHiResDouble, true)]
    [InlineData(ScreenMode.TimexHiColor, false)]
    [InlineData(ScreenMode.TimexHiColorAlt, false)]
    [InlineData(ScreenMode.Spectrum, false)]
    [InlineData(ScreenMode.TimexSecondScreen, false)]
    public void IsTimexHiRes_ReturnsCorrectResult(ScreenMode screenMode, bool expected)
    {
        screenMode.IsTimexHiRes().ShouldBe(expected);
    }
}
