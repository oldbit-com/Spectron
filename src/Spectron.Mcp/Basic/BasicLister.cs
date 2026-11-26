using System.Text;
using OldBit.Spectron.Mcp.Screen;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Mcp.Basic;

/// <summary>
/// A BASIC line as LIST shows it.
/// </summary>
/// <param name="Number">The line number.</param>
/// <param name="Text">The line after the number, starting with the space LIST prints before a command.</param>
internal sealed record ListedLine(int Number, string Text)
{
    public override string ToString() => $"{Number}{Text}";
}

/// <summary>
/// The program read from memory.
/// </summary>
/// <param name="Lines">The lines in memory order.</param>
/// <param name="Problem">Why the listing stopped early, or null when the whole program area was read.</param>
internal sealed record BasicProgram(IReadOnlyList<ListedLine> Lines, string? Problem);

/// <summary>
/// Reads the BASIC program from memory and renders it as text, with the same spacing as LIST.
/// </summary>
internal static class BasicLister
{
    internal const Word Prog = 23635;
    internal const Word Vars = 23627;

    // A conservative floor for PROG: the channel information (CHANS) starts at $5CB6 and the program follows it,
    // normally at $5CCB, later with Interface 1 or extra channels
    private const Word MinProg = 0x5CB6;

    private const byte Enter = 0x0D;
    private const byte HiddenNumber = 0x0E;
    private const int HiddenNumberLength = 5;
    private const byte Quote = 0x22;
    private const byte FirstUdg = 0x90;

    private static readonly string[] ColourControls = ["INK", "PAPER", "FLASH", "BRIGHT", "INVERSE", "OVER"];

    /// <summary>
    /// Reads all lines from PROG up to VARS.
    /// </summary>
    /// <exception cref="InvalidOperationException">PROG and VARS do not describe a program area.</exception>
    internal static BasicProgram Read(IMemory memory)
    {
        var prog = ReadWord(memory, Prog);
        var vars = ReadWord(memory, Vars);

        if (prog < MinProg || vars < prog)
        {
            throw new InvalidOperationException(
                $"The system variables PROG (${prog:X4}) and VARS (${vars:X4}) do not point to a BASIC program area. " +
                "A machine-code program may have overwritten them.");
        }

        var lines = new List<ListedLine>();
        var address = (int)prog;

        while (address < vars)
        {
            if (address + 4 > vars)
            {
                return new BasicProgram(lines, $"The program area ends with {vars - address} stray byte(s) at ${address:X4}.");
            }

            var number = memory.Read((Word)address) << 8 | memory.Read((Word)(address + 1));
            var length = ReadWord(memory, (Word)(address + 2));

            if (address + 4 + length > vars)
            {
                return new BasicProgram(lines,
                    $"Line {number} at ${address:X4} is {length} bytes long and runs past VARS (${vars:X4}). " +
                    "The program area may be corrupted.");
            }

            var data = new byte[length];

            for (var i = 0; i < length; i++)
            {
                data[i] = memory.Read((Word)(address + 4 + i));
            }

            lines.Add(new ListedLine(number, FormatLine(data)));
            address += 4 + length;
        }

        return new BasicProgram(lines, null);
    }

    /// <summary>
    /// Finds a line in the program and returns its bytes after the number and length, or null when there is no such line.
    /// </summary>
    internal static byte[]? ReadLineBytes(IMemory memory, int number)
    {
        var prog = ReadWord(memory, Prog);
        var vars = ReadWord(memory, Vars);

        if (prog < MinProg || vars < prog)
        {
            return null;
        }

        var address = (int)prog;

        while (address + 4 <= vars)
        {
            var lineNumber = memory.Read((Word)address) << 8 | memory.Read((Word)(address + 1));
            var length = ReadWord(memory, (Word)(address + 2));

            if (lineNumber > number || address + 4 + length > vars)
            {
                return null;
            }

            if (lineNumber == number)
            {
                return Enumerable.Range(address + 4, length).Select(a => memory.Read((Word)a)).ToArray();
            }

            address += 4 + length;
        }

        return null;
    }

    /// <summary>
    /// Removes the hidden 5-byte numbers and the final ENTER from the bytes of a program line,
    /// leaving what the edit line had before the line was entered.
    /// </summary>
    internal static List<byte> StripHiddenNumbers(ReadOnlySpan<byte> data)
    {
        var result = new List<byte>(data.Length);
        var isInString = false;
        var isInRemark = false;

        for (var i = 0; i < data.Length; i++)
        {
            var b = data[i];

            if (b == Enter && i == data.Length - 1)
            {
                break;
            }

            if (b == HiddenNumber && !isInString && !isInRemark && i + HiddenNumberLength < data.Length)
            {
                i += HiddenNumberLength;
                continue;
            }

            if (b == Quote && !isInRemark)
            {
                isInString = !isInString;
            }

            isInRemark |= b == BasicKeywords.RemToken && !isInString;
            result.Add(b);
        }

        return result;
    }

