using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices.Keyboard;
using OldBit.Spectron.Mcp.Screen;
using static OldBit.Spectron.Emulation.Devices.Keyboard.SpectrumKey;

namespace OldBit.Spectron.Mcp.Basic;

/// <summary>
/// The outcome of entering a BASIC listing.
/// </summary>
/// <param name="EnteredLines">Line numbers accepted by the ROM, in the order they were entered.</param>
/// <param name="ClearedCharacters">Number of characters that were already in the edit line and had to be removed.</param>
/// <param name="Error">Why entry failed, or null.</param>
/// <param name="StopReason">Why entry was interrupted, e.g. the user resumed emulation.</param>
internal sealed record BasicEntryResult(
    IReadOnlyList<int> EnteredLines,
    int ClearedCharacters,
    string? Error,
    StopReason StopReason);

/// <summary>
/// Types BASIC lines into the 48K ROM editor, the same way a user would, so the ROM checks the syntax.
/// Before every key press it waits until the editor is idle, then reads the cursor mode (K, L or E) from the
/// system variables and picks the keys accordingly. Works with the 48K ROM and compatible ROMs
/// (16K, TC2048, and 128K in 48 BASIC mode).
/// </summary>
internal sealed class BasicEditor : IDisposable
{
    internal const int HoldFrames = 3;
    internal const int ReleaseFrames = 6;

    private const int StartTimeoutFrames = 100;
    private const int KeyTimeoutFrames = 50;
    private const int EnterTimeoutFrames = 250;
    private const int FramesPerLine = 30;

    // System variables
    private const Word Flags = 23611;     // bit 3: L mode (1) or K mode (0), bit 5: new key in LAST_K
    private const Word TvFlag = 23612;    // bit 3: edit line needs redrawing
    private const Word ErrSp = 23613;     // in the editor, points to the ED-ERROR return address
    private const Word Mode = 23617;      // 0: K/L/C, 1: E, 2: G
    private const Word ELine = 23641;     // start of the edit line
    private const Word KCur = 23643;      // cursor address
    private const Word Flags2 = 23658;    // bit 3: caps lock
    private const Word DfSz = 23659;      // number of lines in the lower screen
    private const Word FlagX = 23665;     // bit 5: INPUT mode

    // 48K ROM addresses
    private const Word KeyInput = 0x10A8;
    private const Word EdError = 0x107F;
    private static readonly byte[] KeyInputCode = [0xFD, 0xCB, 0x02, 0x5E];

    private const byte EndOfLine = 0x0D;
    private const int MaxEditLineLength = 4096;

    private static readonly SpectrumKey[] ExtendKeys = [CapsShift, SymbolShift];
    private static readonly SpectrumKey[] GraphicsKeys = [CapsShift, D9];
    private static readonly SpectrumKey[] CapsLockKeys = [CapsShift, D2];
    private static readonly SpectrumKey[] LeftKeys = [CapsShift, D5];
    private static readonly SpectrumKey[] RightKeys = [CapsShift, D8];
    private static readonly SpectrumKey[] DeleteKeys = [CapsShift, D0];
    private static readonly SpectrumKey[] EnterKeys = [Enter];

    internal const string NotReadyMessage =
        "The 48K BASIC editor is not waiting for input. type_basic needs the K cursor showing with no program running " +
        "(16K/48K/TC2048, or 128K in 48 BASIC mode), or the 128 BASIC editor.";

    private readonly EmulatorController _controller;
    private readonly Emulator _emulator;

    private bool _isReady;
    private bool _restoreCapsLock;
    private bool _isEditorConfirmed;

    internal BasicEditor(EmulatorController controller, Emulator emulator)
    {
        _controller = controller;
        _emulator = emulator;

        _emulator.Cpu.BeforeInstruction += OnBeforeInstruction;
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
            keys += line.Number.ToString().Length + 1;
            keys += line.Items.Sum(item => item is KeywordItem { Keyword.IsExtended: true } || IsExtendedCharacter(item) ? 2 : 1);

            if (line.Remark != null)
            {
                // Each character, plus moving back over the text to insert the colons
                keys += line.Remark.Length * 3 + line.Remark.Count(c => c == ':' || BasicKeywords.ExtendedCharacters.ContainsKey(c));
            }
        }

        return keys * (HoldFrames + ReleaseFrames + 1) + count * FramesPerLine + StartTimeoutFrames;
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
                    // Leave the editor clean, e.g. after a listing error in the middle of a line
                    ClearEditLine();
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

