namespace OldBit.Spectron.Files.Z80.Types;

/// <summary>
///     Bit 0: 1 if R register emulation on
///     Bit 1: 1 if LDIR emulation on
///     Bit 2: AY sound in use, even on 48K machines
///     Bit 6: (if bit 2 set) Fuller Audio Box emulation
///     Bit 7: Modify hardware
/// </summary>
public sealed class Flags3
{
    private readonly byte[] _data;

    internal Flags3(byte[] data) => _data = data;

    /// <summary>
    /// Gets or sets a value indicating whether to emulate the behavior of the 'R' register.
    /// </summary>
    public bool EmulateRegisterR
    {
        get => (Value & 0x01) != 0;
        set => Value = (byte)((Value & 0xFE) | (value ? 0x01 : 0));
    }

    /// <summary>
    /// Gets or sets a value indicating whether to emulate the behavior of the 'LDIR' instruction.
    /// </summary>
    public bool EmulateLdirInstruction
    {
        get => (Value & 0x02) != 0;
        set => Value = (byte)((Value & 0xFD) | (value ? 0x02 : 0));
    }

    /// <summary>
    /// Gets or sets a value indicating whether to use AY sound, even on 48K machines.
    /// </summary>
    public bool UseAySound
    {
        get => (Value & 0x04) != 0;
        set => Value = (byte)((Value & 0xFB) | (value ? 0x04 : 0));
    }

    /// <summary>
    /// Gets or sets a value indicating whether to emulate the Fuller Audio Box.
    /// </summary>
    public bool EmulateFullerAudioBox
    {
        get => (Value & 0x40) != 0;
        set => Value = (byte)((Value & 0xBF) | (value ? 0x40 : 0));
    }

    /// <summary>
    /// Gets or sets a modify hardware flag.
    /// </summary>
    public bool ModifyHardware
    {
        get => (Value & 0x80) != 0;
        set => Value = (byte)((Value & 0x7F) | (value ? 0x80 : 0));
    }

    private byte Value
    {
        get => _data[37];
        set => _data[37] = value;
    }
}