# Step 4: breakpoint and stepping tools (proposal)

Status: DONE. PR 1 merged as #93 (2026-11-26). PR 2 (MCP tools) implemented on branch `feature/mcp-debugger-tools` (2026-11-26).
The proposal was based on reading `main` at `618aed2`.

> **PR 1 notes.**
> - Verified in the Z80 repo (`~/Projects/ZX/Z80`, branch `spectron`): INT is accepted only at the top of the
>   `Run()` loop, never in `Step()`; a halted CPU runs NOPs inside `Run()` without raising `BeforeInstruction`;
>   `Clock.IsFrameComplete` covers both the tick and the RZX fetch limit.
> - `Emulator.Step()` therefore runs through `Run()`. When an interrupt is accepted, the step stops at the start
>   of the interrupt routine (`$0038` in IM 1) without executing it. A step on `HALT` runs to the end of the frame.
> - `StepOver.GetReturnAddress` (`src/Spectron.Debugger/Stepping/StepOver.cs`) uses fixed lengths per opcode
>   instead of the disassembler; it adds `RST`, `HALT` and all eight repeating block instructions.
> - The debugger removes leftover single-use breakpoints on every hit and every pause.
> - Also fixed: `DebuggerViewModel` was never disposed when the window closed (stale `BreakpointHit` handlers),
>   and an old `BreakpointListViewModel` stayed registered with the messenger after the machine was replaced.
>   A register breakpoint no longer fires a second time at the same place when two breakpoints match at once.
> - Tests: `tests/Spectron.Emulator.Tests/EmulatorSteppingTests.cs`, `tests/Spectron.Debugger.Tests/Breakpoints/`,
>   `tests/Spectron.Debugger.Tests/Stepping/StepOverTests.cs`.

> **PR 2 notes.**
> - `IEmulatorHost.Breakpoints` exposes the shared `BreakpointHandler` (not only the manager): the controller needs its
>   `BreakpointHit` event to say which breakpoint fired, and `IsEnabled` to report the state. `IsEnabled` is only changed
>   through `EnableBreakpointsAsync`, so the menu item follows.
> - `EmulatorController` follows the user's `BreakpointHit` for its lifetime (it is disposed with the server), so
>   `get_status` and `list_breakpoints` can name a breakpoint hit while the user was running. `RunAsync` (frame-driving
>   tools) clears a pending break first, `ModifyAsync` (`write_memory`, `debugger_command`) and `RunAsync` call
>   `NotifyStateChanged`, which refreshes an open `DebuggerViewModel` (its `Refresh` is now public).
> - `step over` and `run_until` use a temporary `BreakpointHandler` per call. Before running, its register breakpoint
>   takes the current values, so `run_until PC==$8000` at `$8000` runs until execution comes back. Reaching the target
>   clears the break, so it is not reported later as "stopped at a breakpoint".
> - `step out` does not use a `BreakpointHandler`: the condition is "a RET/RETI/RETN leaves SP above its value at the
>   start", which a register breakpoint can't express. A plain `PC==(SP)` target, as in the debugger, breaks when the
>   routine has pushed something, and stops early in recursion. It hooks `BeforeInstruction` for the call only.
> - A user breakpoint and a temporary target firing at the same instruction report the user breakpoint.
> - Agent-set conditions get a canonical text (`PC == $8000`, `$5C3A == $00`), like the debugger's own.
>   Setting an existing condition returns its id, or enables it if it was disabled.
> - `MainViewModel.OnBreakpointHit` doesn't open the window while `IsPaused` is true.
> - Tests: `tests/Spectron.Mcp.Tests/DebuggerToolsTests.cs`.

Goal from `plans/mcp-next-steps.md`: agents can set, list and clear breakpoints and step through code,
not only hit breakpoints that the user set in the debugger.

---

## 1. How it works today

| Piece | Where | Behaviour |
|---|---|---|
| Breakpoint store | `BreakpointManager` (in `BreakpointHandler`), owned by `MainViewModel._breakpointHandler` | Survives machine replacement (`ConfigureDebugging` calls `Update(emulator)`). Not persisted. |
| Hit detection | `BreakpointHandler` | Register breakpoints on `Cpu.BeforeInstruction` (edge-triggered via `LastValue`), memory breakpoints on `Memory.MemoryUpdated` (writes only). A hit calls `Emulator.Break()`. |
| Stop | `Z80.Run()` (OldBit.Z80Cpu) | Checks the break flag after `BeforeInstruction`, before `Step()`. A register breakpoint stops *before* the instruction at PC; a memory breakpoint stops before the *next* instruction. |
| After the stop | `Emulator.RunFrameCore` | `EndFrame()` still runs for the partial frame. Resuming with `Resume(isDebuggerResume: true)` skips `StartFrame()` once. |
| Hit in the UI | `MainViewModel.OnBreakpointHit` | Opens the debugger window (unless already open) and sets `BreakpointsEnabled = true`. |
| Stepping | `DebuggerViewModel.DebuggerStepInto/Over/Out` | Into: `Cpu.Step()` directly. Over/Out: a single-use `PC==` breakpoint, then resume the timer. |
| MCP | `EmulatorController.RunFrames` | Stops when `IsDebuggerBreak` is set and reports `StopReason.Breakpoint`. |

