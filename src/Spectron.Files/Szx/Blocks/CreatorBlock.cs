using OldBit.Spectron.Files.Extensions;
using OldBit.Spectron.Files.IO;

namespace OldBit.Spectron.Files.Szx.Blocks;

/// <summary>
/// This block ('C','R','T','R') identifies the program that created this zx-state file.
/// </summary>
public sealed class CreatorBlock
{
    /// <summary>
    /// Gets or sets the name of the program that created this zx-state file.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the creator program's major version number.
    /// </summary>
    public Word MajorVersion { get; set; }

    /// <summary>
    /// Gets or sets the creator program's minor version number
    /// </summary>
    public Word MinorVersion { get; set; }

    /// <summary>
    /// Gets or sets the data specific to the creator program.
    /// </summary>
    public byte[]? CustomData { get; set; }

    internal void Write(Stream writer)
    {
        var header = new BlockHeader(BlockIds.Creator, 32 + 2 + 2 + (CustomData?.Length ?? 1));
        header.Write(writer);

        writer.WriteChars(Name, 32);
        writer.WriteWord(MajorVersion);
        writer.WriteWord(MinorVersion);
        writer.WriteBytes(CustomData ?? [0]);
    }

    internal static CreatorBlock Read(ByteStreamReader reader, int size)
    {
        var creator = new CreatorBlock
        {
            Name = reader.ReadChars(32).TrimEnd('\0'),
            MajorVersion = reader.ReadWord(),
            MinorVersion = reader.ReadWord()
        };

        var remaining = size - 36;
        var customData = reader.ReadBytes(remaining);
        creator.CustomData = customData is [0] ? null : customData;

        return creator;
    }
}