using OldBit.Spectron.Disassembly.Formatters;

namespace OldBit.Spectron.Disassembly.Tests;

public class NumberFormatterTests
{
    [Theory]
    [InlineData(171, NumberFormat.Decimal, "171")]
    [InlineData(171, NumberFormat.HexPrefixDollar, "$AB")]
    [InlineData(171, NumberFormat.HexSuffixH, "ABh")]
    [InlineData(171, NumberFormat.HexPrefix0X, "0xAB")]
    [InlineData(171, NumberFormat.HexPrefixHash, "#AB")]
    public void ShouldFormatByteValue(byte value, NumberFormat format, string expected)
    {
        var formatter = new NumberFormatter(format);

        var result = formatter.Format(value);

        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData(23, NumberFormat.Decimal, "+23")]
    [InlineData(-23, NumberFormat.Decimal, "-23")]
    [InlineData(23, NumberFormat.HexPrefixDollar, "+$17")]
    [InlineData(-23, NumberFormat.HexPrefixDollar, "-$17")]
    [InlineData(23, NumberFormat.HexSuffixH, "+17h")]
    [InlineData(-23, NumberFormat.HexSuffixH, "-17h")]
    [InlineData(23, NumberFormat.HexPrefix0X, "+0x17")]
    [InlineData(-23, NumberFormat.HexPrefix0X, "-0x17")]
    [InlineData(23, NumberFormat.HexPrefixHash, "+#17")]
    [InlineData(-23, NumberFormat.HexPrefixHash, "-#17")]
    [InlineData(0, NumberFormat.HexPrefix0X, "")]
    public void ShouldFormatOffsetValue(sbyte value, NumberFormat format, string expected)
    {
        var formatter = new NumberFormatter(format);

        var result = formatter.FormatOffset(value);

        result.ShouldBe(expected);
    }
}