using ModelContextProtocol;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices.Keyboard;

namespace OldBit.Spectron.Mcp;

/// <summary>
/// Why a frame-driving operation stopped before running all requested frames.
/// </summary>
internal enum StopReason
{
    None,
    Breakpoint,
    ResumedByUser,
    EmulatorReplaced
}

internal readonly record struct RunResult(int Frames, StopReason StopReason);

/// <summary>
/// Serializes access to the emulator for MCP tools. Every operation runs while the emulator
/// is paused, so tools see a consistent state and can drive frames themselves.
/// The user can still interact with the UI at any time, so frame-driving operations stop
/// as soon as the user resumes emulation or replaces the machine (e.g. loads a snapshot).
/// </summary>
public sealed class EmulatorController(IEmulatorHost host)
{
    internal const int MaxFramesPerCall = 50 * 60 * 5;

    private const int MaxPauseAttempts = 3;

    // The 128 editor takes about 30 frames to redraw after ENTER, more on a full screen
    private const int KeyReadTimeoutFrames = 100;

    private const Word Flags = 23611;
    private const byte NewKeyFlag = 0x20;   // FLAGS bit 5: a new key is waiting in LAST_K

    private readonly SemaphoreSlim _lock = new(1, 1);

    /// <summary>
    /// Runs an action while the emulator is paused, then restores the previous run state.
    /// </summary>
    internal Task<T> ExecuteAsync<T>(Func<Emulator, T> action) =>
        ExecuteAsync((emulator, _) => action(emulator), leavePaused: false);

    /// <summary>
    /// Runs an action while the emulator is paused.
    /// </summary>
    /// <param name="action">The action to execute, receives the emulator and whether it was paused before the call.</param>
    /// <param name="leavePaused">When true, the emulator stays paused after the action, otherwise the previous state is restored.</param>
    internal async Task<T> ExecuteAsync<T>(Func<Emulator, bool, T> action, bool leavePaused)
    {
        await _lock.WaitAsync();

        try
        {
            var (emulator, wasPaused) = await PauseEmulatorAsync();

            try
            {
                return action(emulator, wasPaused);
            }
            finally
            {
                // Don't resume when a breakpoint was hit (the debugger takes over), or when the machine
                // was replaced meanwhile (resuming would affect the new machine, not the one we paused).
                if (!wasPaused && !leavePaused && !emulator.IsDebuggerBreak && IsCurrent(emulator))
                {
                    await host.ResumeAsync();
                }
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    internal async Task RunHostActionAsync(Func<IEmulatorHost, Task> action)
    {
        await _lock.WaitAsync();

        try
        {
            await action(host);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Runs the given number of frames as fast as possible. Must be called while the emulator is paused.
    /// Stops early if a debugger breakpoint is hit, the user resumes emulation or the machine is replaced.
    /// </summary>
    internal RunResult RunFrames(Emulator emulator, int count)
    {
        for (var i = 0; i < count; i++)
        {
            var stopReason = GetStopReason(emulator);

            if (stopReason != StopReason.None)
            {
                return new RunResult(i, stopReason);
            }

            emulator.RunFrame();
        }

        return new RunResult(count, GetStopReason(emulator) == StopReason.Breakpoint ? StopReason.Breakpoint : StopReason.None);
    }

    /// <summary>
    /// Presses each key combination in turn, holding it for <paramref name="holdFrames"/> frames
    /// and releasing for <paramref name="releaseFrames"/> frames, so the ROM keyboard routine registers every press.
    /// </summary>
    /// <param name="emulator">The emulator.</param>
    /// <param name="combinations">Key combinations to press in order.</param>
    /// <param name="holdFrames">Frames to hold each combination.</param>
    /// <param name="releaseFrames">Frames to wait after releasing each combination.</param>
    /// <param name="waitUntilRead">
    /// When true, before the next press waits until the program has read the previous key from LAST_K.
    /// The ROM keyboard routine keeps only one key, so a key pressed while the program is busy (e.g. the 128 editor
    /// redrawing after ENTER) would replace the one still waiting. Waiting stops for the rest of the call
    /// when a key is not read in time, e.g. a game that reads the keyboard ports directly.
    /// </param>
    internal RunResult PressKeys(Emulator emulator, IEnumerable<SpectrumKey[]> combinations, int holdFrames, int releaseFrames,
        bool waitUntilRead = false)
    {
        var frames = 0;

        foreach (var keys in combinations)
        {
            if (waitUntilRead)
            {
                var (waitResult, isRead) = WaitUntilKeyRead(emulator);
                frames += waitResult.Frames;

                if (waitResult.StopReason != StopReason.None)
                {
                    return waitResult with { Frames = frames };
                }

                waitUntilRead = isRead;
            }

            emulator.KeyboardState.KeyDown(keys);

            RunResult result;

            try
            {
                result = RunFrames(emulator, holdFrames);
            }
            finally
            {
                emulator.KeyboardState.KeyUp(keys);
            }

            frames += result.Frames;

            if (result.StopReason == StopReason.None)
            {
                result = RunFrames(emulator, releaseFrames);
                frames += result.Frames;
            }

            if (result.StopReason != StopReason.None)
            {
                return result with { Frames = frames };
            }
        }

        if (waitUntilRead)
        {
            // Let the last key be read too, so the next call or tool sees its effect
            var (waitResult, _) = WaitUntilKeyRead(emulator);

            return waitResult with { Frames = frames + waitResult.Frames };
        }

        return new RunResult(frames, StopReason.None);
    }

    /// <summary>
    /// Runs frames while FLAGS bit 5 says a key is waiting in LAST_K, up to <see cref="KeyReadTimeoutFrames"/>.
    /// </summary>
    private (RunResult Result, bool IsRead) WaitUntilKeyRead(Emulator emulator)
    {
        for (var i = 0; i < KeyReadTimeoutFrames; i++)
        {
            if ((emulator.Memory.Read(Flags) & NewKeyFlag) == 0)
            {
                return (new RunResult(i, StopReason.None), true);
            }

            var result = RunFrames(emulator, 1);

            if (result.StopReason != StopReason.None)
            {
                return (result with { Frames = i + result.Frames }, false);
            }
        }

        return (new RunResult(KeyReadTimeoutFrames, StopReason.None), false);
    }

    private async Task<(Emulator Emulator, bool WasPaused)> PauseEmulatorAsync()
    {
        for (var attempt = 0; attempt < MaxPauseAttempts; attempt++)
        {
            var emulator = host.Emulator ?? throw new McpException("Emulator is not running.");
            var wasPaused = host.IsPaused;

            if (!wasPaused)
            {
                await host.PauseAsync();
            }

            // The machine may have been replaced by the user while pausing, in which case try again
            if (IsCurrent(emulator))
            {
                return (emulator, wasPaused);
            }
        }

        throw new McpException("The emulator is being replaced (a file is loading or the machine is resetting). Try again.");
    }

    private StopReason GetStopReason(Emulator emulator)
    {
        if (emulator.IsDebuggerBreak)
        {
            return StopReason.Breakpoint;
        }

        if (!IsCurrent(emulator))
        {
            return StopReason.EmulatorReplaced;
        }

        return host.IsPaused ? StopReason.None : StopReason.ResumedByUser;
    }

    private bool IsCurrent(Emulator emulator) => ReferenceEquals(host.Emulator, emulator);
}
