using OldBit.Spectron.Disassembly.Formatters;

namespace OldBit.Spectron.Disassembly;

/// <summary>
/// Represents the options that can be used to configure the disassembler.
/// </summary>
public sealed class DisassemblerOptions
{
    /// <summary>
    /// Gets or sets the number format that should be used when formatting numbers.
    /// </summary>
    public NumberFormat NumberFormat { get; set; } = NumberFormat.HexDollarPrefix;
}