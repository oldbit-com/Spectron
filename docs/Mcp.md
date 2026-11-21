### AI Agents (MCP server)

> [!WARNING]
> This is an experimental feature. Tool names, parameters and behaviour may change between releases.

Spectron includes an optional [Model Context Protocol](https://modelcontextprotocol.io) (MCP) server.
It lets AI agents such as Claude Code, GitHub Copilot or Cursor see and control the running emulator.
An agent can type on the Spectrum keyboard, run frames, read the screen, inspect and change memory,
disassemble code, load files and use the built-in debugger, while you watch everything happen in the emulator window.

Typical uses:
- **Writing BASIC or machine code**: the agent writes a program, enters or pokes it into memory, runs it,
  reads the result from the screen and iterates until it works.
- **Exploring software**: load a game or demo and let the agent inspect memory and code, for example
  to find where the number of lives is stored or to explain what a routine does.
- **Investigating emulator behaviour**: reproduce a problem step by step with deterministic frame control.

---

### Enabling the server

The server is **off by default** and must be enabled explicitly:

- **Preferences → General → AI Agents**: tick *Enable MCP server* and optionally change the port (default `5480`).
  The server starts or stops as soon as preferences are saved.
- **Command line**: start Spectron with `--mcp-server` (uses the port from preferences) or `--mcp-port <port>`,
  which also enables the server. Command line options take priority over preferences.

```shell
dotnet run --project ./src/Spectron -- --mcp-server
```

When enabled, the server listens on:

```
http://localhost:5480/mcp
```

If the port is already in use, a notification is shown and the emulator keeps running without the server.

> [!IMPORTANT]
> The server only accepts connections from the local machine (`127.0.0.1`), but it has no authentication:
> any program running on your computer can control the emulator while the server is enabled,
> including loading files from disk. Only enable it when you need it.

---

### Connecting an AI agent

The server uses the **Streamable HTTP** transport. Spectron must be running with the server enabled
before the agent connects. Most MCP clients only need the URL above.

#### Claude Code

```shell
claude mcp add --transport http spectron http://localhost:5480/mcp
```

Add `--scope project` to store the configuration in the project's `.mcp.json` file, so it is shared with
everyone working in that folder. Use `/mcp` inside Claude Code to check the connection.

#### Other clients

Many clients use a JSON configuration file. The exact file name and layout depend on the client, so check
its documentation. Typical entries look like this:

```json
{
  "mcpServers": {
    "spectron": {
      "type": "http",
      "url": "http://localhost:5480/mcp"
    }
  }
}
```

VS Code (GitHub Copilot) uses `.vscode/mcp.json` with a `servers` key instead:

```json
{
  "servers": {
    "spectron": {
      "type": "http",
      "url": "http://localhost:5480/mcp"
    }
  }
}
```

Clients that only support local (stdio) servers can connect through a bridge such as
[mcp-remote](https://www.npmjs.com/package/mcp-remote), using `npx mcp-remote http://localhost:5480/mcp` as the command.

---

### How it works

Normally the emulator runs in real time, 50 frames per second. AI agents work in steps: they call a tool,
read the result, think, and call the next tool. To make results predictable, every tool works like this:

1. The emulator is paused for the duration of the call.
2. Tools that need time to pass (`run_frames`, `type_text`, `press_keys`) run emulation frames themselves,
   as fast as possible. 50 frames equal one second of Spectrum time.
3. The previous run state is restored afterwards. The exception is `run_frames`, which deliberately
   leaves the emulator paused so the agent can inspect the result. The agent calls `resume` to hand
   control back to you.

You stay in control the whole time:
- If you **resume emulation** (for example with `F2`) while the agent is running frames,
  the agent stops immediately and is told that you took over.
- If you **load a file or reset the machine**, the agent stops and is told that the machine was replaced.
- If a **debugger breakpoint** is hit, the agent stops and the debugger window opens as usual.
  The emulator is not resumed automatically.

Calls from the agent are executed one at a time.

---

### Available tools

| Tool               | Description                                                                                                   |
|--------------------|---------------------------------------------------------------------------------------------------------------|
| `get_status`       | Machine type, ROM, whether emulation is paused, program counter and tape state                                |
| `pause`            | Pauses emulation                                                                                              |
| `resume`           | Resumes real-time emulation                                                                                   |
| `reset`            | Soft reset, or hard reset (`hard: true`) which recreates the machine from the current preferences             |
| `load_file`        | Loads a file like *File → Open*: snapshots, tapes, disks, microdrives, RZX, POK and ZIP archives               |
| `run_frames`       | Runs `count` frames as fast as possible and leaves the emulator paused (max 15,000 per call)                  |
| `type_text`        | Types text on the Spectrum keyboard, one key press per character                                              |
| `press_keys`       | Presses named key combinations in turn, e.g. `["J", "SYM+P", "SYM+P", "ENTER"]`                               |
| `read_screen_text` | Reads the 32×24 character screen as text                                                                      |
| `screenshot`       | Captures the screen as a PNG image, scale 1–4, with or without the border                                     |
| `get_registers`    | Z80 registers, flags, interrupt mode and state                                                                |
| `read_memory`      | Hex dump with ASCII, up to 4,096 bytes                                                                        |
| `write_memory`     | Writes bytes to memory, like `POKE`, e.g. `"3E 01 C9"`                                                        |
| `disassemble`      | Disassembles Z80 code, starting at the program counter by default                                             |
| `debugger_command` | Executes a [debugger immediate window](Debugger.md) command, e.g. `HL = $4000`, `PEEK 23560`, `LIST`          |

Addresses can be written as decimal (`32768`) or hexadecimal (`$8000`, `0x8000`, `#8000`, `8000h`).

#### Reading the screen

`read_screen_text` compares every 8×8 character cell with the Spectrum ROM character set and returns
24 lines of text. Inverse video and the Spectrum block graphics are recognised.
Cells containing other graphics are shown as `▒`. Programs that use their own font can be read by passing
its address as `fontAddress` (96 characters of 8 bytes each, starting with space).

Reading the screen as text is quicker and more reliable for an agent than interpreting a screenshot,
so agents should prefer it and use `screenshot` when the graphics matter.

#### Typing

`type_text` supports letters (upper case uses CAPS SHIFT), digits, space, new line (`ENTER`),
backspace (`DELETE`) and the symbols printed in red on the 48K keyboard:
`! @ # $ % & ' ( ) _ < > ; " ^ - + = : £ ? / * , .`

`press_keys` accepts key names `A`–`Z`, `0`–`9`, `ENTER`, `SPACE`, `CAPS`, `SYM` and the shortcuts
`BREAK`, `DELETE`, `EDIT`, `CAPSLOCK`, `TRUEVIDEO`, `INVVIDEO`, `LEFT`, `DOWN`, `UP`, `RIGHT`, `GRAPHICS`
and `EXTEND`. Keys are combined with `+`, e.g. `SYM+P` or `CAPS+5`. A long `holdFrames` value keeps a key
pressed, which is useful in games (50 frames = 1 second).

> [!NOTE]
> 48K BASIC uses keyword entry: at the `K` cursor, a single key produces a whole keyword,
> for example `p` gives `PRINT`, `j` gives `LOAD` and `r` gives `RUN`. To enter `10 PRINT "HELLO"`,
> type `10p"HELLO"` followed by a new line. The 128K BASIC editor accepts keywords spelled out in full.

Some 48K keywords cannot be entered with `type_text` at all, because they need Symbol Shift or Extended mode.
The agent enters those with `press_keys` and uses `type_text` for the rest of the line:

- **Symbol Shift** keywords: `SYM+Y` AND, `SYM+U` OR, `SYM+F` TO, `SYM+D` STEP, `SYM+G` THEN, `SYM+I` AT,
  `SYM+S` NOT, `SYM+A` STOP, `SYM+Q` `<=`, `SYM+W` `<>`, `SYM+E` `>=`.
- **Extended mode** keywords printed above a key (in green), for example `["EXTEND", "H"]` gives SQR.
- **Extended mode** keywords printed below a key (in red), for example `["EXTEND", "SYM+H"]` gives CIRCLE,
  `["EXTEND", "SYM+X"]` gives INK and `["EXTEND", "SYM+C"]` gives PAPER.

For example, `10 CIRCLE 128,88,50` is entered as `type_text "10"`, `press_keys ["EXTEND", "SYM+H"]` and
`type_text "128,88,50\n"`. The full key map is included in the `press_keys` tool description, so agents can see it.

By default each key is held for 3 frames and released for 6. The ROM ignores a key pressed again
less than about 5 frames after it was released, so very small `releaseFrames` values can drop repeated keys.

---

### Example prompts

Once the agent is connected, ask for things in plain language, for example:

- *"Write a BASIC program in Spectron that draws a circle in the middle of the screen, run it and show me a screenshot."*
- *"Load ~/Games/manic.tap, wait until it has loaded and tell me what is on the screen."*
- *"Assemble a routine that fills the screen attributes with random colours, poke it at 32768, call it with
  RANDOMIZE USR 32768 and check the result."*
- *"The game is loaded. Find the memory location that holds the number of lives and suggest a POKE for infinite lives."*
- *"Disassemble the code at the current program counter and explain what it does."*

---

### Tips for working with agents

- **Give the agent context.** If you use an agent regularly with Spectron, add a few notes to its instructions file
  (for example `CLAUDE.md` for Claude Code, or `AGENTS.md`): which machine you work with, where your files are,
  which assembler to use, and the 48K keyword entry note above.
- **Let the agent build and test in a loop.** Agents work best when they can check their own results.
  A typical loop is: write the code, then build it with a cross-assembler such as sjasmplus or pasmo,
  `load_file` or `write_memory`, `run_frames`, then `read_screen_text`, `read_memory` or `screenshot`.
- **Combine with snapshots.** Save a snapshot yourself from the menu and ask the agent to `load_file` it
  before each experiment, so every attempt starts from the same state.
- **Watch and interrupt.** Everything the agent does is visible in the emulator window. Press `F2` at any time
  to take back control.

---

### Limitations

- Experimental: tool names and behaviour may change.
- There are no tools yet for setting breakpoints or stepping through code. Breakpoints set in the debugger still work.
- Tokenised BASIC cannot be entered directly into memory yet, so programs are typed on the keyboard.
- There is no tool for saving snapshots yet.
- If loading a file fails, the usual error dialog is shown and the tool call waits until the dialog is closed.
- Screenshots use the currently selected colour palette.
- The server has no authentication, see the note in [Enabling the server](#enabling-the-server).

---

### Troubleshooting

- **The agent cannot connect**: make sure Spectron is running, the server is enabled and the port in the
  client configuration matches the one in preferences. `curl -i http://localhost:5480/mcp` should get an HTTP
  response (even an error status means the server is listening); "connection refused" means it is not running.
- **"MCP server failed to start" notification**: another program is using the port. Choose a different
  port in preferences and update the client configuration.
- **"Emulator is not running"**: the emulator is being created or replaced, try again in a moment.
- **Typed text is incomplete or keywords are wrong**: see the [Typing](#typing) notes above.
