using OldBit.Spectron.Files.Extensions;
using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Serialization;

namespace OldBit.Spectron.Files.Tzx.Blocks;

/// <summary>
/// Represents the 'Text Description' block.
/// </summary>
public class TextDescriptionBlock : IBlock
{
    /// <summary>
    /// Gets the block ID.
    /// </summary>
    [FileData(Order = 0)]
    public byte BlockId => BlockCode.TextDescription;

    /// <summary>
    /// Helper property needed by the serialization.
    /// Gets the length of the text description.
    /// </summary>
    [FileData(Order = 1)]
    private byte Length => (byte)Description.Length;

    /// <summary>
    /// Gets or sets the description in ASCII format.
    /// </summary>
    [FileData(Order = 2)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Creates a new instance of the 'Text Description' block.
    /// </summary>
    public TextDescriptionBlock()
    {
    }

    /// <summary>
    /// Creates a new instance of the 'Text Description' block using the byte reader.
    /// </summary>
    /// <param name="reader">The ByteStreamReader used to initialize the TextDescriptionBlock properties.</param>
    internal TextDescriptionBlock(ByteStreamReader reader)
    {
        var length = reader.ReadByte();
        Description = reader.ReadBytes(length).ToAsciiString();
    }

    /// <summary>
    /// Converts the 'Text Description' block to its equivalent string representation.
    /// </summary>
    /// <returns>A string representation of the 'Text Description' object which corresponds to Description value.</returns>
    public override string ToString() => Description;
}