        if ((Peek(FlagX) & 0x20) != 0)
        {
            throw new EntryFailedException("A program is waiting for INPUT. Finish or BREAK it first.");
        }

        _isEditorConfirmed = true;

        switch (Peek(Mode))
        {
            case 1:
                Press(ExtendKeys);
                break;

            case 2:
                Press(GraphicsKeys);
                break;
        }

        if (Peek(Mode) != 0)
        {
            throw new EntryFailedException("Could not leave the extended or graphics cursor mode.");
        }

        if ((Peek(Flags2) & 0x08) != 0)
        {
            Press(CapsLockKeys);
            _restoreCapsLock = true;
        }

        return ClearEditLine();
    }

    private void TypeLine(BasicLine line)
    {
        foreach (var c in line.Number.ToString())
        {
            TypeCharacter(c, line);
        }

        foreach (var item in line.Items)
        {
            switch (item)
            {
                case KeywordItem keywordItem:
                    TypeKeyword(keywordItem.Keyword, line);
                    break;

                case CharacterItem characterItem:
                    TypeCharacter(characterItem.Character, line);
                    break;
            }
        }

        if (line.Remark != null)
        {
            TypeRemark(line.Remark, line);
        }

        VerifyEditLine(line);

        Press(EnterKeys, EnterTimeoutFrames);

        if (ReadEditLine().Count == 0)
        {
            return;
        }

        var shown = ReadEditLineFromScreen();
        ClearEditLine();

        throw new EntryFailedException($"Line {line.Number} was rejected by the ROM (syntax error, '?' marks the position): {shown}");
    }

    private void TypeKeyword(BasicKeyword keyword, BasicLine line)
    {
        switch (keyword.Entry)
        {
            case KeywordEntry.KMode when !IsKMode():
                throw new EntryFailedException($"Line {line.Number}: {keyword.Name} can only start a statement " +
                                               "(after the line number, ':' or THEN).");

            case KeywordEntry.KMode:
            case KeywordEntry.SymbolShift:
                Press(keyword.Keys);
                break;

            case KeywordEntry.Spelled:
                throw new EntryFailedException($"Line {line.Number}: {keyword.Name} is a 128 BASIC keyword.");

            default:
                PressExtended(keyword.Keys);
                break;
        }
    }

    private void TypeCharacter(char c, BasicLine line)
    {
        if (char.IsAsciiLetter(c) && IsKMode())
        {
            // At the K cursor a letter key gives a keyword, so the listing has a letter where a statement starts
            throw new EntryFailedException($"Line {line.Number}: a statement must start with a keyword, found '{c}'. " +
                                           "48K BASIC needs LET for assignments, and keywords must be separated by spaces.");
        }

        if (BasicKeywords.ExtendedCharacters.TryGetValue(c, out var key))
        {
            PressExtended([SymbolShift, key]);
        }
        else
        {
            Press(Input.KeyMapper.FromChar(c)!);
        }
    }

    /// <summary>
    /// The ROM switches to the K cursor after ':' even in a REM, so a letter after it would become a keyword.
    /// The remark is typed without colons first, then the colons are inserted using the cursor keys.
    /// </summary>
    private void TypeRemark(string remark, BasicLine line)
    {
        foreach (var c in remark.Where(c => c != ':'))
        {
            TypeCharacter(c, line);
        }

        var colons = remark.Select((c, index) => (c, index)).Where(x => x.c == ':').Select(x => x.index).ToList();

        if (colons.Count == 0)
        {
            return;
        }

        // Positions are relative to the start of the remark, the cursor is at its end
        var cursor = remark.Length - colons.Count;

        for (var k = colons.Count - 1; k >= 0; k--)
        {
            // Colons to the left of this one are not typed yet
            var position = colons[k] - k;

            PressRepeated(LeftKeys, cursor - position);
            TypeCharacter(':', line);

            cursor = position + 1;
        }

        PressRepeated(RightKeys, remark.Length - cursor);
    }

    private void VerifyEditLine(BasicLine line)
    {
        var expected = line.ExpectedBytes;
        var actual = ReadEditLine();

        if (actual.SequenceEqual(expected))
        {
            return;
        }

        ClearEditLine();

        throw new EntryFailedException($"Line {line.Number} was not typed correctly. Expected: " +
                                       $"{BasicKeywords.Detokenize(expected)}, typed: {BasicKeywords.Detokenize(actual)}");
    }

    private int ClearEditLine()
    {
        var length = ReadEditLine().Count;

        if (length == 0)
        {
            return 0;
        }

        for (var i = 0; i <= length && ReadWord(KCur) != GetEditLineEnd(); i++)
        {
            Press(RightKeys);
        }

        for (var i = 0; i <= length && ReadEditLine().Count > 0; i++)
        {
            Press(DeleteKeys);
        }

        if (ReadEditLine().Count > 0)
        {
            throw new EntryFailedException("Could not clear the edit line.");
        }

        return length;
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

    private void PressExtended(SpectrumKey[] keys)
    {
        Press(ExtendKeys);

        if (Peek(Mode) != 1)
        {
            throw new EntryFailedException("The editor did not switch to the extended (E) cursor mode.");
        }

        Press(keys);
    }

    private void PressRepeated(SpectrumKey[] keys, int count)
    {
        for (var i = 0; i < count; i++)
        {
            Press(keys);
        }
    }

    /// <summary>
    /// Presses and releases the keys, then waits until the ROM has processed them and redrawn the edit line,
    /// so the cursor mode can be read and the next key is decoded correctly.
    /// </summary>
    private void Press(SpectrumKey[] keys, int timeoutFrames = KeyTimeoutFrames)
    {
        var result = _controller.PressKeys(_emulator, [keys], HoldFrames, ReleaseFrames);

        ThrowIfStopped(result.StopReason);

        if (!WaitUntilReady(timeoutFrames))
        {
            throw new EntryFailedException("The 48K BASIC editor stopped responding to key presses.");
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
    /// Detects that the editor is idle: KEY-INPUT is entered with no key waiting and no pending redraw,
    /// from within the editor (not e.g. from 'scroll?').
    /// </summary>
    private void OnBeforeInstruction(Word pc)
    {
        if (pc != KeyInput || _isReady)
        {
            return;
        }

        var memory = _emulator.Memory;

        if (memory.IsShadowRomActive)
        {
            return;
        }

        for (var i = 0; i < KeyInputCode.Length; i++)
        {
            if (memory.Read((Word)(KeyInput + i)) != KeyInputCode[i])
            {
                return;
            }
        }

        if ((Peek(TvFlag) & 0x08) != 0 || (Peek(Flags) & 0x20) != 0)
        {
            return;
        }

        _isReady = ReadWord(ReadWord(ErrSp)) == EdError;
    }

    private bool IsKMode() => Peek(Mode) == 0 && (Peek(Flags) & 0x08) == 0;

    private List<byte> ReadEditLine()
    {
        var result = new List<byte>();
        var address = ReadWord(ELine);

        for (var i = 0; i < MaxEditLineLength; i++)
        {
            var value = Peek((Word)(address + i));

            if (value == EndOfLine)
            {
                break;
            }

            result.Add(value);
        }

        return result;
    }

    private Word GetEditLineEnd() => (Word)(ReadWord(ELine) + ReadEditLine().Count);

    private string ReadEditLineFromScreen()
    {
        var rows = Math.Clamp((int)Peek(DfSz), 1, ScreenTextReader.Rows);
        var lines = ScreenTextReader.Create(_emulator.Memory).ReadLines(_emulator.Memory);
        var text = string.Concat(lines[^rows..]).Trim();

        // The last character is the cursor (K, L, C or E)
        return text.Length > 0 ? text[..^1] : text;
    }

    private byte Peek(Word address) => _emulator.Memory.Read(address);

    private Word ReadWord(Word address) => (Word)(Peek(address) | Peek((Word)(address + 1)) << 8);

    private static bool IsExtendedCharacter(BasicItem item) =>
        item is CharacterItem characterItem && BasicKeywords.ExtendedCharacters.ContainsKey(characterItem.Character);

    private static void ThrowIfStopped(StopReason stopReason)
    {
        if (stopReason != StopReason.None)
        {
            throw new EntryStoppedException(stopReason);
        }
    }

    private abstract class EntryException(string message) : Exception(message);

    private sealed class EntryFailedException(string message) : EntryException(message);

    private sealed class EntryStoppedException(StopReason stopReason) : EntryException(stopReason.ToString())
    {
        internal StopReason StopReason { get; } = stopReason;
    }
}
