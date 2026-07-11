using System.Text;
using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Serialization;

namespace OldBit.Spectron.Files.Tzx.Blocks;

/// <summary>
/// Represents the 'Select' block.
/// </summary>
public class SelectBlock : IBlock
{
    /// <summary>
    /// Gets the block ID.
    /// </summary>
    [FileData(Order = 0)]
    public byte BlockId => BlockCode.Select;

    /// <summary>
    /// Helper property needed by the serialization.
    /// Gets the length of the whole block.
    /// </summary>
    [FileData(Order = 1)]
    private Word Length => (Word)(1 + Selections.Count * 3 + Selections.Sum(x => x.Length));

    /// <summary>
    /// Helper property needed by the serialization.
    /// Gets the number of selections.
    /// </summary>
    [FileData(Order = 2)]
    private byte Count => (byte)Selections.Count;

    /// <summary>
    /// Gets or sets the array of selections.
    /// </summary>
    [FileData(Order = 3)]
    public List<Selection> Selections { get; set; } = [];

    /// <summary>
    /// Creates a new instance of the 'Select' block.
    /// </summary>
    public SelectBlock()
    {
    }

    /// <summary>
    /// Creates a new instance of the 'Select' block using the byte reader.
    /// </summary>
    /// <param name="reader">The ByteStreamReader used to initialize the SelectBlock properties.</param>
    internal SelectBlock(ByteStreamReader reader)
    {
        reader.ReadWord();
        var count = reader.ReadByte();
        for (var i = 0; i < count; i++)
        {
            Selections.Add(new Selection(reader));
        }
    }

    /// <summary>
    /// Converts the 'Select' block to its equivalent string representation.
    /// </summary>
    /// <returns>A string representation of the 'Select' object.</returns>
    public override string ToString() => $"{Selections.Count} selection(s)";

    /// <summary>
    /// Represents the SELECT structure.
    /// </summary>
    public class Selection
    {
        /// <summary>
        /// Gets or sets the relative offset.
        /// </summary>
        [FileData(Order = 0)]
        public short Offset { get; set; }

        /// <summary>
        /// Helper property needed by the serialization
        /// Gets the length of the description text.
        /// </summary>
        [FileData(Order = 1)]
        internal byte Length => (byte)Description.Length;

        /// <summary>
        /// Gets or sets the description text.
        /// </summary>
        [FileData(Order = 2)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Creates a new instance of the 'SELECT' structure.
        /// </summary>
        public Selection()
        {
        }

        /// <summary>
        /// Creates a new instance of the 'SELECT' structure using the byte reader..
        /// </summary>
        /// <param name="reader">The ByteStreamReader used to initialize the Selection properties.</param>
        internal Selection(ByteStreamReader reader)
        {
            Offset = (short)reader.ReadWord();
            var length = reader.ReadByte();
            Description = Encoding.ASCII.GetString(reader.ReadBytes(length));
        }
    }
}