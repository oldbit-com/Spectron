using OldBit.Spectron.Disassembly.Formatters;

namespace OldBit.Spectron.Disassembly.Tests;

public class NumberFormatterTests
{
    [Theory]
    [InlineData(171, NumberFormat.Decimal, "171")]
    [InlineData(171, NumberFormat.HexDollarPrefix, "$AB")]
    [InlineData(171, NumberFormat.HexHSuffix, "ABh")]
    [InlineData(171, NumberFormat.HexXPrefix, "0xAB")]
    public void ShouldFormatByteValue(byte value, NumberFormat format, string expected)
    {
        var formatter = new NumberFormatter(format);

        var result = formatter.Format(value);

        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData(23, NumberFormat.Decimal, "+23")]
    [InlineData(-23, NumberFormat.Decimal, "-23")]
    [InlineData(23, NumberFormat.HexDollarPrefix, "+$17")]
    [InlineData(-23, NumberFormat.HexDollarPrefix, "-$17")]
    [InlineData(23, NumberFormat.HexHSuffix, "+17h")]
    [InlineData(-23, NumberFormat.HexXPrefix, "-0x17")]
    [InlineData(0, NumberFormat.HexXPrefix, "")]
    public void ShouldFormatOffsetValue(sbyte value, NumberFormat format, string expected)
    {
        var formatter = new NumberFormatter(format);

        var result = formatter.FormatOffset(value);

        result.ShouldBe(expected);
    }
}