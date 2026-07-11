using OldBit.Spectron.Files.Extensions;
using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Serialization;
using OldBit.Spectron.Files.Tzx.Extensions;

namespace OldBit.Spectron.Files.Tzx.Blocks;

/// <summary>
/// Represents the 'Archive Info' block.
/// </summary>
public class ArchiveInfoBlock : IBlock
{
    /// <summary>
    /// Gets the block ID.
    /// </summary>
    [FileData(Order = 0)]
    public byte BlockId => BlockCode.ArchiveInfo;

    /// <summary>
    /// Helper property needed by the serialization.
    /// Gets the Length of the whole block (without these two bytes).
    /// </summary>
    [FileData(Order = 1)]
    private byte Count => (byte)Infos.Count;

    /// <summary>
    /// Gets or sets a list of <see cref="TextInfo"/> items.
    /// </summary>
    [FileData(Order = 2)]
    public List<TextInfo> Infos { get; set; } = [];

    /// <summary>
    /// Creates a new instance of the 'Archive Info' block.
    /// </summary>
    public ArchiveInfoBlock()
    {
    }

    /// <summary>
    /// Creates a new instance of the 'Archive Info' block using the byte reader.
    /// </summary>
    /// <param name="reader">The ByteStreamReader used to initialize the ArchiveInfoBlock properties.</param>
    internal ArchiveInfoBlock(ByteStreamReader reader)
    {
        reader.ReadWord();
        var count = reader.ReadByte();
        for (var i = 0; i < count; i++)
        {
            Infos.Add(new TextInfo(reader));
        }
    }

    /// <summary>
    /// Convenience method that adds new info to the list of <see cref="Infos"/>.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="text"></param>
    public void AddTextInfo(byte id, string text)
    {
        Infos.Add(new TextInfo { Id = id, Text = text});
    }

    /// <summary>
    /// Helper property needed by the serialization.
    /// Gets the Length of the whole block (without these two bytes).
    /// </summary>
    [FileData(Order = 0)]
    private Word Length => (Word)(1 + Count * 2 + Infos.Sum(x => x.Length));

    /// <summary>
    /// Converts the 'Archive Info' block to its equivalent string representation.
    /// </summary>
    /// <returns>The string representation of this object.</returns>
    public override string ToString()
    {
        var title = this.GetInfoText(ArchiveInfo.Title);
        var year = this.GetInfoText(ArchiveInfo.Year);

        return year != null ? $"{title} ({year})" : title ?? string.Empty;
    }

    /// <summary>
    /// Represents the TEXT structure.
    /// </summary>
    public class TextInfo
    {
        /// <summary>
        /// Gets or sets the text identification.
        /// </summary>
        [FileData(Order = 0)]
        public byte Id { get; set; }

        /// <summary>
        /// Helper property needed by the serialization.
        /// Gets the length of the text string.
        /// </summary>
        [FileData(Order = 1)]
        internal byte Length => (byte)Text.Length;

        /// <summary>
        /// Gets or sets the text string in ASCII format.
        /// </summary>
        [FileData(Order = 2)]
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Creates a new instance of the 'TEXT' structure.
        /// </summary>
        public TextInfo()
        {
        }

        /// <summary>
        /// Creates a new instance of the 'TEXT' structure using the byte reader.
        /// </summary>
        /// <param name="reader">The ByteStreamReader used to initialize the TextInfo properties.</param>
        internal TextInfo(ByteStreamReader reader)
        {
            Id = reader.ReadByte();
            var length = reader.ReadByte();
            var bytes = reader.ReadBytes(length);
            Text = bytes.ToAsciiString();
        }

        /// <summary>
        /// Converts the TextInfo class to its equivalent string representation.
        /// </summary>
        /// <returns>A string representation of TextInfo object.</returns>
        public override string ToString() => Text;
    }
}