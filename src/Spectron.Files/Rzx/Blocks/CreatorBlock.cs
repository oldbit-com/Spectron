using OldBit.Spectron.Files.Extensions;
using OldBit.Spectron.Files.IO;

namespace OldBit.Spectron.Files.Rzx.Blocks;

/// <summary>
/// Information about the program which created the RZX.
/// </summary>
public class CreatorBlock
{
    /// <summary>
    /// Creator's identification string.
    /// </summary>≠≠
    public string CreatorName { get; private set; } = string.Empty;

    /// <summary>
    /// Creator's major version number.
    /// </summary>
    public Word MajorVersion { get; private set; }

    /// <summary>
    /// Creator's minor version number.
    /// </summary>
    public Word MinorVersion { get; private set; }

    /// <summary>
    /// Creator's custom data (may be absent).
    /// </summary>
    public byte[]? Data { get; private set; }

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
}