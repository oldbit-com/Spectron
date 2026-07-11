using OldBit.Spectron.Files.Extensions;
using OldBit.Spectron.Files.IO;

namespace OldBit.Spectron.Files.Szx.Blocks;

/// <summary>
/// This block ('K','E','Y','B') contains the state of the Spectrum keyboard and any keyboard joystick emulation.
/// </summary>
public sealed class KeyboardBlock
{
    /// <summary>
    /// Indicates Issue 2 keyboard emulation is enabled.
    /// <remarks>
    /// This is only applicable for the 16k or 48k ZX Spectrum. For other models, set this member to 0 (zero).
    /// </remarks>
    /// </summary>
    public const DWord FlagsIssue2 = 1;

    /// <summary>
    /// Kempston joystick emulation.
    /// </summary>
    public const byte JoystickKempston = 0;

    /// <summary>
    /// Fuller joystick emulation.
    /// </summary>
    public const byte JoystickFuller = 1;

    /// <summary>
    /// Cursor (AGF or Protek) emulation.
    /// </summary>
    public const byte JoystickCursor = 2;

    /// <summary>
    /// Sinclair Interface II port 1 (or Spectrum +2A/+3 joystick 1) emulation.
    /// </summary>
    public const byte JoystickSinclair1 = 3;

    /// <summary>
    /// Sinclair Interface II port 2 (or Spectrum +2A/+3 joystick 2) emulation.
    /// </summary>
    public const byte JoystickSinclair2 = 4;

    /// <summary>
    /// Spectrum+/128/+2/+2A/+3 cursor keys emulation.
    /// </summary>
    public const byte JoystickSpectrumPlus = 5;

    /// <summary>
    /// Timex TC2048, TC2068, TS2068 and Spectrum SE built-in joystick, port 1.
    /// </summary>
    public const byte JoystickTimex1 = 6;

    /// <summary>
    /// Timex TC2048, TC2068, TS2068 and Spectrum SE built-in joystick, port 2.
    /// </summary>
    public const byte JoystickTimes2 = 7;

    /// <summary>
    /// No joystick emulation.
    /// </summary>
    public const byte JoystickNone = 8;

    /// <summary>
    /// Gets or sets the various flags.
    /// </summary>
    public DWord Flags { get; set; }

    /// <summary>
    /// Gets or sets which joystick the PC keyboard should emulate (the actual keys are emulator dependant).
    /// </summary>
    public byte Joystick { get; set; }

    internal void Write(Stream writer)
    {
        var header = new BlockHeader(BlockIds.Keyboard, 5);
        header.Write(writer);

        writer.WriteDWord(Flags);
        writer.WriteByte(Joystick);
    }

    internal static KeyboardBlock Read(ByteStreamReader reader, int size)
    {
        if (size != 5)
        {
            throw new InvalidDataException("Invalid Keyboard block size.");
        }

        return new KeyboardBlock
        {
            Flags = reader.ReadDWord(),
            Joystick = reader.ReadByte()
        };
    }
}