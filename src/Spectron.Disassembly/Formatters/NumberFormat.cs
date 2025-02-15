namespace OldBit.Spectron.Disassembly.Formatters;

/// <summary>
/// Specifies the number format that should be used when formatting numbers.
/// </summary>
public enum NumberFormat
{
    /// <summary>
    /// Use decimal format.
    /// </summary>
    Decimal,

    /// <summary>
    /// Use hexadecimal format without a prefix or suffix, for example FF.
    /// </summary>
    Hex,

    /// <summary>
    /// Use hexadecimal format with a dollar sign prefix, for example $FF.
    /// </summary>
    HexDollarPrefix,

    /// <summary>
    /// Use hexadecimal format with a 'h' suffix, for example FFh.
    /// </summary>
    HexHSuffix,

    /// <summary>
    /// Use hexadecimal format with a '0x' suffix, for example 0xFF.
    /// </summary>
    HexXPrefix,
}