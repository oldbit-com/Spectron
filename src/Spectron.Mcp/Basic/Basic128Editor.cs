using System.Text;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices.Keyboard;
using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Spectron.Mcp.Input;
using static OldBit.Spectron.Emulation.Devices.Keyboard.SpectrumKey;

namespace OldBit.Spectron.Mcp.Basic;

/// <summary>
/// Types BASIC lines into the 128 BASIC full-screen editor, the same way a user would, so the ROM checks the syntax.
/// Keywords are spelled out. Before every key press it waits until the editor is idle. The cursor position, the
/// screen rows and whether the line at the cursor was changed are read from the editor workspace in RAM bank 7.
/// </summary>
/// <remarks>
/// The editor works on the whole screen: the cursor keys move into other program lines, and DELETE at the start
/// of a row joins it to the line above. So every line is typed on an empty row, and a rejected line is cleared
/// only while the cursor is known to be inside it.
/// </remarks>
internal sealed class Basic128Editor : IDisposable
{
    internal const int HoldFrames = 3;
    internal const int ReleaseFrames = 6;

    private const int StartTimeoutFrames = 100;
    private const int KeyTimeoutFrames = 50;
    private const int EnterTimeoutFrames = 250;
    private const int FramesPerLine = 40;

    // System variables
    private const Word Flags = 23611;     // bit 5: new key in LAST_K
    private const Word Flags2 = 23658;    // bit 3: caps lock
    private const Word FlagX = 23665;     // bit 5: INPUT mode
    private const Word XPtr = 23647;      // where the syntax check stopped
    private const Word WorkSp = 23649;    // the 128 editor tokenizes the line here before checking it

    private const byte EndOfLine = 0x0D;
    private const int MaxTokenizedLength = 4096;

    // ROM 0: the editor and the menus wait for a key in a loop at $3683: LD HL,FLAGS / BIT 5,(HL) / JR Z,$3683
    private const Word KeyWaitRoutine = 0x3680;
    private const Word KeyWaitLoop = 0x3683;
    private static readonly byte[] KeyWaitCode = [0x21, 0x3B, 0x5C, 0xCB, 0x6E, 0x28, 0xFC];

    // Editor workspace in RAM bank 7, as offsets in the bank
    private const int EditorFlags = 0xEC0D - 0xC000;   // bit 1: a menu is shown, bit 3: the line at the cursor was changed
    private const int EditorMode = 0xEC0E - 0xC000;    // 0: editing or a menu, 4: calculator
    private const int RowBuffer = 0xEC16 - 0xC000;     // the rows of the editing area
    private const int CursorRow = 0xF6EE - 0xC000;
    private const int CursorColumn = 0xF6EF - 0xC000;

    private const int EditorBank = 7;
    private const int Rows = 21;
    private const int Columns = 32;
    private const int RowSize = 35;                    // 32 characters, flags, line number (high byte first)

    private const byte MenuFlag = 0x02;
    private const byte ChangedFlag = 0x08;
    private const byte FirstRowFlag = 0x01;            // row flags
    private const byte LastRowFlag = 0x08;
    private const byte StoredLineFlag = 0x10;

    private const byte PagingLockBit = 0x20;

    private static readonly SpectrumKey[] ExtendKeys = [CapsShift, SymbolShift];
    private static readonly SpectrumKey[] CapsLockKeys = [CapsShift, D2];
    private static readonly SpectrumKey[] RightKeys = [CapsShift, D8];
    private static readonly SpectrumKey[] DeleteKeys = [CapsShift, D0];
    private static readonly SpectrumKey[] EnterKeys = [Enter];

    internal const string NotReadyMessage =
        "The 128 BASIC editor is not waiting for input. type_basic needs the 128 BASIC editor with no program running, " +
        "or the 48K BASIC editor.";

    private readonly EmulatorController _controller;
    private readonly Emulator _emulator;
    private readonly Memory128K _memory;

    private bool _isReady;
    private bool _restoreCapsLock;
    private bool _isEditorConfirmed;

    internal Basic128Editor(EmulatorController controller, Emulator emulator)
    {
        _controller = controller;
        _emulator = emulator;
        _memory = (Memory128K)emulator.Memory;

        _emulator.Cpu.BeforeInstruction += OnBeforeInstruction;
    }

