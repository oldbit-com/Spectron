namespace OldBit.Spectron.Emulation.Tape;

/// <summary>
/// Specifies the speed at which the tape should be loaded or saved
/// </summary>
public enum TapeSpeed
{
    /// <summary>
    /// Load or save tape using normal speed.
    /// </summary>
    Normal,

    /// <summary>
    /// Load or save tape instantly. Uses ROM traps to bypass standard loading/saving routines.
    /// </summary>
    Instant,

    /// <summary>
    /// Accelerate tape loading or saving speed. Uses increased emulation speed to makes it faster.
    /// </summary>
    Accelerated,
}