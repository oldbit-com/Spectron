namespace OldBit.Spectron.Files.Z80.Types;

/// <summary>
///     Bit 0-1: Interrupt mode (0, 1 or 2)
///     Bit 2  : 1=Issue 2 emulation
///     Bit 3  : 1=Double interrupt frequency
///     Bit 4-5: 1=High video synchronisation
///              3=Low video synchronisation
///              0,2=Normal
///     Bit 6-7: 0=Cursor / Protek / AGF joystick
///              1=Kempston joystick
///              2=Sinclair 2 Left joystick (or user defined, for version 3 .z80 files)
///              3=Sinclair 2 Right joystick
/// </summary>
public sealed class Flags2
{
    private readonly byte[] _data;

    internal Flags2(byte[] data) => _data = data;

    /// <summary>
    /// Gets or sets the interrupt mode.
    /// </summary>
    public byte InterruptMode
    {
        get => (byte)(Value & 0x03);
        set => Value = (byte)((Value & 0xFC) | (value & 0x03));
    }

    /// <summary>
    /// Gets or sets a value indicating whether to emulate Issue 2 behavior.
    /// </summary>
    public bool Issue2Emulation
    {
        get => (Value & 0x04) != 0;
        set => Value = (byte)((Value & 0xFB) | (value ? 0x04 : 0));
    }

    /// <summary>
    /// Gets or sets a value indicating whether to double the interrupt frequency.
    /// </summary>
    public bool DoubleInterruptFrequency
    {
        get => (Value & 0x08) != 0;
        set => Value = (byte)((Value & 0xF7) | (value ? 0x08 : 0));
    }

    /// <summary>
    /// Gets or sets the video synchronization mode.
    /// </summary>
    public byte VideoSynchronization
    {
        get => (byte)((Value >> 4) & 0x03);
        set => Value = (byte)((Value & 0xCF) | ((value & 0x03) << 4));
    }

    /// <summary>
    /// Gets or sets the joystick type.
    /// </summary>
    public JoystickType JoystickType
    {
       get => (JoystickType)((Value >> 6) & 0x03);
       set => Value = (byte)((Value & 0x3F) | ((byte)value << 6));
    }

    private byte Value
    {
        get => _data[29];
        set => _data[29] = value;
    }
}