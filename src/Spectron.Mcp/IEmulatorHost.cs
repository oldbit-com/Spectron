using OldBit.Spectron.Emulation;

namespace OldBit.Spectron.Mcp;

/// <summary>
/// Implemented by the application that owns the emulator. The emulator instance can be replaced
/// at any time (loading a snapshot, changing the machine type, etc.), so MCP tools always
/// ask the host for the current one. Lifecycle operations are routed through the host,
/// so the UI stays in sync with what the agent does.
/// </summary>
public interface IEmulatorHost
{
    /// <summary>
    /// Gets the currently running emulator or null if there is none.
    /// </summary>
    Emulator? Emulator { get; }

    /// <summary>
    /// Gets a value indicating whether emulation is paused, as shown in the UI.
    /// </summary>
    bool IsPaused { get; }

    /// <summary>
    /// Pauses the emulator. When the returned task completes, the emulator thread is no longer running frames.
    /// </summary>
    Task PauseAsync();

    /// <summary>
    /// Resumes the emulator.
    /// </summary>
    Task ResumeAsync();

    /// <summary>
    /// Resets the emulator. A hard reset recreates the machine using the current preferences.
    /// </summary>
    Task ResetAsync(bool hardReset);

    /// <summary>
    /// Loads a supported file (snapshot, tape, disk, etc.) the same way as File/Open does.
    /// </summary>
    Task LoadFileAsync(string filePath);
}
