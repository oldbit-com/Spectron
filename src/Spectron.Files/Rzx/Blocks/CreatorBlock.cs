using OldBit.Spectron.Files.Extensions;
using OldBit.Spectron.Files.IO;

namespace OldBit.Spectron.Files.Rzx.Blocks;

/// <summary>
/// Information about the program which created the RZX.
/// </summary>
public sealed class CreatorBlock
{
    /// <summary>
    /// Creator's identification string.
    /// </summary>
    public string CreatorName { get; set; } = string.Empty;

    /// <summary>
    /// Creator's major version number.
    /// </summary>
    public Word MajorVersion { get; set; }

    /// <summary>
    /// Creator's minor version number.
    /// </summary>
    public Word MinorVersion { get; set; }

    /// <summary>
    /// Creator's custom data (optional).
    /// </summary>
    public byte[] Data { get; set; } = [];

    internal static CreatorBlock Read(ByteStreamReader reader, DWord blockLength)
    {
        var block = new CreatorBlock();

        var bytes = reader.ReadBytes(20);
        block.CreatorName = bytes.ToAsciiString().Trim();
        block.MajorVersion = reader.ReadWord();
        block.MinorVersion = reader.ReadWord();
        block.Data = reader.ReadBytes((int)(blockLength - 29));

        return block;
    }

    internal void Write(Stream writer)
    {
        writer.WriteByte(BlockIds.Creator);
        writer.WriteDWord(29 + Data.Length);
        writer.WriteChars(CreatorName.PadRight(20, ' '), 20);
        writer.WriteWord(MajorVersion);
        writer.WriteWord(MinorVersion);
        writer.WriteBytes(Data);
    }
}