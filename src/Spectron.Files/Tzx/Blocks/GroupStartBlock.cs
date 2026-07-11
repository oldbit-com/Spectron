using OldBit.Spectron.Files.Extensions;
using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Serialization;

namespace OldBit.Spectron.Files.Tzx.Blocks;

/// <summary>
/// Represents the 'Group Start' block.
/// </summary>
public class GroupStartBlock : IBlock
{
    /// <summary>
    /// Gets the block ID.
    /// </summary>
    [FileData(Order = 0)]
    public byte BlockId => BlockCode.GroupStart;

    /// <summary>
    /// Helper property needed by the serialization.
    /// Gets the length of the group name string.
    /// </summary>
    [FileData(Order = 1)]
    private byte Length => (byte)Name.Length;

    /// <summary>
    /// Gets or sets the name in ASCII format.
    /// </summary>
    [FileData(Order = 2)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Creates a new instance of the 'Group Start' block.
    /// </summary>
    public GroupStartBlock()
    {
    }

    /// <summary>
    /// Creates a new instance of the 'Group Start' block using the byte reader.
    /// </summary>
    /// <param name="reader">The ByteStreamReader used to initialize the GroupStartBlock properties.</param>
    internal GroupStartBlock(ByteStreamReader reader)
    {
        var length = reader.ReadByte();
        Name = reader.ReadBytes(length).ToAsciiString();
    }

    /// <summary>
    /// Converts the 'Group Start' block to its equivalent string representation.
    /// </summary>
    /// <returns>A string representation of the 'Group Start' object which corresponds to Name value.</returns>
    public override string ToString() => Name;
}