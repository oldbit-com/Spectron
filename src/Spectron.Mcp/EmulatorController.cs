using ModelContextProtocol;
using OldBit.Spectron.Debugger.Breakpoints;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices.Keyboard;
using OldBit.Spectron.Emulation.Devices.Memory;

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

/// <summary>
/// What stopped execution: a user breakpoint, or the temporary target of a step or run_until call.
/// </summary>
/// <param name="Emulator">The emulator that stopped.</param>
/// <param name="Breakpoint">The breakpoint that was hit, null for a step out target.</param>
/// <param name="PreviousAddress">The address of the previous instruction, or of the instruction that wrote to memory.</param>
/// <param name="IsTemporary">True when the temporary target of the current call stopped execution, not a user breakpoint.</param>
internal sealed record BreakpointHit(Emulator Emulator, Breakpoint? Breakpoint, Word PreviousAddress, bool IsTemporary);

/// <param name="Frames">The number of frames run. A frame that stopped part way counts.</param>
/// <param name="StopReason">Why the run stopped early, if it did.</param>
/// <param name="Hit">For a breakpoint stop, what stopped execution, when known.</param>
internal readonly record struct RunResult(int Frames, StopReason StopReason, BreakpointHit? Hit = null);

/// <summary>
/// Serializes access to the emulator for MCP tools. Every operation runs while the emulator
/// is paused, so tools see a consistent state and can drive frames themselves.
/// The user can still interact with the UI at any time, so frame-driving operations stop
/// as soon as the user resumes emulation or replaces the machine (e.g. loads a snapshot).
/// </summary>
public sealed class EmulatorController(IEmulatorHost host) : IDisposable
{
    internal const int MaxFramesPerCall = 50 * 60 * 5;

    private const int MaxPauseAttempts = 3;

    // The 128 editor takes about 30 frames to redraw after ENTER, more on a full screen
    private const int KeyReadTimeoutFrames = 100;

    private const Word Flags = 23611;
    private const byte NewKeyFlag = 0x20;   // FLAGS bit 5: a new key is waiting in LAST_K

    private readonly SemaphoreSlim _lock = new(1, 1);

    private BreakpointHandler? _subscribedBreakpoints;
    private volatile BreakpointHit? _lastHit;

    /// <summary>
    /// Gets the user's breakpoints, shared with the debugger window, or null when there is no machine yet.
    /// </summary>
    internal BreakpointHandler? Breakpoints => host.Breakpoints;

    /// <summary>
    /// Runs an action while the emulator is paused, then restores the previous run state.
    /// </summary>
    internal Task<T> ExecuteAsync<T>(Func<Emulator, T> action) =>
        ExecuteAsync((emulator, _) => action(emulator), leavePaused: false);

    /// <summary>
    /// Runs an action that changes memory or registers while the emulator is paused, then restores the previous
    /// run state. An open debugger window is refreshed afterwards.
    /// </summary>
    internal Task<T> ModifyAsync<T>(Func<Emulator, T> action) =>
        ExecuteAsync((emulator, _) => action(emulator), leavePaused: false, changesState: true);

    /// <summary>
    /// Runs an action that drives frames while the emulator is paused. A pending debugger break is cleared first,
    /// because running is an explicit request: execution continues where the breakpoint stopped it, without
    /// starting the timer. An open debugger window is refreshed afterwards.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <param name="leavePaused">When true, the emulator stays paused after the action, otherwise the previous state is restored.</param>
    internal Task<T> RunAsync<T>(Func<Emulator, T> action, bool leavePaused = false) =>
        ExecuteAsync((emulator, _) =>
        {
            emulator.ClearBreak();
            _lastHit = null;

            return action(emulator);
        }, leavePaused, changesState: true);

