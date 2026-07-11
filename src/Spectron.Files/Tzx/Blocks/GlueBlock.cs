using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Serialization;

namespace OldBit.Spectron.Files.Tzx.Blocks;

/// <summary>
/// Represents the 'Glue' block.
/// </summary>
public class GlueBlock : IBlock
{
    /// <summary>
    /// Gets the block ID.
    /// </summary>
    [FileData(Order = 0)]
    public byte BlockId => BlockCode.Glue;

    /// <summary>
    /// Gets or sets the data object.
    /// </summary>
    [FileData(Order = 1, Size = 9)]
    public List<byte> Data { get; set; }

    /// <summary>
    /// Creates a new instance of the 'Glue' block.
    /// </summary>
    public GlueBlock()
    {
        Data = new List<byte> { 0x58, 0x54, 0x61, 0x70, 0x65, 0x21, 0x1A, HeaderBlock.TzxVerMajor, HeaderBlock.TzxVerMinor };
    }

    /// <summary>
    /// Creates a new instance of the 'Glue' block using the byte reader.
    /// </summary>
    /// <param name="reader">The ByteStreamReader used to initialize the GlueBlock properties.</param>
    internal GlueBlock(ByteStreamReader reader)
    {
        Data = new List<byte>(reader.ReadBytes(9));
    }

    /// <summary>
    /// Converts the 'Glue' block to its equivalent string representation.
    /// </summary>
    /// <returns>A string representation of the 'Glue' object.</returns>
    public override string ToString() => "Glue";
}