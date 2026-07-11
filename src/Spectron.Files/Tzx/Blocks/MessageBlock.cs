using OldBit.Spectron.Files.Extensions;
using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Serialization;

namespace OldBit.Spectron.Files.Tzx.Blocks;

/// <summary>
/// Represents the 'Message' block.
/// </summary>
public class MessageBlock : IBlock
{
    /// <summary>
    /// Gets the block ID.
    /// </summary>
    [FileData(Order = 0)]
    public byte BlockId => BlockCode.Message;

    /// <summary>
    /// Gets or sets the time (in seconds) for which the message should be displayed.
    /// </summary>
    [FileData(Order = 1)]
    public byte Time { get; set; }

    /// <summary>
    /// Helper property needed by the serialization.
    /// Gets the length of the text message.
    /// </summary>
    [FileData(Order = 2)]
    private byte Length => (byte)Message.Length;

    /// <summary>
    /// Gets or sets the message that should be displayed in ASCII format.
    /// </summary>
    [FileData(Order = 3)]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Creates a new instance of the 'Message' block.
    /// </summary>
    public MessageBlock()
    {
    }

    /// <summary>
    /// Creates a new instance of the 'Message' block using the byte reader.
    /// </summary>
    /// <param name="reader">The ByteStreamReader used to initialize the MessageBlock properties.</param>
    internal MessageBlock(ByteStreamReader reader)
    {
        Time = reader.ReadByte();
        var length = reader.ReadByte();
        Message = reader.ReadBytes(length).ToAsciiString();
    }

    /// <summary>
    /// Converts the 'Message' block to its equivalent string representation.
    /// </summary>
    /// <returns>A string representation of the 'Message' object which corresponds to Message value.</returns>
    public override string ToString() => Message;
}