using OldBit.Spectron.Mcp.Input;

namespace OldBit.Spectron.Mcp.Basic;

/// <summary>
/// A keyword or a plain character of a BASIC line.
/// </summary>
internal abstract record BasicItem
{
    internal abstract byte Code { get; }
}

internal sealed record KeywordItem(BasicKeyword Keyword) : BasicItem
{
    internal override byte Code => Keyword.Token;
}

internal sealed record CharacterItem(char Character) : BasicItem
{
    internal override byte Code => BasicKeywords.ToCode(Character);
}

/// <summary>
/// A parsed BASIC line ready to be typed into the 48K editor.
/// </summary>
/// <param name="Number">The line number.</param>
/// <param name="Items">Keywords and characters after the line number, up to and including REM.</param>
/// <param name="Remark">Text after REM, typed literally, or null when the line has no REM.</param>
internal sealed record BasicLine(int Number, IReadOnlyList<BasicItem> Items, string? Remark)
{
    /// <summary>
    /// The bytes the edit line should contain once the line has been typed.
    /// </summary>
    internal byte[] ExpectedBytes =>
    [
        ..Number.ToString().Select(c => (byte)c),
        ..Items.Select(item => item.Code),
        ..(Remark ?? string.Empty).Select(BasicKeywords.ToCode)
    ];
}

/// <summary>
/// Splits a plain-text BASIC listing into line numbers, keywords and characters.
/// </summary>
internal static class BasicLineParser
{
    internal const int MaxLineNumber = 9999;

    // Longest names first, so INKEY$ wins over INK and GO SUB over GO TO's prefix
    private static readonly Lazy<BasicKeyword[]> KeywordsByLength = new(() =>
        BasicKeywords.All.OrderByDescending(keyword => keyword.Name.Length).ToArray());

    /// <summary>
    /// Parses a listing, one BASIC line per text line. Blank lines are skipped.
    /// </summary>
    /// <exception cref="ArgumentException">The listing contains a line that cannot be typed.</exception>
    internal static List<BasicLine> Parse(string program) => program
        .Replace("\r\n", "\n")
        .Split('\n')
        .Where(text => !string.IsNullOrWhiteSpace(text))
        .Select(ParseLine)
        .ToList();

    internal static BasicLine ParseLine(string text)
    {
        var position = 0;

        while (position < text.Length && char.IsWhiteSpace(text[position]))
        {
            position++;
        }

        var numberStart = position;

        while (position < text.Length && char.IsAsciiDigit(text[position]))
        {
            position++;
        }

        if (position == numberStart)
        {
            throw new ArgumentException($"'{text.Trim()}' has no line number. Every line needs a number from 1 to {MaxLineNumber}.");
        }

        var number = position - numberStart <= 5 ? int.Parse(text[numberStart..position]) : 0;

        if (number is < 1 or > MaxLineNumber)
        {
            throw new ArgumentException($"Line number in '{text.Trim()}' must be between 1 and {MaxLineNumber}.");
        }

        var bodyStart = position;
        var items = new List<BasicItem>();
        var isInString = false;

        while (position < text.Length)
        {
            var c = text[position];

            if (isInString || c == '"')
            {
                if (c == '"')
                {
                    isInString = !isInString;
                }

                items.Add(CreateCharacter(c, number, position));
                position++;

                continue;
            }

            if (char.IsWhiteSpace(c))
            {
                // The ROM adds its own spaces around keywords; outside strings spaces have no meaning
                position++;

                continue;
            }

            var keyword = MatchKeyword(text, position, bodyStart, out var length);

            if (keyword == null)
            {
                items.Add(CreateCharacter(c, number, position));
                position++;

                continue;
            }

            items.Add(new KeywordItem(keyword));
            position += length;

            if (keyword.Token == BasicKeywords.RemToken)
            {
                return new BasicLine(number, items, ParseRemark(text[position..], number, position));
            }
        }

        return new BasicLine(number, items, null);
    }

    private static string ParseRemark(string remark, int number, int position)
    {
        // LIST shows a space after REM, so a single space there is not part of the remark
        if (remark.StartsWith(' '))
        {
            remark = remark[1..];
            position++;
        }

        remark = remark.TrimEnd();

        for (var i = 0; i < remark.Length; i++)
        {
            CreateCharacter(remark[i], number, position + i);
        }

        return remark;
    }

    private static BasicKeyword? MatchKeyword(string text, int position, int bodyStart, out int length)
    {
        foreach (var keyword in KeywordsByLength.Value)
        {
            if (TryMatch(text, position, bodyStart, keyword.Name, out length))
            {
                return keyword;
            }
        }

        length = 0;

        return null;
    }

    private static bool TryMatch(string text, int position, int bodyStart, string name, out int length)
    {
        length = 0;

        if (char.IsAsciiLetter(name[0]) && position > bodyStart && char.IsAsciiLetterOrDigit(text[position - 1]))
        {
            return false;
        }

        var index = position;

        foreach (var c in name)
        {
            if (c == ' ')
            {
                // GO TO, GO SUB, DEF FN, OPEN # and CLOSE # can be written with or without the space
                while (index < text.Length && char.IsWhiteSpace(text[index]))
                {
                    index++;
                }

                continue;
            }

            if (index >= text.Length || char.ToUpperInvariant(text[index]) != c)
            {
                return false;
            }

            index++;
        }

        if (char.IsAsciiLetter(name[^1]) && index < text.Length && (char.IsAsciiLetterOrDigit(text[index]) || text[index] == '$'))
        {
            return false;
        }

        length = index - position;

        return true;
    }

    private static CharacterItem CreateCharacter(char c, int number, int position)
    {
        if (c is '\n' or '\b' || (KeyMapper.FromChar(c) == null && !BasicKeywords.ExtendedCharacters.ContainsKey(c)))
        {
            throw new ArgumentException($"Line {number}, column {position + 1}: character '{c}' cannot be typed. " +
                                        "Block graphics, UDGs and colour control codes are not supported.");
        }

        return new CharacterItem(c);
    }
}
