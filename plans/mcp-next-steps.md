# MCP server: handover and next steps

Status as of 2026-11-22. The experimental MCP server was merged into `main` in PR #78
(squashed as `6122385`). Baseline on `main`: solution builds with 0 warnings, all 57 tests in
`Spectron.Mcp.Tests` pass.

Each step below can be done on its own, in its own session and branch. Start a session by pointing it
at this file and naming the step, for example:
*"Read plans/mcp-next-steps.md and do step 1 (type_basic)."*

---

## Context for a new session

### What exists

| File | Role |
|---|---|
| `src/Spectron.Mcp/McpServer.cs` | Kestrel host, Streamable HTTP on `127.0.0.1:<port>/mcp`, stateless, server instructions text |
| `src/Spectron.Mcp/IEmulatorHost.cs` | What the app exposes: `Emulator`, `IsPaused`, `PauseAsync`, `ResumeAsync`, `ResetAsync`, `LoadFileAsync` |
| `src/Spectron.Mcp/EmulatorController.cs` | Serialises tool calls, pauses or restores the run state, drives frames, `StopReason` |
| `src/Spectron.Mcp/Tools/EmulatorTools.cs` | The 15 tools. Tool and parameter `[Description]` texts are what agents read |
| `src/Spectron.Mcp/Input/KeyMapper.cs` | Text and key names to `SpectrumKey` combinations |
| `src/Spectron.Mcp/Screen/ScreenTextReader.cs` | Display file to text via ROM font matching (inverse and block graphics) |
| `src/Spectron.Emulation/Screen/PngEncoder.cs` | PNG encoding (moved here from Spectron.Mcp in #81) |
| `src/Spectron/ViewModels/MainViewModel.Mcp.cs` | `IEmulatorHost` implementation, start or stop from preferences and command line |
| `src/Spectron/Settings/McpServerSettings.cs` | `IsEnabled`, `Port` (default 5480) |
| `tests/Spectron.Mcp.Tests/` | Tool, key mapper and screen reader tests. `Fixtures/TestEmulatorHost` uses a real emulator with its timer never started |
| `docs/Mcp.md`, `README.md` | User documentation. Update both when tools change |

### Design rules to keep

- Every tool runs through `EmulatorController.ExecuteAsync`. It pauses the emulator, runs the action and
  restores the previous run state. `run_frames` passes `leavePaused: true`.
- The run state comes from `IEmulatorHost.IsPaused`, which is what the UI shows, not `Emulator.IsPaused`.
  This also makes it testable, because the test host never starts the emulator's timer, since that would
  open an audio device.
- Frame loops (`RunFrames`, `PressKeys`) check `StopReason` before every frame: breakpoint hit, user resumed,
  or emulator replaced. Never resume after a breakpoint or after the emulator was replaced.
- `Emulator.RunFrame` takes a frame lock, so the timer thread and MCP never run frames at the same time.
- UI lifecycle operations go through the host on the UI thread (`Dispatcher.UIThread.InvokeAsync`).
- Input errors are reported with `McpException`. The SDK returns those as tool errors with the message;
  other exceptions become a generic error.

### Live testing against the real app

- Launch: `dotnet run --project src/Spectron --no-build -- --mcp-server --no-resume --mute --computer Spectrum48K`
  (run it in the background).
- Stop it with `kill -9 <pid>`, **not** by closing the window. A normal close saves the session snapshot and
  preferences and would overwrite the user's own.
- Avalonia fails at startup with native error `-6661` when the Mac display is asleep or locked. That is not a
  code problem; ask the user to unlock the screen.
- A small Python client is enough: POST JSON-RPC to `http://localhost:5480/mcp` with headers
  `Content-Type: application/json`, `Accept: application/json, text/event-stream` and
  `MCP-Protocol-Version: 2025-06-18`. The response is SSE, so take the line starting with `data: `.
  Stateless mode, so `tools/call` works without `initialize`.
- The user has *Automatically pause when window is deactivated* turned **off**. `F2` toggles pause, which is
  the easiest way to simulate the user taking over.

### Gotchas learned so far

- 48K BASIC keyword entry: at the `K` cursor a letter is a whole keyword. Symbol Shift and Extended mode
  keywords need `press_keys`. The key map in the `press_keys` description was verified against the 48K ROM.
- The ROM ignores a key pressed again within about 5 frames of release (default `releaseFrames` is 6).
- The ROM prints functions without a leading space (`10SQR`) but commands with one (`10 CIRCLE`).
  Keywords such as `TO` and `AND` show with a single space in the edit line (`FOR TO`).
- In the debugger grammar, `PEEK` and `IN` are statements, not expressions: `PEEK 23560` works,
  `PRINT PEEK 23560` returns nothing.
- The `Microsoft.Extensions.*` versions must be at least what the MCP SDK depends on, otherwise the
  build fails with `NU1605` (package downgrade).
- `ModelContextProtocol.AspNetCore` needs `<FrameworkReference Include="Microsoft.AspNetCore.App" />`,
  which increases self-contained publish size.

---

## Step 1: `type_basic` tool (48K keyword-aware entry) — DONE

> Implemented on branch `feature/type-basic` (2026-11-25): `src/Spectron.Mcp/Basic/`, tests in
> `tests/Spectron.Mcp.Tests/Basic/`. The `BasicKeywords` token table can be reused by step 2.

> **Detailed plan: `plans/type-basic.md`** (verified against the ROM and a probe on 2026-11-25). It supersedes
> points 2 and 4 below and the "Reuse" note. Wait for the editor to be idle before each key press, decide
> success from memory instead of the screen, derive the keyword table from the ROM, and handle the REM `:`
> and caps lock quirks.

**Problem.** Entering a 48K program with `type_text` and `press_keys` works but is fiddly. The agent has to
plan every keyword key and cursor mode itself, and a short program takes dozens of tool calls.
A real session needed several attempts to enter `CIRCLE`.

**Goal.** A tool that takes a plain listing and enters it through the ROM editor, for example:

```
10 BORDER 1: CIRCLE 128,88,50
20 FOR i=1 TO 10 STEP 2: PRINT AT i,0;"X": NEXT i
30 IF a<=b THEN GO TO 10
```

**Approach (agreed).** Type through the editor, so the ROM still checks syntax and errors show as they
would for a user. Do not write tokenised bytes into memory.

1. Split each line into the line number, keywords and plain characters. Match keywords longest first
   (`GO TO` before `GO`, `INKEY$`, `DEF FN`), case-insensitively, and only on word boundaries outside
   strings and after `REM`. Watch out for keywords that are prefixes of each other or of variable names
   (`IN` vs `INK`/`INPUT`/`INT`, `TO` in `TOTAL`, `AT` in `ATTR`/`CAT`).
2. Before each keyword, read the cursor mode from system variables. The tool drives frames, so memory
   can be read between key presses. The relevant system variables are `FLAGS` (23611) and `MODE` (23617).
   **Verify which bits mean K or L mode against a ROM disassembly or with a probe test before relying on them.**
3. Choose the key sequence:
   - K mode and a command keyword: the single key (table below).
   - Symbol Shift keywords (`AND OR TO STEP THEN AT NOT STOP <= <> >=`): `SYM+<key>`, in K or L mode.
   - Extended mode keywords: `EXTEND`, then the key or `SYM+<key>`.
   - A command keyword needed while the cursor is in L mode: report a clear error, because it is
     usually a syntax problem in the listing.
4. Type plain characters with the existing `KeyMapper.FromText`. Press ENTER at the end of each line.
   After each line, use `ScreenTextReader` to check the bottom lines. If the ROM rejected the line
   (the edit line is still there with a `?` marker), stop and report which line failed.

**Key tables.** Symbol Shift and Extended mode keys are already in the `press_keys` description and were
verified. K-mode command keys (verify with a probe test, as was done for the others):
A NEW, B BORDER (verified), C CONTINUE, D DIM, E REM, F FOR (verified), G GO TO, H GO SUB, I INPUT,
J LOAD, K LIST, L LET, M PAUSE, N NEXT, O POKE, P PRINT (verified), Q PLOT, R RUN, S SAVE,
T RANDOMIZE, U IF (verified), V CLS, W DRAW, X CLEAR, Y RETURN, Z COPY.

A quick way to verify a table is a throwaway xunit `[Theory]` that presses keys on a booted 48K machine
(`TestEmulatorHost`, `RunFrames(150)`) and asserts the last non-empty screen line.

**Reuse.** The `zx-basic` branch has `src/Spectron.Basic/Tokenizer/Keywords.cs`, a table of the 91 token
bytes and keyword names (`0xA5` RND to `0xFF` COPY), plus a detokeniser (`BasicMemoryReader`,
`BasicTokenizer`). That branch is work in progress. Copy the keyword names rather than depending on it,
or move the table into a shared place if that branch is going to be merged.

**128K.** The 128K editor accepts keywords spelled out, so plain `type_text` works there. Decide whether
`type_basic` detects 128K BASIC and types words, or only supports 48K and says so. Note that 128K machines
can also be in "48 BASIC" mode.

**Done when.** The three lines above, plus a line with `INK`/`PAPER`, a function call (`SIN`, `RND`) and a
string containing keyword-like text (`PRINT "TO BE"`), are entered correctly on 48K and run. Tests cover the
splitting, the mode handling and an end-to-end entry. The tool description, `docs/Mcp.md` and the Typing
notes are updated.

## Step 2: `list_basic` tool (read the program back as text)

A natural pair for step 1. The agent can check what is in memory without typing `LIST` and reading the screen.
Read lines from `PROG` (23635) up to `VARS` (23627). Detokenise keywords, skip the hidden 5-byte numbers
that follow `0x0E`, and render embedded colour codes in some readable form. The `zx-basic` branch's
`BasicMemoryReader` and `BasicTokenizer` already do most of this.

**Done when.** A program entered in step 1 lists back identically, and there are tests for keywords,
numbers, strings and `REM`.

## Step 3: report `load_file` errors instead of showing a dialog

`IEmulatorHost.LoadFileAsync` calls `MainViewModel.HandleLoadFileAsync`. If loading fails it shows an error
dialog, and the tool call waits until someone closes it. Add a way to return the error, for example an
optional `showErrors` flag or a separate method that throws, so the tool can report it with `McpException`.
User-initiated loads must keep the dialog.

## Step 4: breakpoint and stepping tools

Agents can hit breakpoints that the user set in the debugger, but they can't set breakpoints or step.

- Tools: `set_breakpoint` (same syntax as the debugger: `PC==32768`, `16384==32`), `clear_breakpoint`,
  `list_breakpoints`, `step` (one instruction), and possibly `run_until` (run frames until a breakpoint).
- `BreakpointHandler` and `BreakpointManager` live in `Spectron.Debugger` and are owned by `MainViewModel`.
  When a breakpoint is hit, `MainViewModel.OnBreakpointHit` opens the debugger window. Decide whether the
  agent shares the user's breakpoints (simplest, and visible in the debugger UI) and whether a hit during
  an agent call should open the window.
- Check how the debugger implements stepping (`DebuggerViewModel`) before writing `step`. Stepping inside a
  frame interacts with frame timing (`_isDebuggerResume` in `Emulator`).

## Step 5: `save_snapshot` tool

Save the current state through `SnapshotManager` (SNA, Z80, SZX or `.spectron`). Useful so an agent can
create a known starting point and return to it with `load_file`. Decide whether to restrict paths, for
example to an existing folder, and whether to overwrite existing files.

## Step 6: headless stdio mode (larger)

Run without a window, for CI or for agents that launch the emulator themselves, for example
`Spectron --headless --mcp-stdio`. `Spectron.Emulation` has no Avalonia dependency, so this needs a non-UI
`IEmulatorHost` that creates the emulator through `EmulatorFactory`, plus the SDK's stdio transport. Audio
should be off. Consider whether this is a separate small executable project.

## Step 7: smaller items

- **Optional auth token.** The HTTP server has no authentication. Any local process can control the emulator
  and load files. Option: a random token stored in preferences and required as `Authorization: Bearer`.
- **Shutdown overlap.** One MCP frame can still overlap `Emulator.Shutdown` of a machine being replaced.
  `Shutdown` is outside the frame lock on purpose, because it waits for the timer thread and holding the lock
  there could deadlock. Possible fix: stop the timer first, then take the lock for the rest of the shutdown.
  Check that the audio stop order still works.
- **Screenshot top row.** With `includeBorder: true`, the first frame-buffer line has a short white segment.
  The integration tests now crop it (`IntegrationTestBase` encodes from line 1 with `Height - 2`). Consider
  cropping the same way in the `screenshot` tool.
- **Server version.** `ServerInfo.Version` reports the assembly version (`0.0.0.0` in dev builds). Use the
  informational version, as `AppVersion` does.

---

## Suggested order

1. Step 1 `type_basic`, then step 2 `list_basic`. They are the biggest improvement for agents writing BASIC.
2. Step 3, load errors. Small, and removes a way for a tool call to hang.
3. Steps 4 and 5, debugging and snapshots, for machine code work.
4. Step 7 items as needed. Step 6 only if headless use becomes a real need.
