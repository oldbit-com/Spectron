namespace OldBit.Spectron.Files.Z80.Types;

/// <summary>
///     Bit 0  : Bit 7 of the R-register
///     Bit 1-3: Border colour
///     Bit 4  : 1=Basic SamRom switched in
///     Bit 5  : 1=Block of data is compressed
///     Bit 6-7: No meaning
/// </summary>
public class Flags1
{
    private readonly byte[] _data;

    internal Flags1(byte[] data) => _data = data;

    /// <summary>
    /// Gets or sets the value of bit 7 of the R-register.
    /// </summary>
    public byte Bit7R
    {
        get => (byte)(Value& 0x01);
        set => Value = (byte)((Value & 0xFE) | (value & 0x01));
    }

    /// <summary>
    /// Gets or sets the border color.
    /// </summary>
    public byte BorderColor
    {
        get => (byte)(Value>> 1 & 0x07);
        set => Value = (byte)((Value & 0xF1) | ((value & 0x07) << 1));
    }

    /// <summary>
    /// Gets or sets a value indicating whether the Basic SamRom is switched in.
    /// </summary>
    public bool IsSamRam
    {
        get => (Value & 0x10) != 0;
        set => Value = (byte)((Value & 0xEF) | (value ? 0x10 : 0));
    }

    /// <summary>
    /// Gets or sets a value indicating whether the block of data is compressed.
    /// </summary>
    public bool IsDataCompressed
    {
        get => (Value & 0x20) != 0;
        set => Value = (byte)((Value & 0xDF) | (value ? 0x20 : 0));
    }

    private byte Value
    {
        get => _data[12];
        set => _data[12] = value;
    }
}