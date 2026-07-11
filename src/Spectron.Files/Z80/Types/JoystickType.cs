namespace OldBit.Spectron.Files.Z80.Types;

/// <summary>
/// Defines the types of joysticks supported.
/// </summary>
public enum JoystickType
{
    /// <summary>
    /// Cursor / Protek / AGF joystick.
    /// </summary>
    Cursor = 0,

    /// <summary>
    /// Kempston joystick.
    /// </summary>
    Kempston = 1,

    /// <summary>
    /// Sinclair 2 left joystick (or user defined, for version 3 .z80 files).
    /// </summary>
    Sinclair1 = 2,

    /// <summary>
    /// Sinclair 2 right joystick.
    /// </summary>
    Sinclair2 = 3
}