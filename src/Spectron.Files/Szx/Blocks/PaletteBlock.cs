using OldBit.Spectron.Files.Extensions;
using OldBit.Spectron.Files.IO;

namespace OldBit.Spectron.Files.Szx.Blocks;

/// <summary>
/// This block ('P','L','T','T') contains the state of the ULA registers found in the 64 colour replacement ULA.
/// </summary>
public sealed class PaletteBlock
{
    /// <summary>
    /// Indicates normal palette mode with BRIGHT and FLASH.
    /// </summary>
    public const byte FlagsPaletteDisabled = 0;

    /// <summary>
    /// Indicates 64 colour palette mode.
    /// </summary>
    public const byte FlagsPaletteEnabled = 1;

    /// <summary>
    /// Gets or sets the flags indicating the palette mode.
    /// </summary>
    public byte Flags { get; set; }

    /// <summary>
    /// Gets or sets the currently selected palette register (0-63).
    /// </summary>
    public byte CurrentRegister { get; set; }

    /// <summary>
    /// Gets or sets the current values of the palette registers.
    /// </summary>
    public byte[] Registers { get; private set; } = new byte [64];

    private static int Size => 66;

    internal void Write(Stream writer)
    {
        var header = new BlockHeader(BlockIds.Palette, Size);
        header.Write(writer);

        writer.WriteByte(Flags);
        writer.WriteByte(CurrentRegister);
        writer.WriteBytes(Registers);
    }

    internal static PaletteBlock Read(ByteStreamReader reader, int size)
    {
        if (size != Size)
        {
            throw new InvalidDataException("Invalid Palette block size.");
        }

        return new PaletteBlock
        {
            Flags = reader.ReadByte(),
            CurrentRegister = reader.ReadByte(),
            Registers = reader.ReadBytes(64)
        };
    }
}