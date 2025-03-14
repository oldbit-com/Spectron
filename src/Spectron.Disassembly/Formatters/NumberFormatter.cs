namespace OldBit.Spectron.Disassembly.Formatters;

public sealed class NumberFormatter(NumberFormat numberFormat)
{
    public string Format(byte value) => Format(value, numberFormat);

    public static string Format(byte value, NumberFormat numberFormat)
    {
        return numberFormat switch
        {
            NumberFormat.Hex => $"{value:X2}",
            NumberFormat.HexDollarPrefix => $"${value:X2}",
            NumberFormat.HexHSuffix => $"{value:X2}h",
            NumberFormat.HexXPrefix => $"0x{value:X2}",
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

    public static string FormatAutoHex(int value) => value switch
    {
        >= 0 and <= 0xFF => $"{value:X2}",
        <= 0xFFFF => $"{value:X4}",
        <= 0xFFFFFF => $"{value:X6}",
        _ => $"{value:X8}"
    };

    public static string Format(Word value, NumberFormat numberFormat)
    {
        return numberFormat switch
        {
            NumberFormat.Hex => $"{value:X4}",
            NumberFormat.HexDollarPrefix => $"${value:X4}",
            NumberFormat.HexHSuffix => $"{value:X4}h",
            NumberFormat.HexXPrefix => $"0x{value:X4}",
            _  => value.ToString(),
        };
    }

    internal string Format(string value)
    {
        return numberFormat switch
        {
            NumberFormat.Hex => value,
            NumberFormat.HexDollarPrefix => $"${value}",
            NumberFormat.HexHSuffix => $"{value}h",
            NumberFormat.HexXPrefix => $"0x{value}",
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
            NumberFormat.HexDollarPrefix => $"{sign}${Math.Abs(value):X2}",
            NumberFormat.HexHSuffix => $"{sign}{Math.Abs(value):X2}h",
            NumberFormat.HexXPrefix => $"{sign}0x{Math.Abs(value):X2}",
            _  => value.ToString(),
        };
    }
}