## 2. Problems found

These matter for MCP, and most are also real bugs or rough edges in the debugger.

1. **A partial frame is ended twice.** After a break, `EndFrame()` runs on the partial frame: a full frame of
   beeper/AY samples, a border update, `FrameCompleted` (the recorder appends a frame, quick-save check) and a
   time machine entry. The rest of the frame then ends again on resume. Today that happens once per breakpoint
   hit. If stepping went through the frame loop, it would happen once per *instruction*.
2. **The agent can't continue after a break.** `IsDebuggerBreak` is only cleared by `Emulator.Resume()`, which
   starts the timer. Until the user resumes, every frame-driving tool returns immediately with "Stopped at a
   debugger breakpoint". Resuming through `IEmulatorHost.ResumeAsync()` would call `Resume(false)`, so
   `StartFrame()` resets the clock mid-frame and loses the rest of that frame.
3. **The debugger's Step Into bypasses the emulator.** `Cpu.Step()` is called on the UI thread:
   - no frame lock, so it can race with an MCP frame;
   - no `BeforeInstruction`, so ROM tape traps don't run and breakpoint `LastValue`s go stale;
   - at the end of a frame only `Cpu.Clock.NewFrame()` runs: no `StartFrame`/`EndFrame` for screen, audio,
     Beta 128 or time machine, and no `FrameCompleted`, so the main screen doesn't refresh while stepping;
   - interrupt acceptance and `HALT` depend on what the CPU package does in `Step()` versus `Run()`.
     **Verify:** the local `Z80/src/Z80Cpu/Z80.cs` never calls `TriggerInt`, so the `OldBit.Z80Cpu.Spectron`
     1.0.10 package must accept INT internally (it has `Clock.IsInterruptWindow`). Check whether that is in
     `Run()` or `Step()` before relying on either.
4. **`BreakpointHandler.IsEnabled` is not honoured at creation.** The constructor and `Update()` subscribe to
   CPU and memory events unconditionally. `BreakpointsEnabled` starts `false` (menu unchecked) but breakpoints
   fire anyway. After the user unchecks the menu, loading a snapshot (`Update`) silently re-enables them.
5. **The hit doesn't say which breakpoint fired.** `BreakpointHitEventArgs` only has `PreviousAddress`.
6. **Memory breakpoint check returns too early.** `IsMemoryBreakpointHit` returns on the first enabled breakpoint
   at the address even when its value doesn't match, so a second breakpoint at the same address is never checked.
7. **Not thread-safe.** The list is changed from the UI thread (and with MCP, from the tool thread) while the
   emulation thread iterates `_breakpoints.Register[index]`, re-reading the property each iteration. A swap
   between two iterations can throw `IndexOutOfRange`. Single-use breakpoints are also removed from the emulation thread.
8. **Single-use breakpoints can linger.** Step Over/Out leave a hidden `PC==` breakpoint that fires much later if
   the return address is never reached (stack manipulated, `JP (HL)` out of the routine, user pauses and loads a
   snapshot). `BreakpointListViewModel` also lists them when it is built from `Breakpoints`.
9. **The debugger window only knows its own changes.** `BreakpointListViewModel` adds to its collection only
   from its own commands, and `DebuggerViewModel` refreshes only on its own step or a hit. Breakpoints added
   by an agent, or memory and registers changed by `write_memory`/`debugger_command`/`run_frames`, leave the
   window stale.
10. Step Over doesn't treat `RST` as a call, and uses hard-coded lengths (+2/+3) instead of the instruction length.

## 3. Proposed changes

Two PRs. PR 1 has no MCP code and fixes the debugger itself, PR 2 adds the tools on top.

### PR 1: emulation and debugger groundwork

**A. Track a frame in progress in `Emulator`** (`src/Spectron.Emulation/Emulator.cs`)

