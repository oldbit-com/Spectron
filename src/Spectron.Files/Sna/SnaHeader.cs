using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Serialization;

namespace OldBit.Spectron.Files.Sna;

/// <summary>
/// Represents the .sna file header.
/// </summary>
public sealed class SnaHeader
{
    /// <summary>
    /// Gets or sets the I register.
    /// </summary>
    [FileData(Order = 0)]
    public byte I { get; set; }

    /// <summary>
    /// Gets or sets the HL' register.
    /// </summary>
    [FileData(Order = 1)]
    public Word HLPrime { get; set; }

    /// <summary>
    /// Gets or sets the DE' register.
    /// </summary>
    [FileData(Order = 2)]
    public Word DEPrime { get; set; }

    /// <summary>
    /// Gets or sets the BC' register.
    /// </summary>
    [FileData(Order = 3)]
    public Word BCPrime { get; set; }

    /// <summary>
    /// Gets or sets the AF' register.
    /// </summary>
    [FileData(Order = 4)]
    public Word AFPrime { get; set; }

    /// <summary>
    /// Gets or sets the HL register.
    /// </summary>
    [FileData(Order = 5)]
    public Word HL { get; set; }

    /// <summary>
    /// Gets or sets the DE register.
    /// </summary>
    [FileData(Order = 6)]
    public Word DE { get; set; }

    /// <summary>
    /// Gets or sets the BC register.
    /// </summary>
    [FileData(Order = 7)]
    public Word BC { get; set; }

    /// <summary>
    /// Gets or sets the IY register.
    /// </summary>
    [FileData(Order = 8)]
    public Word IY { get; set; }

    /// <summary>
    /// Gets or sets the IX register.
    /// </summary>
    [FileData(Order = 9)]
    public Word IX { get; set; }

    /// <summary>
    /// Gets or sets the interrupt (bit 2 contains IFF2, 1=EI/0=DI)
    /// </summary>
    [FileData(Order = 10)]
    public byte Interrupt { get; set; }

    /// <summary>
    /// Gets or sets the R register.
    /// </summary>
    [FileData(Order = 11)]
    public byte R { get; set; }

    /// <summary>
    /// Gets or sets the AF register.
    /// </summary>
    [FileData(Order = 12)]
    public Word AF { get; set; }

    /// <summary>
    /// Gets or sets the SP register.
    /// </summary>
    [FileData(Order = 13)]
    public Word SP { get; set; }

    /// <summary>
    /// Gets or sets the interrupt mode.
    /// </summary>
    [FileData(Order = 14)]
    public byte InterruptMode { get; set; }

    /// <summary>
    /// Gets or sets the border color.
    /// </summary>
    [FileData(Order = 15)]
    public byte BorderColor { get; set; }

    /// <summary>
    /// Creates a new instance of the 48K SNA header.
    /// </summary>
    public SnaHeader()
    {
    }

    /// <summary>
    /// Creates a new instance of the 48K SNA header.
    /// </summary>
    /// <param name="reader">The ByteStreamReader used to initialize the SnaHeader properties.</param>
    internal SnaHeader(ByteStreamReader reader)
    {
        I = reader.ReadByte();
        HLPrime = reader.ReadWord();
        DEPrime = reader.ReadWord();
        BCPrime = reader.ReadWord();
        AFPrime = reader.ReadWord();
        HL = reader.ReadWord();
        DE = reader.ReadWord();
        BC = reader.ReadWord();
        IY = reader.ReadWord();
        IX = reader.ReadWord();
        Interrupt = reader.ReadByte();
        R = reader.ReadByte();
        AF = reader.ReadWord();
        SP = reader.ReadWord();
        InterruptMode = reader.ReadByte();
        BorderColor = reader.ReadByte();
    }
}