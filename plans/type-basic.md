# Step 1: `type_basic` implementation plan

Detailed plan for step 1 of `plans/mcp-next-steps.md`. Written 2026-11-25 after checking the approach
against the 48K ROM bytes and a throwaway probe test on the real emulator (deleted afterwards).

## Verdict on the agreed approach

Typing through the ROM editor is the right approach and works, but **not with fixed key timing alone**.
The approach needs three changes and two additions to be reliable:

| Original plan | Problem found | Change |
|---|---|---|
| Press keys with fixed hold/release frames, read the mode between presses | After ENTER the ROM syntax-checks, inserts the line and auto-lists the program. The probe measured 8 frames with one line, growing to 27 frames with ~12 lines. A key pressed during that time is decoded with a stale mode, and a second key overwrites the first in `LAST_K`. | Wait until the editor is **idle** before every key press (see "Ready detection"). |
| Read `FLAGS`/`MODE` before each keyword | `FLAGS` bit 3 is only updated when the edit line is redrawn. Reading it while the ROM is still busy gives the old mode. | Read it only once the editor is idle. Check the mode before **every** item, not only keywords. Letters typed in K mode silently become keywords. |
| Check the bottom screen lines for a `?` marker after ENTER | Works, but the screen is not reliable for success or failure. | Decide success from memory. An empty edit line (`E_LINE` holds `0D`) means the line was accepted. Use the screen only to quote the rejected line in the error message. |
| Copy keyword names from the `zx-basic` branch | Hand-copied tables can have mistakes. | **Derive the whole table from the 48K ROM**, which is already embedded. That gives the token names and the key for each token (see "Keyword table"). |
| — | **REM quirk (confirmed):** the ROM switches to K mode after `:` even inside a REM. `10 REM a: b` stores `b` as the `BORDER` token. | Type REM text without its colons, then insert the colons with LEFT/RIGHT (see "REM colons"). |
| — | **Caps lock (confirmed):** with caps lock on, lower-case letters in strings come out upper case. | Turn caps lock off for the call, then restore it. |

## ROM facts (verified from ROM bytes and the probe)

All addresses are the same in `48.rom`, `128-1.rom` (48 BASIC on 128K) and `TC2048.rom`. The relevant
routines are byte-identical in all three. `gw03`, `bsrom140`, `HTR`, `JGH077` and the 128 ROM 0 differ.

| Item | Address | Meaning |
|---|---|---|
| `FLAGS` bit 3 | 23611 | 1 = L mode, 0 = K mode. Valid when `MODE` = 0. `OUT-CURS` ($18E1) copies it from `FLAGS` bit 2 when the cursor is drawn. |
| `FLAGS` bit 5 | 23611 | A new key is waiting in `LAST_K` (set by the interrupt, cleared by `KEY-INPUT`). |
| `MODE` | 23617 | 0 = K/L/C, 1 = E, 2 = G. |
| `FLAGS2` bit 3 | 23658 | Caps lock. |
| `TV_FLAG` bit 3 | 23612 | The edit line needs redrawing. `WAIT-KEY` sets it, `ED-COPY` clears it. |
| `FLAGX` bit 5 | 23665 | INPUT mode (a running program is waiting for INPUT). |
| `ERR_SP` | 23613 | Inside the editor, the word at `(ERR_SP)` is `$107F` (`ED-ERROR`), pushed by `EDITOR` at $0F2C. |
| `E_LINE`, `K_CUR`, `WORKSP` | 23641, 23643, 23649 | Edit line start, cursor, end. An empty edit line is `0D 80`. |
| `DF_SZ` | 23659 | Number of rows in the lower screen. Used to quote the rejected line. |

- **Keys are decoded in the interrupt** (`K-NEW` at $02F1 loads `C=MODE`, `D=FLAGS`, then calls `K-DECODE`
  at $0333). The mode must be correct before the key goes down.
