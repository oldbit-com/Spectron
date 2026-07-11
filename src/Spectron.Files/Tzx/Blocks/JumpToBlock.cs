using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Serialization;

namespace OldBit.Spectron.Files.Tzx.Blocks;

/// <summary>
/// Represents the 'Jump To' block.
/// </summary>
public class JumpToBlock : IBlock
{
    /// <summary>
    /// Gets the block ID.
    /// </summary>
    [FileData(Order = 0)]
    public byte BlockId => BlockCode.JumpToBlock;

    /// <summary>
    /// Gets or sets the relative jump value.
    /// </summary>
    [FileData(Order = 1)]
    public short Jump { get; set; }

    /// <summary>
    /// Creates a new instance of the 'Jump To' block.
    /// </summary>
    public JumpToBlock()
    {
    }

    /// <summary>
    /// Creates a new instance of the 'Jump To' block using the byte reader.
    /// </summary>
    /// <param name="reader">The ByteStreamReader used to initialize the JumpToBlock properties.</param>
    internal JumpToBlock(ByteStreamReader reader)
    {
        Jump = (short)reader.ReadWord();
    }

    /// <summary>
    /// Converts the 'Jump To' block to its equivalent string representation.
    /// </summary>
    /// <returns>A string representation of the 'Jump To' object which corresponds to Name value.</returns>
    public override string ToString() => $"Jump to {Jump}";
}