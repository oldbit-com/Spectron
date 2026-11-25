using OldBit.Spectron.Mcp.Basic;

namespace OldBit.Spectron.Mcp.Tests.Basic;

public class BasicLineParserTests
{
    [Theory]
    [InlineData("10 BORDER 1: CIRCLE 128,88,50", "10 BORDER 1: CIRCLE 128,88,50")]
    [InlineData("20 FOR i=1 TO 10 STEP 2: PRINT AT i,0;\"X\": NEXT i", "20 FOR i=1 TO 10 STEP 2: PRINT AT i,0;\"X\": NEXT i")]
    [InlineData("30 IF a<=b THEN GO TO 10", "30 IF a <= b THEN GO TO 10")]
    [InlineData("40 if a<>b then goto 10", "40 IF a <> b THEN GO TO 10")]
    [InlineData("50 GOSUB 100", "50 GO SUB 100")]
    [InlineData("60 LET k$=INKEY$: INK 2: PAPER 7", "60 LET k$= INKEY$ : INK 2: PAPER 7")]
    [InlineData("70 LET x=INT (RND*6)+SIN a", "70 LET x= INT ( RND *6)+ SIN a")]
    [InlineData("80 DEF FN f(x)=x*2: PRINT FN f(2)", "80 DEF FN f(x)=x*2: PRINT FN f(2)")]
    [InlineData("90 OPEN #4,\"b\": CLOSE#4", "90 OPEN # 4,\"b\": CLOSE # 4")]
    [InlineData("100 PRINT ATTR (0,0); AT 1,1", "100 PRINT ATTR (0,0); AT 1,1")]
    [InlineData("110 INPUT a: PRINT IN 254", "110 INPUT a: PRINT IN 254")]
    [InlineData("120CLS", "120 CLS")]
    public void ParseLine_ShouldFindKeywords(string text, string expected)
    {
        var line = BasicLineParser.ParseLine(text);

        BasicKeywords.Detokenize(line.ExpectedBytes).ShouldBe(expected);
    }

    [Theory]
    [InlineData("10 LET total=1", "total")]
    [InlineData("10 LET into=1", "into")]
    [InlineData("10 LET cat2=1", "cat2")]
    [InlineData("10 LET a$=b$", "a$=b$")]
    public void ParseLine_ShouldNotFindKeywordsInsideNames(string text, string name)
    {
        var line = BasicLineParser.ParseLine(text);

        line.Items.OfType<KeywordItem>().Select(item => item.Keyword.Name).ShouldBe(["LET"]);
        new string(line.Items.OfType<CharacterItem>().Select(item => item.Character).ToArray()).ShouldStartWith(name);
    }

    [Fact]
    public void ParseLine_ShouldKeepStringsLiteral()
    {
        var line = BasicLineParser.ParseLine("10 PRINT \"TO BE or  NOT\";\"<=\"\"\"");

        line.Items.OfType<KeywordItem>().Count().ShouldBe(1);
        BasicKeywords.Detokenize(line.ExpectedBytes).ShouldBe("10 PRINT \"TO BE or NOT\";\"<=\"\"\"");
        line.ExpectedBytes.Count(b => b == ' ').ShouldBe(4);
    }

    [Fact]
    public void ParseLine_ShouldKeepRemarkLiteral()
    {
        var line = BasicLineParser.ParseLine("10 PRINT 1: REM go to: PRINT \"x  ");

        line.Items.OfType<KeywordItem>().Select(item => item.Keyword.Name).ShouldBe(["PRINT", "REM"]);
        line.Remark.ShouldBe("go to: PRINT \"x");
    }

    [Fact]
    public void ParseLine_ShouldAcceptLineNumberOnly()
    {
        var line = BasicLineParser.ParseLine(" 20 ");

        line.Number.ShouldBe(20);
        line.Items.ShouldBeEmpty();
        line.ExpectedBytes.ShouldBe("20"u8.ToArray());
    }

    [Fact]
    public void ParseLine_ShouldAcceptExtendedCharacters()
    {
        var line = BasicLineParser.ParseLine("10 PRINT \"[©]\"");

        line.ExpectedBytes[^4..].ShouldBe([0x5B, 0x7F, 0x5D, 0x22]);
    }

    [Theory]
    [InlineData("PRINT 1", "no line number")]
    [InlineData("0 PRINT 1", "between 1 and 9999")]
    [InlineData("10000 PRINT 1", "between 1 and 9999")]
    [InlineData("123456789012 PRINT 1", "between 1 and 9999")]
    [InlineData("10 PRINT \"\t\"", "column 11")]
    [InlineData("10 PRINT \"█\"", "cannot be typed")]
    [InlineData("10 REM `", "cannot be typed")]
    public void ParseLine_ShouldRejectInvalidLines(string text, string message)
    {
        Should.Throw<ArgumentException>(() => BasicLineParser.ParseLine(text)).Message.ShouldContain(message);
    }

    [Fact]
    public void Parse_ShouldSkipBlankLines()
    {
        var lines = BasicLineParser.Parse("10 CLS\r\n\n   \n20 STOP\n");

        lines.Select(line => line.Number).ShouldBe([10, 20]);
    }
}
