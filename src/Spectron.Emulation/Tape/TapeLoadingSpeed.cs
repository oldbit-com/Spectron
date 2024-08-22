namespace OldBit.Spectron.Emulation.Tape;

/// <summary>
/// Specifies the speed at which the tape should be loaded.
/// </summary>
public enum TapeLoadingSpeed
{
    /// <summary>
    /// Load tape using normal loading speed.
    /// </summary>
    Normal,

    /// <summary>
    /// Load tape instantly. Uses ROM traps to bypass standard loading routines.
    /// </summary>
    Instant,

    /// <summary>
    /// Accelerate tape loading speed. Uses increased CPU speed to load the tape faster.
    /// </summary>
    Accelerated,
}