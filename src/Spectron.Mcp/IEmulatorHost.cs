using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Files;

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
    /// Loads a supported file (snapshot, tape, disk, etc.) the same way as File/Open does, but without
    /// showing any dialogs. Errors are thrown to the caller.
    /// </summary>
    /// <param name="filePath">The path to the file.</param>
    /// <param name="archiveEntry">For a zip archive, the name of the entry to load. It can be null when the archive
    /// contains exactly one supported file.</param>
    /// <returns>The type of the loaded file. For an archive, the type of the loaded entry.</returns>
    Task<FileType> LoadFileAsync(string filePath, string? archiveEntry);
}
