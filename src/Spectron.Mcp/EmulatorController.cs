using ModelContextProtocol;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices.Keyboard;

namespace OldBit.Spectron.Mcp;

/// <summary>
/// Serializes access to the emulator for MCP tools. Every operation runs while the emulator
/// thread is paused, so tools see a consistent state and can drive frames themselves.
/// </summary>
public sealed class EmulatorController(IEmulatorHost host)
{
    internal const int MaxFramesPerCall = 50 * 60 * 5;

    private readonly SemaphoreSlim _lock = new(1, 1);

    /// <summary>
    /// Runs an action while the emulator is paused, then restores the previous run state.
    /// </summary>
    internal Task<T> ExecuteAsync<T>(Func<Emulator, T> action) => ExecuteAsync(action, leavePaused: false);

    /// <summary>
    /// Runs an action while the emulator is paused.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <param name="leavePaused">When true, the emulator stays paused after the action, otherwise the previous state is restored.</param>
    internal async Task<T> ExecuteAsync<T>(Func<Emulator, T> action, bool leavePaused)
    {
        await _lock.WaitAsync();

        try
        {
            var emulator = host.Emulator ?? throw new McpException("Emulator is not running.");
            var wasPaused = emulator.IsPaused;

            if (!wasPaused)
            {
                await host.PauseAsync();
            }

            try
            {
                return action(emulator);
            }
            finally
            {
                if (!wasPaused && !leavePaused)
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
    /// </summary>
    /// <returns>Number of frames that were actually executed, fewer than requested if a debugger breakpoint was hit.</returns>
    internal static int RunFrames(Emulator emulator, int count)
    {
        for (var i = 0; i < count; i++)
        {
            emulator.RunFrame();

            if (emulator.IsDebuggerBreak)
            {
                return i + 1;
            }
        }

        return count;
    }

    /// <summary>
    /// Presses each key combination in turn, holding it for <paramref name="holdFrames"/> frames
    /// and releasing for <paramref name="releaseFrames"/> frames, so the ROM keyboard routine registers every press.
    /// </summary>
    internal static int PressKeys(Emulator emulator, IEnumerable<SpectrumKey[]> combinations, int holdFrames, int releaseFrames)
    {
        var frames = 0;

        foreach (var keys in combinations)
        {
            emulator.KeyboardState.KeyDown(keys);
            frames += RunFrames(emulator, holdFrames);

            emulator.KeyboardState.KeyUp(keys);
            frames += RunFrames(emulator, releaseFrames);

            if (emulator.IsDebuggerBreak)
            {
                break;
            }
        }

        return frames;
    }
}