    /// <summary>
    /// Renders the bytes of a line after its number and length, as LIST shows them.
    /// </summary>
    /// <remarks>
    /// Spacing follows PO-TOKENS in the 48K ROM: a keyword gets a leading space unless it is a function
    /// (RND to BIN), starts with a symbol (&lt;=, &gt;=, &lt;&gt;) or the last character printed was a space.
    /// It gets a trailing space unless it ends with a symbol other than '$', or is RND, INKEY$ or PI.
    /// The trailing space of a keyword that ends the line is left out.
    /// Characters that LIST does not print as text are shown in braces, e.g. {INK 2}, {AT 1,5}, {UDG A} or {$06}.
    /// </remarks>
    internal static string FormatLine(ReadOnlySpan<byte> data)
    {
        var text = new StringBuilder();
        var isInString = false;
        var isInRemark = false;
        var lastWasSpace = false;
        var endsWithKeywordSpace = false;
        var i = 0;

        while (i < data.Length)
        {
            var b = data[i];

            if (b == Enter && i == data.Length - 1)
            {
                break;
            }

            endsWithKeywordSpace = false;

            // The syntax checker stores a number's value in 5 hidden bytes after its digits, except in strings and REM
            if (b == HiddenNumber && !isInString && !isInRemark && i + HiddenNumberLength < data.Length)
            {
                i += 1 + HiddenNumberLength;
                continue;
            }

            switch (b)
            {
                case >= 0x10 and <= 0x15 when i + 1 < data.Length:
                    text.Append($"{{{ColourControls[b - 0x10]} {data[i + 1]}}}");
                    i += 2;
                    continue;

                case 0x16 when i + 2 < data.Length:
                    text.Append($"{{AT {data[i + 1]},{data[i + 2]}}}");
                    i += 3;
                    continue;

                case 0x17 when i + 2 < data.Length:
                    text.Append($"{{TAB {data[i + 1] | data[i + 2] << 8}}}");
                    i += 3;
                    continue;

                case >= BasicKeywords.FirstToken:
                    var index = b - BasicKeywords.FirstToken;
                    var name = BasicKeywords.Get(b).Name;
                    AppendKeyword(name, leadingSpace: index >= 0x20 && char.IsAsciiLetter(name[0]), trailingSpace: index >= 3);
                    isInRemark |= b == BasicKeywords.RemToken && !isInString;
                    break;

                case BasicKeywords.Spectrum128Token or BasicKeywords.Play128Token when !isInString && !isInRemark:
                    // 128 BASIC keywords. In strings and on 48K these codes are the UDGs T and U.
                    AppendKeyword(BasicKeywords.Get(b).Name, leadingSpace: true, trailingSpace: true);
                    break;

                case >= 0x20 and < 0x80:
                    if (b == Quote && !isInRemark)
                    {
                        isInString = !isInString;
                    }

                    AppendCharacter(BasicKeywords.ToChar(b));
                    break;

                case >= 0x80 and < FirstUdg:
                    AppendCharacter(ScreenTextReader.BlockGraphics[b - 0x80]);
                    break;

                case >= FirstUdg:
                    text.Append($"{{UDG {(char)('A' + b - FirstUdg)}}}");
                    lastWasSpace = false;
                    break;

                default:
                    text.Append($"{{${b:X2}}}");
                    break;
            }

            i++;
        }

        if (endsWithKeywordSpace)
        {
            text.Length--;
        }

        return text.ToString();

        void AppendCharacter(char c)
        {
            text.Append(c);
            lastWasSpace = c == ' ';
        }

        void AppendKeyword(string name, bool leadingSpace, bool trailingSpace)
        {
            if (leadingSpace && !lastWasSpace)
            {
                text.Append(' ');
            }

            text.Append(name);
            lastWasSpace = false;

            var last = name[^1];

            if (trailingSpace && (last == '$' || char.IsAsciiLetter(last)))
            {
                text.Append(' ');
                lastWasSpace = true;
                endsWithKeywordSpace = true;
            }
        }
    }

    private static Word ReadWord(IMemory memory, Word address) =>
        (Word)(memory.Read(address) | memory.Read((Word)(address + 1)) << 8);
}
