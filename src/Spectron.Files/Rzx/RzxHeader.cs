using OldBit.Spectron.Files.Extensions;
using OldBit.Spectron.Files.IO;

namespace OldBit.Spectron.Files.Rzx;

/// <summary>
/// Represents the RZX file header.
/// </summary>
public sealed class RzxHeader
{
    /// <summary>
    /// RZX file signature.
    /// </summary>
    public string Signature { get; private set; } = "RZX!";

    /// <summary>
    /// Major version numbers.
    /// </summary>
    public byte MajorVersion { get; private set; }

    /// <summary>
    /// Minor version numbers.
    /// </summary>
    public byte MinorVersion { get; private set; } = 13;

    /// <summary>
    /// Flags.
    /// </summary>
    public DWord Flags { get; private set; }

    internal static RzxHeader Read(ByteStreamReader reader)
    {
        var header = new RzxHeader();

        var bytes = reader.ReadBytes(4);
        header.Signature = bytes.ToAsciiString();
        header.MajorVersion = reader.ReadByte();
        header.MinorVersion = reader.ReadByte();
        header.Flags = reader.ReadDWord();

        return header;
    }
}