    /// <summary>
    /// Runs an action while the emulator is paused.
    /// </summary>
    /// <param name="action">The action to execute, receives the emulator and whether it was paused before the call.</param>
    /// <param name="leavePaused">When true, the emulator stays paused after the action, otherwise the previous state is restored.</param>
    /// <param name="changesState">When true, the action may change memory or registers, so the host is told afterwards.</param>
    internal async Task<T> ExecuteAsync<T>(Func<Emulator, bool, T> action, bool leavePaused, bool changesState = false)
    {
        await _lock.WaitAsync();

        try
        {
            SubscribeToBreakpoints();

            var (emulator, wasPaused) = await PauseEmulatorAsync();

            try
            {
                return action(emulator, wasPaused);
            }
            finally
            {
                if (changesState && IsCurrent(emulator))
                {
                    host.NotifyStateChanged();
                }

                // Don't resume when a breakpoint was hit (the agent or the debugger takes over), or when the machine
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

    internal async Task<T> RunHostActionAsync<T>(Func<IEmulatorHost, Task<T>> action)
    {
        await _lock.WaitAsync();

        try
        {
            return await action(host);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Gets what stopped execution, while the given emulator is stopped at a breakpoint.
    /// </summary>
    /// <returns>The hit, or null when the emulator is not stopped at a breakpoint or the hit is not known.</returns>
    internal BreakpointHit? GetCurrentHit(Emulator emulator)
    {
        var hit = _lastHit;

        return emulator.IsDebuggerBreak && hit != null && ReferenceEquals(hit.Emulator, emulator) ? hit : null;
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
                return new RunResult(i, stopReason, GetCurrentHit(emulator));
            }

            emulator.RunFrame();
        }

        return GetStopReason(emulator) == StopReason.Breakpoint
            ? new RunResult(count, StopReason.Breakpoint, GetCurrentHit(emulator))
            : new RunResult(count, StopReason.None);
    }

    /// <summary>
    /// Executes a single instruction. A user breakpoint hit by the instruction (e.g. a memory write) or at the next
    /// instruction is reported in the result.
    /// </summary>
    internal RunResult Step(Emulator emulator)
    {
        var stopReason = GetStopReason(emulator);

        if (stopReason is StopReason.EmulatorReplaced or StopReason.ResumedByUser)
        {
            return new RunResult(0, stopReason);
        }

        _lastHit = null;
        emulator.Step();

        return GetStopReason(emulator) == StopReason.Breakpoint
            ? new RunResult(0, StopReason.Breakpoint, GetCurrentHit(emulator))
            : new RunResult(0, StopReason.None);
    }

    /// <summary>
    /// Runs frames until the temporary breakpoint is hit, a user breakpoint is hit, or <paramref name="maxFrames"/>
    /// frames have run. The breakpoint lives in its own handler for this call only, so it never shows in the user's
    /// list, nothing is left behind, and it works even when the user's breakpoints are turned off.
    /// </summary>
    internal RunResult RunToBreakpoint(Emulator emulator, Breakpoint breakpoint, int maxFrames)
    {
        using var handler = new BreakpointHandler(emulator);

        handler.BreakpointManager.AddBreakpoint(breakpoint);

        // Register breakpoints fire when the value changes to the target. Take the current values first, so a
        // condition that is already true (e.g. PC at the target) fires the next time it becomes true, not at once.
        handler.BreakpointManager.FindRegisterBreakpointHit();

        handler.BreakpointHit += (_, e) => RecordHit(emulator, e, isTemporary: true);
        handler.IsEnabled = true;

        return ClearTemporaryBreak(emulator, RunFrames(emulator, maxFrames));
    }

    /// <summary>
    /// Runs frames until the current routine returns: a RET, RETI or RETN that leaves the stack pointer above its
    /// value at the start. Interrupt routines that run meanwhile restore the stack pointer, so their returns
    /// don't count. Also stops at user breakpoints and after <paramref name="maxFrames"/> frames.
    /// </summary>
    internal RunResult RunToReturn(Emulator emulator, int maxFrames)
    {
        var startSp = emulator.Cpu.Registers.SP;
        var isReturn = false;
        Word previousPc = 0;

        emulator.Cpu.BeforeInstruction += BeforeInstruction;

        try
        {
            return ClearTemporaryBreak(emulator, RunFrames(emulator, maxFrames));
        }
        finally
        {
            emulator.Cpu.BeforeInstruction -= BeforeInstruction;
        }

        void BeforeInstruction(Word pc)
        {
            if (isReturn && (short)(emulator.Cpu.Registers.SP - startSp) > 0)
            {
                emulator.Break();
                RecordHit(emulator, new BreakpointHitEventArgs(previousPc), isTemporary: true);
            }

            isReturn = IsReturnInstruction(emulator.Memory, pc);
            previousPc = pc;
        }
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

    // Reaching the target of a step or run_until is a normal end of the call, not a stop at a breakpoint,
    // so the next frame-driving call or the user's resume doesn't see a pending break.
    private RunResult ClearTemporaryBreak(Emulator emulator, RunResult result)
    {
        if (result.Hit is { IsTemporary: true })
        {
            emulator.ClearBreak();
            _lastHit = null;
        }

        return result;
    }

    // RET, RET cc, RETN and RETI (with their ED mirrors)
    private static bool IsReturnInstruction(IEmulatorMemory memory, Word pc)
    {
        var opCode = memory.Read(pc);

        return opCode switch
        {
            0xC9 => true,
            _ when (opCode & 0b1100_0111) == 0b1100_0000 => true,
            0xED => (memory.Read((Word)(pc + 1)) & 0b1100_0111) == 0b0100_0101,
            _ => false
        };
    }

    /// <summary>
    /// Follows the user's breakpoint hits, including those that happen while the user runs the emulator,
    /// so tools can say which breakpoint stopped execution. The host creates its handler with the first machine,
    /// which may be after the server started, so this is checked on every call.
    /// </summary>
    private void SubscribeToBreakpoints()
    {
        var breakpoints = host.Breakpoints;

        if (ReferenceEquals(breakpoints, _subscribedBreakpoints))
        {
            return;
        }

        _subscribedBreakpoints?.BreakpointHit -= OnUserBreakpointHit;
        _subscribedBreakpoints = breakpoints;
        _subscribedBreakpoints?.BreakpointHit += OnUserBreakpointHit;
    }

    private void OnUserBreakpointHit(object? sender, BreakpointHitEventArgs e)
    {
        if (host.Emulator is { } emulator)
        {
            RecordHit(emulator, e, isTemporary: false);
        }
    }

    // Called on the emulation thread. A user breakpoint and a temporary target can fire at the same instruction,
    // in either order: the user breakpoint wins.
    private void RecordHit(Emulator emulator, BreakpointHitEventArgs e, bool isTemporary)
    {
        var last = _lastHit;

        if (isTemporary && last is { IsTemporary: false } && ReferenceEquals(last.Emulator, emulator) && emulator.IsDebuggerBreak)
        {
            return;
        }

        _lastHit = new BreakpointHit(emulator, e.Breakpoint, e.PreviousAddress, isTemporary);
    }

    public void Dispose()
    {
        _subscribedBreakpoints?.BreakpointHit -= OnUserBreakpointHit;
        _subscribedBreakpoints = null;
    }
}