    /// <summary>
    /// Returns true when the machine runs the 128 ROM with paging enabled, i.e. 128 BASIC rather than 48 BASIC mode.
    /// </summary>
    internal static bool IsActive(Emulator emulator)
    {
        if (emulator.Memory is not Memory128K memory || (memory.LastPagingModeValue & PagingLockBit) != 0)
        {
            return false;
        }

        for (var i = 0; i < KeyWaitCode.Length; i++)
        {
            if (memory.RomBank0.Read((Word)(KeyWaitRoutine + i)) != KeyWaitCode[i])
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Estimates the number of frames needed to enter the lines.
    /// </summary>
    internal static long EstimateFrames(IEnumerable<BasicLine> lines)
    {
        long keys = 0;
        long count = 0;

        foreach (var line in lines)
        {
            count++;
            keys += FormatForTyping(line).Sum(c => BasicKeywords.ExtendedCharacters.ContainsKey(c) ? 2 : 1) + 1;
        }

        return keys * (HoldFrames + ReleaseFrames + 1) + count * FramesPerLine + StartTimeoutFrames;
    }

    /// <summary>
    /// The text typed for a line: keywords spelled out in lower case, separated from names and numbers by spaces,
    /// which the editor removes when it stores the line.
    /// </summary>
    internal static string FormatForTyping(BasicLine line)
    {
        var text = new StringBuilder(line.Number.ToString());

        for (var i = 0; i < line.Items.Count; i++)
        {
            switch (line.Items[i])
            {
                case KeywordItem { Keyword.Name: var name }:
                    if (char.IsAsciiLetter(name[0]) && (char.IsAsciiLetterOrDigit(text[^1]) || text[^1] is '$' or '.' or '"' or ')'))
                    {
                        text.Append(' ');
                    }

                    text.Append(name.ToLowerInvariant());

                    var isLast = i == line.Items.Count - 1 && string.IsNullOrEmpty(line.Remark);

                    if (!isLast && (char.IsAsciiLetter(name[^1]) || name[^1] == '$'))
                    {
                        text.Append(' ');
                    }

                    break;

                case CharacterItem { Character: var c }:
                    text.Append(c);
                    break;
            }
        }

        // The editor removes the first space after REM, as LIST adds it back
        if (!string.IsNullOrEmpty(line.Remark) && text[^1] != ' ')
        {
            text.Append(' ');
        }

        return text.Append(line.Remark).ToString();
    }

    /// <summary>
    /// Types the lines one by one. Stops at the first line the ROM rejects.
    /// </summary>
    internal BasicEntryResult EnterLines(IReadOnlyList<BasicLine> lines)
    {
        var enteredLines = new List<int>();
        var clearedCharacters = 0;

        try
        {
            clearedCharacters = Prepare();

            foreach (var line in lines)
            {
                TypeLine(line);
                enteredLines.Add(line.Number);
            }

            RestoreCapsLock();

            return new BasicEntryResult(enteredLines, clearedCharacters, null, StopReason.None);
        }
        catch (EntryStoppedException ex)
        {
            return new BasicEntryResult(enteredLines, clearedCharacters, null, ex.StopReason);
        }
        catch (EntryFailedException ex)
        {
            // Only touch the keyboard when the editor was confirmed to be waiting for input,
            // otherwise the keys could reach a running program
            if (_isEditorConfirmed)
            {
                try
                {
                    RestoreCapsLock();
                }
                catch (EntryException)
                {
                    // Best effort, the original error is more useful
                }
            }

            return new BasicEntryResult(enteredLines, clearedCharacters, ex.Message, StopReason.None);
        }
    }

    public void Dispose() => _emulator.Cpu.BeforeInstruction -= OnBeforeInstruction;

    private int Prepare()
    {
        if (!WaitUntilReady(StartTimeoutFrames))
        {
            throw new EntryFailedException(NotReadyMessage);
        }

        if ((ReadWorkspace(EditorFlags) & MenuFlag) != 0)
        {
            throw new EntryFailedException("A 128 menu is shown. Choose 128 BASIC (or Exit in the edit menu) first.");
        }

        if (ReadWorkspace(EditorMode) != 0)
        {
            throw new EntryFailedException("The 128 editor is not in 128 BASIC mode (e.g. the calculator is running). " +
                                           "Choose 128 BASIC in the main menu first.");
        }

        if ((Peek(FlagX) & 0x20) != 0)
        {
            throw new EntryFailedException("A program is waiting for INPUT. Finish or BREAK it first.");
        }

        _isEditorConfirmed = true;

        if ((Peek(Flags2) & 0x08) != 0)
        {
            Press(CapsLockKeys);
            _restoreCapsLock = true;
        }

        return MoveToEmptyRow();
    }

    private void TypeLine(BasicLine line)
    {
        var text = FormatForTyping(line);

        foreach (var c in text)
        {
            TypeCharacter(c);
        }

        VerifyTypedLine(line, text);

        Press(EnterKeys, EnterTimeoutFrames);

        var row = ReadLineAtCursor();

        if ((ReadWorkspace(EditorFlags) & ChangedFlag) != 0 && !row.IsStored && row.Text.Count > 0)
        {
            // The editor keeps a rejected line. Its cursor does not mark the error, X_PTR in the tokenized copy does.
            var shown = FormatRejectedLine(line.Number) ?? FormatText(row.Text);
            ClearLine();

            throw new EntryFailedException($"Line {line.Number} was rejected by the ROM (syntax error, '?' marks the position): {shown}");
        }

        VerifyStoredLine(line);
        MoveToEmptyRow();
    }

    private void TypeCharacter(char c)
    {
        if (BasicKeywords.ExtendedCharacters.TryGetValue(c, out var key))
        {
            Press(ExtendKeys);
            Press([SymbolShift, key]);
        }
        else
        {
            Press(KeyMapper.FromChar(c)!);
        }
    }

    /// <summary>
    /// Checks the row before ENTER, so a dropped key cannot store a different line, e.g. 2 instead of 20.
    /// </summary>
    private void VerifyTypedLine(BasicLine line, string text)
    {
        var row = ReadLineAtCursor();
        var expected = text.Select(BasicKeywords.ToCode).ToList();

        if (!row.IsStored && row.Text.SequenceEqual(expected))
        {
            return;
        }

        var typed = FormatText(row.Text);

        if (!row.IsStored)
        {
            ClearLine();
        }

        throw new EntryFailedException($"Line {line.Number} was not typed correctly. Expected: {text}, typed: {typed}");
    }

    private void VerifyStoredLine(BasicLine line)
    {
        var stored = BasicLister.ReadLineBytes(_memory, line.Number);

        if (line.Items.Count == 0 && line.Remark == null)
        {
            if (stored != null)
            {
                throw new EntryFailedException($"Line {line.Number} was not deleted.");
            }

            return;
        }

        if (stored == null)
        {
            throw new EntryFailedException($"Line {line.Number} was accepted by the editor but is not in the program.");
        }

        var expected = line.ExpectedBytes[line.Number.ToString().Length..];

        if (BasicLister.StripHiddenNumbers(stored).SequenceEqual(expected))
        {
            return;
        }

        throw new EntryFailedException($"Line {line.Number} was stored differently from the listing, e.g. a name was " +
                                       $"read as a keyword. Stored: {line.Number}{BasicLister.FormatLine(stored)}, " +
                                       $"expected: {line.Number}{BasicLister.FormatLine(expected)}");
    }

    /// <summary>
    /// Makes sure the cursor is at the start of an empty row, so the next line is typed as a new line.
    /// Clears text that is not a program line, e.g. a line rejected earlier or a key typed after a report.
    /// </summary>
    /// <returns>The number of characters cleared.</returns>
    private int MoveToEmptyRow()
    {
        var row = ReadLineAtCursor();

        if (row.IsStored)
        {
            if ((ReadWorkspace(EditorFlags) & ChangedFlag) != 0)
            {
                throw new EntryFailedException($"Line {row.Number} is being edited at the cursor. Press ENTER to keep " +
                                               "the changes, or move off the line, then try again.");
            }

            // ENTER on an unchanged program line opens an empty row below it
            Press(EnterKeys, EnterTimeoutFrames);
            row = ReadLineAtCursor();

            if (row.IsStored || row.Text.Count > 0)
            {
                throw new EntryFailedException("Could not open an empty line in the 128 editor.");
            }
        }

        var cleared = row.Text.Count > 0 ? ClearLine() : 0;

        if (ReadWorkspace(CursorColumn) != 0)
        {
            throw new EntryFailedException("The cursor is not at the start of an empty line in the 128 editor.");
        }

        return cleared;
    }

    /// <summary>
    /// Deletes the text of the line at the cursor, which is not a program line. DELETE removes the character
    /// before the cursor; at the start of the line RIGHT moves past the first character, so it can be deleted.
    /// </summary>
    private int ClearLine()
    {
        var length = ReadLineAtCursor().Text.Count;

        for (var i = 0; i < 2 * length + 2; i++)
        {
            var row = ReadLineAtCursor();

            if (row.IsStored)
            {
                throw new EntryFailedException("The cursor left the line while clearing it.");
            }

            if (row.Text.Count == 0)
            {
                return length;
            }

            if (row.CursorOffset == 0)
            {
                Press(RightKeys);
            }

            Press(DeleteKeys);
        }

        throw new EntryFailedException("Could not clear the line at the cursor in the 128 editor.");
    }

    private void RestoreCapsLock()
    {
        if (!_restoreCapsLock)
        {
            return;
        }

        _restoreCapsLock = false;
        Press(CapsLockKeys);
    }

    /// <summary>
    /// Presses and releases the keys, then waits until the editor has processed them and is waiting for the next key.
    /// </summary>
    private void Press(SpectrumKey[] keys, int timeoutFrames = KeyTimeoutFrames)
    {
        var result = _controller.PressKeys(_emulator, [keys], HoldFrames, ReleaseFrames);

        ThrowIfStopped(result.StopReason);

        if (!WaitUntilReady(timeoutFrames))
        {
            throw new EntryFailedException("The 128 BASIC editor stopped responding to key presses.");
        }
    }

    private bool WaitUntilReady(int maxFrames)
    {
        _isReady = false;

        for (var i = 0; i < maxFrames; i++)
        {
            ThrowIfStopped(_controller.RunFrames(_emulator, 1).StopReason);

            if (_isReady)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Detects that the editor is idle: the ROM 0 key wait loop runs with no key waiting.
    /// </summary>
    private void OnBeforeInstruction(Word pc)
    {
        if (pc != KeyWaitLoop || _isReady || _memory.RomBank != RomBank.Bank0 || _memory.IsShadowRomActive)
        {
            return;
        }

        _isReady = (Peek(Flags) & 0x20) == 0;
    }

    /// <summary>
    /// Reads the line under the cursor from the row buffer: the rows from the one marked as first to the one marked
    /// as last. Empty cells (e.g. the indent of a continuation row) are 0, typed spaces are 32.
    /// </summary>
    private EditorLine ReadLineAtCursor()
    {
        var cursorRow = ReadWorkspace(CursorRow);
        var cursorColumn = ReadWorkspace(CursorColumn);

        if (cursorRow >= Rows || cursorColumn >= Columns)
        {
            throw new EntryFailedException($"Unexpected 128 editor cursor position, row {cursorRow}, column {cursorColumn}.");
        }

        var first = (int)cursorRow;

        while (first > 0 && (ReadRowFlags(first) & FirstRowFlag) == 0)
        {
            first--;
        }

        var last = (int)cursorRow;

        while (last < Rows - 1 && (ReadRowFlags(last) & LastRowFlag) == 0)
        {
            last++;
        }

        var text = new List<byte>();
        var cursorOffset = -1;

        for (var row = first; row <= last; row++)
        {
            for (var column = 0; column < Columns; column++)
            {
                if (row == cursorRow && column == cursorColumn)
                {
                    cursorOffset = text.Count;
                }

                var value = ReadWorkspace(RowBuffer + row * RowSize + column);

                if (value != 0)
                {
                    text.Add(value);
                }
            }
        }

        var flags = ReadRowFlags(first);
        var number = ReadWorkspace(RowBuffer + first * RowSize + Columns + 1) << 8 |
                     ReadWorkspace(RowBuffer + first * RowSize + Columns + 2);

        return new EditorLine(text, cursorOffset, (flags & StoredLineFlag) != 0, number);
    }

    private byte ReadRowFlags(int row) => ReadWorkspace(RowBuffer + row * RowSize + Columns);

    /// <summary>
    /// Renders the line the syntax check rejected, with '?' at the error. The editor tokenizes the line (without its
    /// number) at WORKSP and the syntax check leaves X_PTR at the error. Returns null when they do not describe it.
    /// </summary>
    private string? FormatRejectedLine(int number)
    {
        var start = ReadWord(WorkSp);
        var error = ReadWord(XPtr);

        if (start < 0x5C00 || error < start)
        {
            return null;
        }

        var data = new List<byte>();

        for (var address = start; data.Count < MaxTokenizedLength; address++)
        {
            var value = Peek(address);
            data.Add(value);

            if (value == EndOfLine)
            {
                break;
            }
        }

        var offset = error - start;

        if (data[^1] != EndOfLine || offset >= data.Count)
        {
            return null;
        }

        var bytes = data.ToArray();

        return $"{number}{BasicLister.FormatLine(bytes.AsSpan(0, offset))}?{BasicLister.FormatLine(bytes.AsSpan(offset))}";
    }

    private static string FormatText(IEnumerable<byte> text) => string.Concat(text.Select(BasicKeywords.ToChar));

    private byte ReadWorkspace(int offset) => _memory.Banks[EditorBank][offset];

    private byte Peek(Word address) => _memory.Read(address);

    private Word ReadWord(Word address) => (Word)(Peek(address) | Peek((Word)(address + 1)) << 8);

    private static void ThrowIfStopped(StopReason stopReason)
    {
        if (stopReason != StopReason.None)
        {
            throw new EntryStoppedException(stopReason);
        }
    }

    private sealed record EditorLine(List<byte> Text, int CursorOffset, bool IsStored, int Number);

    private abstract class EntryException(string message) : Exception(message);

    private sealed class EntryFailedException(string message) : EntryException(message);

    private sealed class EntryStoppedException(StopReason stopReason) : EntryException(stopReason.ToString())
    {
        internal StopReason StopReason { get; } = stopReason;
    }
}
