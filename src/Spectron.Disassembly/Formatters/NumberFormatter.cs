namespace OldBit.Spectron.Disassembly.Formatters;

public sealed class NumberFormatter(NumberFormat numberFormat)
{
    public string Format(byte value) => Format(value, numberFormat);

    public static string Format(byte value, NumberFormat numberFormat) => Format(value, 2, numberFormat);

    public string Format(Word value) => Format(value, numberFormat);

    public static string Format(int value, Type type, NumberFormat numberFormat)
    {
        if (type == typeof(byte))
        {
            return Format((byte)value, numberFormat);
        }

        return Format((Word)value, numberFormat);
    }

    public static string Format(int value, int digits, NumberFormat numberFormat)
    {
        var hex = value.ToString($"X{digits}");

        return numberFormat switch
        {
            NumberFormat.Hex => hex,
            NumberFormat.HexPrefixDollar => $"${hex}",
            NumberFormat.HexSuffixH => $"{hex}h",
            NumberFormat.HexPrefix0X => $"0x{hex}",
            NumberFormat.HexPrefixHash => $"#{hex}",
            _  => value.ToString(),
        };
    }

    public static string Format(Word value, NumberFormat numberFormat) => Format(value, 4, numberFormat);

    internal string Format(string value) => numberFormat switch
    {
        NumberFormat.Hex => value,
        NumberFormat.HexPrefixDollar => $"${value}",
        NumberFormat.HexSuffixH => $"{value}h",
        NumberFormat.HexPrefix0X => $"0x{value}",
        NumberFormat.HexPrefixHash => $"#{value}",
        _ => value
    };


    internal string FormatOffset(sbyte value)
    {
        if (value == 0)
        {
            return string.Empty;
        }

        var sign = value < 0 ? "-" : "+";
        var hex = Format(Math.Abs(value), 2, numberFormat);

        return $"{sign}{hex}";
    }
}