- **K/L rule** (`OUT-LINE2` $187D, `OUT-CHAR` $1937): K at the start of the line. Digits and spaces do not
  change the mode. `:` outside quotes and `THEN` switch to K. Every other character, including tokens,
  switches to L. Quotes toggle a flag in `FLAGS2` bit 2, which is reset at the start of the line. REM is not
  special-cased.
- **Decoding** (`K-DECODE`): a letter in K mode gives token `$A5 + (letter - 'A')`, whatever the shift state.
  Symbol Shift + letter uses the `SYM-CODES` table in K and L mode. E mode uses `E-UNSHIFT`, or `EXT-SHIFT`
  with Symbol Shift. E mode with Symbol Shift + digit uses `E-DIGITS`.
- **Syntax error:** the edit line stays and the ROM draws `?` before the error (probe screen:
  `30 PRINT ?)`). `X_PTR` is already cleared by the time the editor is idle again, so it can't be used for the
  error position.
- **KSTATE:** a released key slot is freed only after 5 interrupts, so keep `releaseFrames` at 6 or more.
- **128K:** the 128 menu and 128 BASIC never reach the 48K `KEY-INPUT` with `(ERR_SP) = $107F`. 128K in
  48 BASIC mode does, and was confirmed working: `EXTEND` sets `MODE`=1, then `SYM+H` stores `$D8` CIRCLE.
  `Memory.RomBank` is **not** a usable 48 BASIC indicator, because 128 BASIC pages ROM 1 for short periods.

## Ready detection

Subscribe to `emulator.Cpu.BeforeInstruction` for the duration of the tool call. The emulator and
`BreakpointHandler` already use this event. Set `Ready = true` when all of these hold:

- `pc == $10A8` (`KEY-INPUT` entry) and `!Memory.IsShadowRomActive`
- the bytes at `$10A8` are `FD CB 02 5E` (guards against ROM 0 and incompatible ROMs)
- `TV_FLAG` bit 3 = 0 (edit line fully redrawn, so the mode in `FLAGS` bit 3 is current)
- `FLAGS` bit 5 = 0 (no key waiting)
- the word at `(ERR_SP)` = `$107F` (inside the editor, not `scroll?` or a running program)

`WaitUntilReady(maxFrames)` clears the flag and runs one frame at a time through `EmulatorController.RunFrames`,
so `StopReason` handling stays in one place, until the flag is set. Once set, the ROM stays idle for the rest
of the frame because no key changes between frames. The probe saw "ready" 1 frame after each key release,
8–27 frames after ENTER, 87 frames after a 48K boot, and never while `scroll?` was showing or 128 BASIC was
active. Those are the cases that should fail.

## Components

All new code goes in `src/Spectron.Mcp/Basic/`.

### `BasicKeywords` (ROM-derived table)

Built lazily from `RomReader.ReadRom(RomType.Original48)`, the same way `ScreenTextReader` loads the font.

- Names: token table at `$0095`, with bit 7 marking the last character. Entry 0 is `?`, entry *n* is token
  `$A4 + n`. That gives tokens `$A5` RND to `$FF` COPY: 91 keywords, including `GO TO`, `GO SUB`, `DEF FN`,
  `OPEN #`, `CLOSE #`, `<=`, `>=` and `<>`.
