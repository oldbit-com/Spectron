using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Serialization;

namespace OldBit.Spectron.Files.Tzx.Blocks;

/// <summary>
/// Represents a Kansas City Standard block in a TZX tape file. It is specific to TSX format.
/// </summary>
public class KansasCityStandardBlock : IBlock
{
    /// <summary>
    /// Gets the block ID.
    /// </summary>
    [FileData(Order = 0)]
    public byte BlockId => BlockCode.KansasCityStandard;

    /// <summary>
    /// Gets the block length.
    /// </summary>
    [FileData(Order = 1)]
    public DWord Length => 12 + (DWord)Data.Count;

    /// <summary>
    /// Gets or sets the pause value (in millisecond) that should be applied after this block.
    /// </summary>
    [FileData(Order = 2)]
    public Word PauseDuration { get; set; }


    /// <summary>
    /// Gets or sets the duration of a PILOT pulse in T-states (same as ONE pulse).
    /// </summary>
    [FileData(Order = 3)]
    public Word PilotPulseDuration { get; set; }

    /// <summary>
    /// Gets or sets the number of pulses in the PILOT tone.
    /// </summary>
    [FileData(Order = 4)]
    public Word NumberOfPulsesInPilot { get; set; }

    /// <summary>
    /// Gets or sets the duration of a ZERO pulse in T-states.
    /// </summary>
    [FileData(Order = 5)]
    public Word ZeroPulseDuration { get; set; }

    /// <summary>
    /// Gets or sets the duration of a ONE pulse in T-states.
    /// </summary>
    [FileData(Order = 6)]
    public Word OnePulseDuration { get; set; }

    /// <summary>
    /// Gets or sets the bit configuration.
    /// <remarks>
    ///     Bits 7-4: Number of ZERO pulses in a ZERO bit (0=16 pulses) {4}
    ///     Bits 3-0: Number of ONE pulses in a ONE bit (0=16 pulses) {8}
    /// </remarks>
    /// </summary>
    [FileData(Order = 7)]
    public byte BitConfiguration { get; set; }

    /// <summary>
    /// Gets or sets the byte configuration.
    /// <remarks>
    ///     Bits 7-6: Number of leading bits {1}
    ///     Bit 5: Value of leading bits {0}
    ///     Bits 4-3: Number of trailing bits {2}
    ///     Bit 2: Value of trailing bits {0}
    ///     Bit 1: Reserved
    ///     Bit 0: Endianness (0 for LSb first, 1 for MSb first> {0}
    /// </remarks>
    /// </summary>
    [FileData(Order = 8)]
    public byte ByteConfiguration { get; set; }

    /// <summary>
    /// Gets or sets the data.
    /// </summary>
    [FileData(Order = 9)]
    public List<byte> Data { get; set; } = [];

    /// <summary>
    /// Creates a new instance of the 'Kansas City Standard' block.
    /// </summary>
    public KansasCityStandardBlock()
    {
    }

    /// <summary>
    /// Creates a new instance of the 'Kansas City Standard' block using the byte reader.
    /// </summary>
    /// <param name="reader">The ByteStreamReader used to initialize the KansasCityStandardBlock properties.</param>
    internal KansasCityStandardBlock(ByteStreamReader reader)
    {
        var length = (int)reader.ReadDWord();
        PauseDuration = reader.ReadWord();
        PilotPulseDuration = reader.ReadWord();
        NumberOfPulsesInPilot = reader.ReadWord();
        ZeroPulseDuration = reader.ReadWord();
        OnePulseDuration = reader.ReadWord();
        BitConfiguration = reader.ReadByte();
        ByteConfiguration = reader.ReadByte();
      //  Data = [..reader.ReadBytes(length-12)];
    }
}