using OldBit.Spectron.Files.IO;

namespace OldBit.Spectron.Files.Szx.Blocks;

/// <summary>
/// This block ('S','C','L','D') contains the current screen mode and memory paging status of Timex Sinclair machines.
/// </summary>
public sealed class TimexSinclairBlock
{
    /// <summary>
    /// Gets or sets the current value of port 0xF4 which controls memory paging.
    /// </summary>
    public byte PortF4 { get; set; }

    /// <summary>
    /// Gets or sets the current value of port 0xFF which controls the Timex screen mode,
    /// interrupt state, high resolution colours, and the remainder of the memory paging system.
    /// </summary>
    public byte PortFF { get; set; }

    private static int Size => 2;

    internal void Write(Stream writer)
    {
        var header = new BlockHeader(BlockIds.TimexSinclair, Size);
        header.Write(writer);

        writer.WriteByte(PortF4);
        writer.WriteByte(PortFF);
    }

    internal static TimexSinclairBlock Read(ByteStreamReader reader, int size)
    {
        if (size != Size)
        {
            throw new InvalidDataException("Invalid TimexSinclair block size.");
        }

        return new TimexSinclairBlock
        {
            PortF4 = reader.ReadByte(),
            PortFF = reader.ReadByte()
        };
    }
}