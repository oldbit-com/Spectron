using OldBit.Spectron.Files.Extensions;
using OldBit.Spectron.Files.IO;

namespace OldBit.Spectron.Files.Szx.Blocks;

/// <summary>
/// This block ('S','P','C','R') contains the Spectrum's ULA state specifying the
/// current border colour, memory paging status etc.
/// </summary>
public sealed class SpecRegsBlock
{
    /// <summary>
    /// Gets or sets the current border colour. This can be 0 (black) through to 7 (white).
    /// </summary>
    public byte Border { get; set; }

    /// <summary>
    /// Gets or sets the current value of port $7ffd which is used to control memory paging on 128k Spectrum.
    /// For 16k and 48k Spectrum, this should be set to 0.
    /// </summary>
    public byte Port7FFD { get; set; }

    /// <summary>
    /// Gets or sets the current value of port $1ffd which controls additional memory paging features on the Spectrum +2A/+3 and the ZS Scorpion.
    /// Should be set to 0 (zero) for other models.
    /// <remarks>
    /// </remarks>
    /// </summary>
    public byte Port1FFD { get; set; }

    /// <summary>
    /// Gets or sets the last value of written to port $fe. Only bits 3 and 4 (the MIC and EAR bits) are guaranteed to be valid.
    /// </summary>
    public byte PortFE { get; set; }

    /// <summary>
    /// This member is reserved and should be set to 0 (zero).
    /// </summary>
    public byte[] Reserved { get; private set; } = new byte[4];

    private static int Size => 8;

    internal void Write(Stream writer)
    {
        var header = new BlockHeader(BlockIds.SpecRegs, Size);
        header.Write(writer);

        writer.WriteByte(Border);
        writer.WriteByte(Port7FFD);
        writer.WriteByte(Port1FFD);
        writer.WriteByte(PortFE);
        writer.WriteBytes(Reserved);
    }

    internal static SpecRegsBlock Read(ByteStreamReader reader, int size)
    {
        if (size != Size)
        {
            throw new InvalidDataException("Invalid SpecRegs block size.");
        }

        return new SpecRegsBlock
        {
            Border = reader.ReadByte(),
            Port7FFD = reader.ReadByte(),
            Port1FFD = reader.ReadByte(),
            PortFE = reader.ReadByte(),
            Reserved = reader.ReadBytes(4)
        };
    }
}
