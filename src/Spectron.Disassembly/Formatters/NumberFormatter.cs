namespace OldBit.Spectron.Disassembly.Formatters;

/// <summary>
/// Represents a number formatter that can be used to format numbers in different ways.
/// </summary>
public sealed class NumberFormatter
{
    private readonly NumberFormat _numberFormat;

    /// <summary>
    /// Initializes a new instance of the <see cref="NumberFormatter"/> class with the specified number format.
    /// </summary>
    /// <param name="numberFormat">The number format to use for formatting numbers.</param>
    public NumberFormatter(NumberFormat numberFormat) => _numberFormat = numberFormat;

    /// <summary>
    /// Formats a byte value according to the specified number format.
    /// </summary>
    /// <param name="value">The byte value to format.</param>
    /// <returns>A string representation of the formatted byte value.</returns>
    public string Format(byte value) => Format(value, _numberFormat);

    /// <summary>
    /// Formats a byte value according to the specified number format.
    /// </summary>
    /// <param name="value">The byte value to format.</param>
    /// <param name="numberFormat">The format to use.</param>
    /// <returns>A string representation of the formatted word value.</returns>
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

    /// <summary>
    /// Formats a word value according to the specified number format.
    /// </summary>
    /// <param name="value">The word value to format.</param>
    /// <returns>A string representation of the formatted word value.</returns>
    public string Format(Word value) => Format(value, _numberFormat);

    /// <summary>
    /// Formats a word value according to the specified number format.
    /// </summary>
    /// <param name="value">The word value to format.</param>
    /// <param name="numberFormat">The format to use.</param>
    /// <returns>A string representation of the formatted word value.</returns>
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
        return _numberFormat switch
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

        return _numberFormat switch
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