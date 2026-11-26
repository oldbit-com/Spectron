using OldBit.Spectron.Mcp.Basic;

namespace OldBit.Spectron.Mcp.Tests.Basic;

public class BasicListerTests
{
    private static readonly object[] HiddenOne = [0x0E, 0x00, 0x00, 0x01, 0x00, 0x00];

    [Fact]
    public void FormatLine_ShouldSkipHiddenNumbersAndFinalEnter()
    {
        var text = Format("PRINT", "1", HiddenOne, "+12", 0x0E, 0x00, 0x00, 0x0C, 0x00, 0x00, 0x0D);

        text.ShouldBe(" PRINT 1+12");
    }

    [Theory]
    [InlineData("CLS", " CLS")]
    [InlineData("RANDOMIZE", " RANDOMIZE")]
    [InlineData("OPEN #", " OPEN #")]
    public void FormatLine_ShouldLeaveOutTrailingSpaceAtEndOfLine(string keyword, string expected)
    {
        Format(keyword, 0x0D).ShouldBe(expected);
    }

    [Fact]
    public void FormatLine_ShouldSpaceKeywordsLikeList()
    {
        Format("IF", "a", "<=", "b", "THEN", "GO TO", "1", HiddenOne).ShouldBe(" IF a<=b THEN GO TO 1");
        Format("BORDER", "1", HiddenOne, ":", "CIRCLE", "1", HiddenOne).ShouldBe(" BORDER 1: CIRCLE 1");
        Format("PRINT", "INKEY$", ";", "RND", ";", "PI", ";", "SIN", "0", HiddenOne).ShouldBe(" PRINT INKEY$;RND;PI;SIN 0");
        Format("LET", "x=", "INT", "(", "RND", "*6)").ShouldBe(" LET x=INT (RND*6)");
        Format("PRINT", "1", "AND", "NOT", "0", "OR", "1").ShouldBe(" PRINT 1 AND NOT 0 OR 1");
        Format("OPEN #", "4,\"p\"").ShouldBe(" OPEN #4,\"p\"");
        Format("LET", "a$=", "STR$", "1").ShouldBe(" LET a$=STR$ 1");
    }

    [Fact]
    public void FormatLine_ShouldKeepStringsAndRemarksLiteral()
    {
        Format("PRINT", "\"TO BE\"").ShouldBe(" PRINT \"TO BE\"");
        Format("REM", " note: a\u000Eb").ShouldBe(" REM  note: a{$0E}b");
        Format("REM", "x", 0x60, 0x7F).ShouldBe(" REM x£©");
    }

    [Fact]
    public void FormatLine_ShouldNotStartRemarkAtRemTokenInsideString()
    {
        Format("PRINT", "\"", "REM", "\":", "PRINT", "1", HiddenOne, 0x0D).ShouldBe(" PRINT \" REM \": PRINT 1");
    }

    [Fact]
    public void FormatLine_ShouldShowControlCodesInBraces()
    {
        Format("PRINT", "\"", 0x10, 0x02, 0x11, 0x07, 0x12, 0x01, 0x13, 0x01, 0x14, 0x00, 0x15, 0x01, "A\"")
            .ShouldBe(" PRINT \"{INK 2}{PAPER 7}{FLASH 1}{BRIGHT 1}{INVERSE 0}{OVER 1}A\"");
        Format("PRINT", "\"", 0x16, 0x01, 0x05, 0x17, 0x08, 0x00, "X", 0x06, "\"")
            .ShouldBe(" PRINT \"{AT 1,5}{TAB 8}X{$06}\"");
    }

    [Fact]
    public void FormatLine_ShouldShowGraphics()
    {
        Format("PRINT", "\"", 0x80, 0x83, 0x8F, 0x90, 0xA2, 0xA3, "\"").ShouldBe(" PRINT \" ▀█{UDG A}{UDG S}{UDG T}\"");
    }

    [Fact]
    public void FormatLine_ShouldShow128BasicKeywordsOutsideStrings()
    {
        Format(0xA3, 0x0D).ShouldBe(" SPECTRUM");
        Format(0xA4, "\"a\"").ShouldBe(" PLAY \"a\"");
    }

    [Fact]
    public void FormatLine_ShouldShowTruncatedControlCodeAsHex()
    {
        Format("PRINT", "\"", 0x16, 0x01).ShouldBe(" PRINT \"{$16}{$01}");
    }

    /// <summary>
    /// Builds line bytes from keyword names (when the string is a keyword), text and byte values.
    /// </summary>
    private static string Format(params object[] parts)
    {
        var bytes = new List<byte>();

        foreach (var part in parts)
        {
            switch (part)
            {
                case object[] nested:
                    bytes.AddRange(nested.Select(Convert.ToByte));
                    break;

                case string text when BasicKeywords.All.FirstOrDefault(keyword => keyword.Name == text) is { } keyword:
                    bytes.Add(keyword.Token);
                    break;

                case string text:
                    bytes.AddRange(text.Select(c => (byte)c));
                    break;

                default:
                    bytes.Add(Convert.ToByte(part));
                    break;
            }
        }

        return BasicLister.FormatLine(bytes.ToArray());
    }
}