- Replace `_isDebuggerResume` with `_isFrameInProgress`, owned by the emulator.
- `RunFrameCore`:
  ```csharp
  if (!_isFrameInProgress) StartFrame();
  // NMI as now
  while (!IsFrameComplete && !IsDebuggerBreak && !_isStepComplete) Cpu.Run();
  if (!IsFrameComplete) { _isFrameInProgress = true; ExecutionStopped?.Invoke(); return; }   // no EndFrame
  _isFrameInProgress = false;
  EndFrame();
  ```
  Looping on `Cpu.Run()` also absorbs a stale break flag left inside the CPU (a memory breakpoint on the last
  instruction of a frame sets `_isBreakpoint`, which the CPU only clears on the next `Run`).
  **Verify** the RZX path: frames end on a fetch count there, and `Clock.IsFrameComplete` is tick-based.
- `Resume(bool isDebuggerResume)` loses its parameter; the emulator knows whether it is mid-frame.
  `MainViewModel.Resume(isDebuggerResume: true)` callers simplify.
- New `event Action? ExecutionStopped`, raised on a stop mid-frame. `MainViewModel` uses it to redraw the
  screen bitmap only (the frame buffer is already updated up to the beam position through `TicksAdded`),
  without audio, recorder, quick-save or time machine. This replaces what `FrameCompleted` does today at a break.
- `internal void ClearBreak() => IsDebuggerBreak = false;` so a caller can continue without starting the timer.
- `internal void Step()`: one instruction through the same path, under the frame lock. It subscribes a
  one-shot `BeforeInstruction` handler for the call that sets `_isStepComplete` and calls `Cpu.Break()` on
  the *second* `BeforeInstruction`, then runs `RunFrameCore`. So interrupts, `HALT`, contention, ROM traps,
  screen, audio and frame boundaries all behave exactly as in normal running:
  - on `HALT`, `Run()` consumes the rest of the frame, so the step ends at the frame boundary; the next step
    accepts the interrupt;
  - if the instruction completes the frame, `EndFrame()` runs once, as it should.

**B. Debugger uses the new primitive** (`src/Spectron.Debugger/ViewModels/DebuggerViewModel.cs`)

- `DebuggerStepInto` calls `Emulator.Step()` (internals are already visible to `OldBit.Spectron.Debugger`).
- `DebuggerResume` and the window-closed path use the new `Resume()`.
- Move the step-over logic (`IsCallInstruction` etc.) into a small shared helper in `Spectron.Debugger`, for
  example `Stepping/StepOverTarget.GetReturnAddress(memory, pc)`, using the disassembler for the instruction
  length and adding `RST`. MCP uses the same helper.

**C. Breakpoint model** (`src/Spectron.Debugger/Breakpoints/`)

- `Breakpoint.Id` (int, assigned by `BreakpointManager`, never reused in a session), so agents can refer to one.
- `BreakpointManager.Changed` event (added, removed, updated). `BreakpointListViewModel` subscribes (posting
  to the UI thread) instead of changing its collection directly, and sends the existing
  `BreakpointAdded/Removed/UpdatedMessage` from there, so code-line markers keep working whoever made the change.
- `BreakpointHitEventArgs.Breakpoint`: the breakpoint that fired (the `Is…Hit` methods return it).
- Fix problem 6 (memory breakpoint early return).
- Thread safety: lock around add/remove/replace in `BreakpointList`, keep copy-on-write lists, and take a
  local copy of the list at the start of each `Is…Hit` loop.
- `BreakpointHandler` subscribes only when `IsEnabled` (constructor and `Update`). This makes the menu item
  truthful. Behaviour change: breakpoints set in the debugger stay active after the window closes only while
  *Breakpoints Enabled* is checked (opening the debugger checks it, as now).
- Single-use breakpoints are excluded from `Breakpoints` as seen by the list view.

Tests (`tests/Spectron.Emulator.Tests`, `tests/Spectron.Debugger.Tests`):
a break mid-frame then continue runs exactly one frame's T-states and raises `FrameCompleted` once;
`Step()` across a frame boundary with IM 1 and interrupts enabled lands at `$0038`; `Step()` on `HALT`;
ROM trap still fires when stepping onto the tape routine; manager ids, `Changed` events, the memory breakpoint fix,
and `IsEnabled` honoured after `Update`.

### PR 2: MCP tools

**D. Host and controller**

