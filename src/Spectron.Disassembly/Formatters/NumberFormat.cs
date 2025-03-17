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
    /// Format without a prefix or suffix, for example FF.
    /// </summary>
    Hex,

    /// <summary>
    /// Format with a dollar sign prefix, for example $FF.
    /// </summary>
    HexPrefixDollar,

    /// <summary>
    /// Format with a 'h' suffix, for example FFh.
    /// </summary>
    HexSuffixH,

    /// <summary>
    /// Format with a '0x' suffix, for example 0xFF.
    /// </summary>
    HexPrefix0X,

    /// <summary>
    /// Format with a hash sign prefix, for example #FF.
    /// </summary>
    HexPrefixHash,
}