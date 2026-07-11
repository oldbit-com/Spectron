using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Serialization;

namespace OldBit.Spectron.Files.Tzx.Blocks;

/// <summary>
/// Represents the 'Set Signal Level' block.
/// </summary>
public class SetSignalLevelBlock : IBlock
{
    /// <summary>
    /// Gets the block ID.
    /// </summary>
    [FileData(Order = 0)]
    public byte BlockId => BlockCode.SetSignalLevel;

    /// <summary>
    /// Helper property needed by the serialization.
    /// Gets the length of the block without these 4 bytes.
    /// </summary>
    [FileData(Order = 1)]
    private int Length => 1;

    /// <summary>
    /// Gets or sets the signal level.
    /// </summary>
    [FileData(Order = 2)]
    public SignalLevel Level { get; set; }

    /// <summary>
    /// Creates a new instance of the 'Set Signal Level' block.
    /// </summary>
    public SetSignalLevelBlock()
    {
    }

    /// <summary>
    /// Creates a new instance of the 'Set Signal Level' block using the byte reader.
    /// </summary>
    /// <param name="reader">The ByteStreamReader used to initialize the SetSignalLevelBlock properties.</param>
    internal SetSignalLevelBlock(ByteStreamReader reader)
    {
        Level = (SignalLevel)reader.ReadByte();
    }

    /// <summary>
    /// Converts the 'Set Signal Level' block to its equivalent string representation.
    /// </summary>
    /// <returns>A string representation of the 'Set Signal Level' object.</returns>
    public override string ToString() => $"Signal Level: {Level}";
}