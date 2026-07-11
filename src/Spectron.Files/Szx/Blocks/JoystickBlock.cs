using OldBit.Spectron.Files.Extensions;
using OldBit.Spectron.Files.IO;

namespace OldBit.Spectron.Files.Szx.Blocks;

/// <summary>
/// This block ('J','O','Y',0) contains setup for both players.
/// </summary>
public sealed class JoystickBlock
{
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
    /// Comcom programmable joystick interface.
    /// </summary>
    public const byte JoystickComCon = 5;

    /// <summary>
    /// Timex TC2048, TC2068, TS2068 and Spectrum SE built-in joystick, port 1.
    /// </summary>
    public const byte JoystickTimex1 = 6;

    /// <summary>
    /// Timex TC2048, TC2068, TS2068 and Spectrum SE built-in joystick, port 2.
    /// </summary>
    public const byte JoystickTimes2 = 7;

    /// <summary>
    /// Joystick disabled.
    /// </summary>
    public const byte JoystickDisabled = 8;

    /// <summary>
    /// This flag is now deprecated as it is an emulator feature rather than hardware state information.
    /// </summary>
    public DWord Flags { get; set; }

    /// <summary>
    /// Specifies which joystick to emulate for Player 1.
    /// </summary>
    public byte JoystickTypePlayer1 { get; set; }

    /// <summary>
    /// Specifies which joystick to emulate for Player 2.
    /// </summary>
    public byte JoystickTypePlayer2 { get; set; }

    private static int Size => 6;

    internal void Write(Stream writer)
    {
        var header = new BlockHeader(BlockIds.Joystick, Size);
        header.Write(writer);

        writer.WriteDWord(Flags);
        writer.WriteByte(JoystickTypePlayer1);
        writer.WriteByte(JoystickTypePlayer2);
    }

    internal static JoystickBlock Read(ByteStreamReader reader, int size)
    {
        if (size != Size)
        {
            throw new InvalidDataException("Invalid Joystick block size.");
        }

        return new JoystickBlock
        {
            Flags = reader.ReadDWord(),
            JoystickTypePlayer1 = reader.ReadByte(),
            JoystickTypePlayer2 = reader.ReadByte()
        };
    }
}