- `IEmulatorHost`:
  - `BreakpointManager Breakpoints { get; }` (the user's, shared; `Spectron.Mcp` already references `Spectron.Debugger`);
  - `Task EnableBreakpointsAsync()`: sets `BreakpointsEnabled = true` on the UI thread, so the menu shows it;
  - `void NotifyStateChanged()`: called by `EmulatorController.ExecuteAsync` after any tool that ran frames or
    changed memory or registers. `MainViewModel` posts `_debuggerViewModel?.Refresh()` to the UI thread (problem 9).
- `MainViewModel.OnBreakpointHit` doesn't open the debugger window when emulation is paused. While the timer is
  paused, frames only run from a tool call (or a debugger step, when the window is already open), so a hit there
  belongs to the agent and is reported in the tool result. An open debugger window still refreshes, so the user
  sees where the agent stopped.
- Frame-driving tools (`run_frames`, `type_text`, `press_keys`, `type_basic`, `step`, `run_until`) call
  `emulator.ClearBreak()` first. Running is an explicit request, so a pending break doesn't block the agent
  (problem 2), and with A the frame continues where it stopped. `ExecuteAsync` still never resumes the timer
  after a break.
- `RunResult` carries the hit `Breakpoint` (from `BreakpointHit`) and the previous PC, so results can say
  *which* breakpoint fired.

**E. Tools** (`src/Spectron.Mcp/Tools/`, maybe a new `DebuggerTools.cs` to keep `EmulatorTools.cs` from growing)

| Tool | Parameters | Behaviour |
|---|---|---|
| `set_breakpoint` | `condition` | Same syntax as the debugger list: `PC==$8000`, `A==5`, `HL==16384` (register reaches value), `$5C3A` (any write to address), `$5C3A==0` (write of value). Parsed with `BreakpointParser`; error text explains the syntax. Turns breakpoints on if off and says so. An existing identical condition returns its id. Result: `Breakpoint #3 set: PC==$8000`. |
| `list_breakpoints` | — | Id, condition, enabled; whether breakpoints are enabled globally; which one stopped execution, if any. |
| `clear_breakpoint` | `id` or `all` | Removes from the shared list (the debugger window updates through `Changed`). |
| `step` | `mode` = `into`/`over`/`out`, `count` = 1 (1–1000) | `into`: `Emulator.Step()`. `over`: for `CALL`/`RST`/`DJNZ`/block instructions, run until the return address (shared helper from B), otherwise one step. `out`: run until a `RET`-family instruction pops SP above its value at the start. Over and out stop at the user's breakpoints too and have a frame budget (default 50), so they can't run forever. Result: executed instruction(s), registers, next instruction, frame T-state. |
| `run_until` | `condition`, `maxFrames` = 500 | Runs until the condition is true, without adding a breakpoint the user sees. |

`step over/out` and `run_until` don't use single-use breakpoints in the shared list (problem 8). They create a
temporary `BreakpointHandler(emulator)` for the call with one breakpoint and dispose it in `finally`. It hooks the
same CPU and memory events and calls `Emulator.Break()`, but its `BreakpointHit` isn't wired to `MainViewModel`, so
no window opens and nothing lingers. It also works when the user has breakpoints turned off. The `sender` of the hit
tells the temporary target apart from a user breakpoint.

Result text examples:

```
Stopped at breakpoint #2 (PC==$8000) after 12 frame(s), at frame T-state 31744. PC=$8000, previous PC=$7FFD.
Stepped 1 instruction: $8000 LD A,(HL) → PC=$8001. A=$3C F=$44 (Z) HL=$4000 ...
Stepped over CALL $8100: stopped at $8003 after 1 frame(s).
```

Also: `get_status` gains *Breakpoints: 2 (enabled)* and *Stopped at breakpoint #2*.

**F. Docs**: tool descriptions (agents read these), the server instructions in `McpServer.cs` (a short
"debugging machine code" workflow: `write_memory` or `load_file`, `set_breakpoint`, `run_frames`, `step`),
`docs/Mcp.md`, `README.md`, and step 4 in `plans/mcp-next-steps.md`.

Tests (`tests/Spectron.Mcp.Tests`, using `TestEmulatorHost` extended with a `BreakpointHandler`): poke a short routine
(`LD A,5 / CALL sub / INC A / JR $`) into RAM and set PC with `debugger_command GOTO`, then set, list and clear
breakpoints; `run_frames` stops at a PC breakpoint and reports it; continuing after a break; step into/over/out;
`run_until` leaves no breakpoint behind; a memory write breakpoint; an invalid condition error.
Live check against the app: no debugger window during an agent run, the open window's list and code view
update when the agent changes breakpoints or steps.

## 4. Decisions for the user

1. **Shared breakpoints** (recommended): the agent and the user see one list, visible in the debugger window.
   Alternative: agent-only breakpoints in a separate manager, invisible in the UI.
2. **No debugger window for agent hits** (recommended): the agent gets the result; an already open window refreshes.
   Alternative: always open it, as for user runs.
3. **The agent may continue past a break the user is looking at** (recommended, since a tool call is explicit).
   Alternative: refuse while the debugger window is open.
4. **Fix `IsEnabled` (problem 4)**: makes the menu item truthful, but changes when breakpoints fire.
5. **Skip `EndFrame` on a partial frame (A)**: fixes audio/recording glitches at every break, needs the new
   `ExecutionStopped` event for the screen redraw.
