using OldBit.Spectron.Files.Extensions;
using OldBit.Spectron.Files.IO;

namespace OldBit.Spectron.Files.Szx.Blocks;

/// <summary>
/// This block ('A','Y',0,0) contains the state of the AY sound chip.
/// </summary>
public sealed class AyBlock
{
    /// <summary>
    /// Indicates Fuller Box emulation.
    /// </summary>
    public const byte FlagsFullerBox = 1;

    /// <summary>
    /// Indicates Melodik Soundbox emulation.
    /// </summary>
    public const byte Flags128Ay = 2;

    /// <summary>
    /// Gets or sets a set of flags that indicate an AY chip is available to the 16k/48k ZX Spectrum and Timex TC2048.
    /// <remarks>
    /// This member should be set to 0 (zero) for other machines which have a built-in AY chip.
    /// </remarks>
    /// </summary>
    public byte Flags { get; set; }

    /// <summary>
    /// Gets or sets the currently selected AY register (0-15).
    /// </summary>
    public byte CurrentRegister { get; set; }

    /// <summary>
    /// Gets or sets the values of the AY registers.
    /// </summary>
    public byte[] Registers { get; private set; } = new byte[16];

    private static int Size => 18;

    internal void Write(Stream writer)
    {
        var header = new BlockHeader(BlockIds.Ay, Size);
        header.Write(writer);

        writer.WriteByte(Flags);
        writer.WriteByte(CurrentRegister);
        writer.WriteBytes(Registers);
    }

    internal static AyBlock Read(ByteStreamReader reader, int size)
    {
        if (size != Size)
        {
            throw new InvalidDataException("Invalid AY block size.");
        }

        return new AyBlock
        {
            Flags = reader.ReadByte(),
            CurrentRegister = reader.ReadByte(),
            Registers = reader.ReadBytes(16)
        };
    }
}