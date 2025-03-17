namespace OldBit.Spectron.Disassembly.Formatters;

public sealed class NumberFormatter(NumberFormat numberFormat)
{
    public string Format(byte value) => Format(value, numberFormat);

    public static string Format(byte value, NumberFormat numberFormat)
    {
        return numberFormat switch
        {
            NumberFormat.Hex => $"{value:X2}",
            NumberFormat.HexPrefixDollar => $"${value:X2}",
            NumberFormat.HexSuffixH => $"{value:X2}h",
            NumberFormat.HexPrefix0X => $"0x{value:X2}",
            NumberFormat.HexPrefixHash => $"#{value:X2}",
            _  => value.ToString(),
        };
    }

    public string Format(Word value) => Format(value, numberFormat);

    public static string Format(int value, Type type, NumberFormat numberFormat)
    {
        if (type == typeof(byte))
        {
            return Format((byte)value, numberFormat);
        }

        return Format((Word)value, numberFormat);
    }

    public static string Format(Word value, NumberFormat numberFormat)
    {
        return numberFormat switch
        {
            NumberFormat.Hex => $"{value:X4}",
            NumberFormat.HexPrefixDollar => $"${value:X4}",
            NumberFormat.HexSuffixH => $"{value:X4}h",
            NumberFormat.HexPrefix0X => $"0x{value:X4}",
            NumberFormat.HexPrefixHash => $"#{value:X4}",
            _  => value.ToString(),
        };
    }

    internal string Format(string value)
    {
        return numberFormat switch
        {
            NumberFormat.Hex => value,
            NumberFormat.HexPrefixDollar => $"${value}",
            NumberFormat.HexSuffixH => $"{value}h",
            NumberFormat.HexPrefix0X => $"0x{value}",
            NumberFormat.HexPrefixHash => $"#{value}",
            _  => value
        };
    }


    internal string FormatOffset(sbyte value)
    {
        if (value == 0)
        {
            return string.Empty;
        }

        var sign = value < 0 ? "-" : "+";

        return numberFormat switch
        {
            NumberFormat.Hex => $"{sign}{Math.Abs(value):X2}",
            NumberFormat.Decimal => $"{sign}{Math.Abs(value)}",
            NumberFormat.HexPrefixDollar => $"{sign}${Math.Abs(value):X2}",
            NumberFormat.HexSuffixH => $"{sign}{Math.Abs(value):X2}h",
            NumberFormat.HexPrefix0X => $"{sign}0x{Math.Abs(value):X2}",
            NumberFormat.HexPrefixHash => $"{sign}#{Math.Abs(value):X2}",
            _  => value.ToString(),
        };
    }
}