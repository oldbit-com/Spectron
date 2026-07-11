using OldBit.Spectron.Files.Serialization;

namespace OldBit.Spectron.Files.Sna;

/// <summary>
/// Represents additional header in a 128K SNA file.
/// </summary>
public sealed class SnaHeader128
{
    /// <summary>
    /// Gets or sets the PC register.
    /// </summary>
    [FileData(Order = 0)]
    public Word PC { get; set; }

    /// <summary>
    /// Gets or sets the page mode (port 7FFD value).
    /// </summary>
    [FileData(Order = 1)]
    public byte PageMode { get; set; }

    /// <summary>
    /// Gets or sets the TR-DOS ROM paged flag (1=yes, 0=no).
    /// </summary>
    [FileData(Order = 2)]
    public byte TrDosRom { get; set; }
}