using OldBit.Spectron.Debugger.Converters;

namespace OldBit.Spectron.Debugger.Tests.Converters;

public class HexTests
{
    [Theory]
    [InlineData("12", 18)]
    [InlineData("0x12", 18)]
    [InlineData("0X12", 18)]
    [InlineData("$12", 18)]
    [InlineData("#12", 18)]
    [InlineData("12h", 18)]
    [InlineData("12H", 18)]
    [InlineData("0012H", 18)]
    [InlineData(" 0012H ", 18)]
    public void HexString_ShouldConvertToByte(string input, byte expected)
    {
        var isSuccess = Hex.TryParse<byte>(input, out var result);

        isSuccess.ShouldBeTrue();
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("F2", -14)]
    [InlineData("0xF2", -14)]
    [InlineData("0XF2", -14)]
    [InlineData("$F2", -14)]
    [InlineData("#F2", -14)]
    [InlineData("F2h", -14)]
    [InlineData("F2H", -14)]
    [InlineData("00F2H", -14)]
    [InlineData(" 00F2H ", -14)]
    public void HexString_ShouldConvertToSignedByte(string input, sbyte expected)
    {
        var isSuccess = Hex.TryParse<sbyte>(input, out var result);

        isSuccess.ShouldBeTrue();
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("1234", 4660)]
    [InlineData("0x1234", 4660)]
    [InlineData("0X1234", 4660)]
    [InlineData("$1234", 4660)]
    [InlineData("#1234", 4660)]
    [InlineData("1234h", 4660)]
    [InlineData("1234H", 4660)]
    [InlineData("001234H", 4660)]
    [InlineData(" 001234H ", 4660)]
    public void HexString_ShouldConvertToWord(string input, Word expected)
    {
        var isSuccess = Hex.TryParse<Word>(input, out var result);

        isSuccess.ShouldBeTrue();
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("F234", -3532)]
    [InlineData("0xF234", -3532)]
    [InlineData("0XF234", -3532)]
    [InlineData("$F234", -3532)]
    [InlineData("#F234", -3532)]
    [InlineData("F234h", -3532)]
    [InlineData("F234H", -3532)]
    [InlineData("00F234H", -3532)]
    [InlineData(" 00F234H ", -3532)]
    public void HexString_ShouldConvertToSignedWord(string input, short expected)
    {
        var isSuccess = Hex.TryParse<short>(input, out var result);

        isSuccess.ShouldBeTrue();
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("12345", 12345)]
    [InlineData("1234A", 74570)]
    public void HexString_ShouldPreferDecimal(string input, int expected)
    {
        var isSuccess = Hex.TryParse<int>(input, out var result, preferDecimal: true);

        isSuccess.ShouldBeTrue();
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("120G")]
    [InlineData("test")]
    [InlineData("-123")]
    [InlineData("0x100h")]
    public void InvalidHexString_ShouldNotParse(string input)
    {
        var isSuccess = Hex.TryParse<int>(input, out var _);

        isSuccess.ShouldBeFalse();
    }
}