- Keys, by inverting the decode tables:
  - K-mode letter (`$E6`–`$FF`, 26 tokens): the letter alone. **Needs K mode.**
  - `SYM-CODES` `$026A` (A–Z): `SYM+letter`, 11 tokens (STOP, STEP, `>=`, TO, THEN, AT, `<=`, NOT, OR, `<>`, AND).
  - `E-UNSHIFT` `$022C` (A–Z): `EXTEND`, then the letter. 26 tokens.
  - `EXT-SHIFT` `$0246` (A–Z): `EXTEND`, then `SYM+letter`. 18 tokens.
  - `E-DIGITS` `$0284` (0–9): `EXTEND`, then `SYM+digit`. 10 tokens (FORMAT, DEF FN, FN, LINE, OPEN #, CLOSE #, MOVE, ERASE, POINT, CAT).
- The same `EXT-SHIFT` entries below `$80` give the characters `KeyMapper` can't type: `~ \ { } | ] [ ©`.
  Allow them in strings and REM text.

This replaces the hand-written key list. Step 2 (`list_basic`) can reuse the token names for detokenising.

### `BasicLineParser` (pure, no emulator)

Turns listing text into `BasicLine(int Number, IReadOnlyList<BasicItem> Items)`. Each `BasicItem` is a
`Keyword(byte Token)` or a `Character(char C)`. The parser also produces the expected edit-line bytes for each line.

- Line endings `\n` or `\r\n`. Skip blank lines. Each line needs a line number from 1 to 9999. Reject 0,
  because line 0 would run as a direct command, and reject lines without a number.
- Outside strings and REM:
  - Match keywords **longest first, case-insensitively**. A space inside a keyword is optional
    (`GO TO`/`GOTO`, `GO SUB`/`GOSUB`, `DEF FN`, `OPEN #`, `CLOSE #`).
  - **Word boundaries:** the character before an alphabetic keyword must not be a letter or digit. The
    position right after the line number counts as a boundary. The character after it must not be a letter,
    digit or `$`. `<=`, `>=` and `<>` need no boundary. This handles `TOTAL`, `INT` vs `IN` vs `INK`/`INKEY$`/`INPUT`,
    `ATTR` vs `AT`, and `CAT`.
  - **Drop all whitespace.** The ROM adds its own display spaces around tokens, and spaces have no meaning in
    48K BASIC outside strings. This keeps the stored line identical to one typed by hand.
- Inside `"..."` every character is literal. A doubled `""` works because each quote toggles the state.
- After the `REM` token, the rest of the line is literal. Drop one space right after `REM`, because the ROM
  shows `REM` followed by a space.
- Characters must be typeable with `KeyMapper` or the `EXT-SHIFT` extras. Otherwise report the line and column.
- Out of scope, with a clear error: block graphics, UDGs and embedded colour control codes.

### `BasicEditor` (driver)

Uses `EmulatorController` frame primitives inside one `ExecuteAsync` call (`leavePaused: false`, like `type_text`).

**Before typing**

1. `WaitUntilReady(100)`. On timeout, report: *"The 48K BASIC editor is not waiting for input. type_basic needs
   16K/48K/TC2048, or a 128K in 48 BASIC mode, with no program running. In 128 BASIC, use type_text: it
   accepts keywords spelled out."*
2. If `FLAGX` bit 5 is set, report that a program is waiting for INPUT.
3. If `MODE` is not 0, press `EXTEND` (E mode) or `GRAPHICS` (G mode) to leave it.
4. If caps lock is on, turn it off and restore it at the end.
5. If the edit line is not empty, clear it: RIGHT until `K_CUR` reaches the `0D`, then DELETE until empty.
   Mention this in the result.

**Per line**

1. For each item, read `FLAGS` bit 3 and `MODE` while the editor is idle, then:
   - K-mode keyword: in K mode, press the letter. In L mode, report *"line N: GO TO can only start a statement
     (after the line number, ':' or THEN)"*.
   - Symbol Shift keyword: `SYM+key` in either mode.
   - E-mode keyword: `EXTEND`, check `MODE` = 1, then the key.
   - Letter in K mode, outside REM: report *"line N: a statement must start with a keyword, found 'x'. 48K BASIC
     needs LET for assignments."*
   - Other characters: `KeyMapper` keys, or `EXTEND` + `SYM+key` for the extras.
   - After each press: hold 3 frames, release 6 frames (the defaults proven in practice), then `WaitUntilReady`.
2. **REM colons:** type the REM text without its `:` characters, so the whole text is typed in L mode. Then,
   from the last colon to the first, press LEFT to the colon's position and type `:`. Press RIGHT back to the
   end. Cursor keys work in any mode and `:` comes from `SYM+Z` in any mode.
3. Before ENTER, compare the bytes from `E_LINE` to the `0D` with the expected bytes: line-number digits,
   tokens and characters. If they differ, clear the line and report both versions. This catches every dropped
   key or wrong-mode decode.
4. Press ENTER, then `WaitUntilReady(250)`.
5. If the edit line is empty, the line was accepted. Otherwise the ROM rejected it: read the `DF_SZ` bottom
   rows with `ScreenTextReader` and trim the trailing cursor letter, giving for example `30 PRINT ?)`. Clear
   the line and stop.

**Limits and stopping**

- Estimate the cost up front: about 10 frames per key and 30 per line. Reject listings over
  `MaxFramesPerCall` and ask for several calls. That is roughly 40 typical lines per call.
- Any `StopReason` (breakpoint, user resumed, machine replaced) stops typing. The result lists the lines
  already entered.

### Tool `type_basic`

`type_basic(string program)` in `EmulatorTools`. It returns `Entered 4 line(s): 10, 20, 30, 40.` and mentions
a cleared edit line if there was one. Failures are `McpException`s that name the lines already entered and
the failing line with its reason. It does not run the program; RUN is `type_text "r\n"`, because `r` at the K
cursor is RUN.

The description should cover:

- 48K BASIC only.
- Keywords spelled out as LIST shows them, separated by spaces.
- Keywords are matched case-insensitively, so variables can't be named `to`, `at`, `pi` and so on.
- Strings and REM text are literal.
- Each line needs a number from 1 to 9999. A number alone deletes that line.
- What is out of scope.

## Tests

- **`BasicKeywordsTests`:** 91 tokens with exactly one key sequence each, plus spot checks (CIRCLE =
  `EXTEND, SYM+H`; GO TO = K-mode `G`; TO = `SYM+F`; DEF FN = `EXTEND, SYM+1`; INKEY$ = `EXTEND, N`) and the
  names that contain spaces.
- **`BasicLineParserTests`:**
  - longest-first matching and boundaries (`INKEY$`/`INK`/`INPUT`/`INT`/`IN`, `ATTR`/`AT`, `TOTAL`, `CAT`, `GOTO`)
  - strings (`PRINT "TO BE"`, `""`) and REM (rest literal, colons kept)
  - `<=` as a token vs `<` `=` inside a string
  - whitespace dropping
  - line-number errors (missing, 0, 10000) and characters that can't be typed
- **`BasicEditorTests`** (end-to-end, `TestEmulatorHost`):
  - The "Done when" lines. Line 30 needs `a` and `b` set before RUN, for example
    `5 LET a=1: LET b=2`. Also cover INK/PAPER, `SIN`/`RND`, and `PRINT "TO BE"`. Assert the tokenised bytes
    in the program area (`PROG`..`VARS`), then RUN and check the screen.
  - REM with a colon stores the letters, not BORDER.
  - With caps lock on, a lower-case string stays lower case and caps lock is restored.
  - A syntax error reports the line with `?` and leaves the edit line empty.
  - A command keyword in L mode and a missing LET give clear errors.
  - A partly typed edit line is cleared first.
  - On 128K, 128 BASIC gives the clear error, and 48 BASIC (menu: DOWN×3, ENTER) works.

## Docs

- Tool table and a short `type_basic` paragraph in the Typing section of `docs/Mcp.md`, plus the README
  tool list.
- Mention `type_basic` in the server instructions in `McpServer.cs`.
- In the `type_text` description, point to `type_basic` for 48K program entry.

## Open decisions (defaults chosen; change if you disagree)

1. **128 BASIC.** The default is 48 BASIC only, with an error that points to `type_text`. Supporting 128 BASIC
   would need a second ROM 0 ready detector and different error detection; leave it for later if needed.
2. **Leftover edit line.** The default is to clear it and say so. The alternative is to fail and let the agent
   clear it.
3. **Strict word boundaries.** The default is that `1TO10` is not split, so the ROM reports a syntax error.
   A preceding digit could count as a boundary, but `a1TO` is then ambiguous.
