using ModelContextProtocol;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Mcp.Basic;
using OldBit.Spectron.Mcp.Tests.Fixtures;
using OldBit.Spectron.Mcp.Tools;

namespace OldBit.Spectron.Mcp.Tests.Basic;

public class TypeBasic128Tests
{
    private const int Flags2 = 23658;

    private readonly TestEmulatorHost _host = new(ComputerType.Spectrum128K);
    private readonly EmulatorTools _tools;

    public TypeBasic128Tests()
    {
        _tools = new EmulatorTools(new EmulatorController(_host));
    }

    [Fact]
    public async Task TypeBasic_ShouldEnterProgramThatRuns()
    {
        const string program = """
                               5 LET a=1: LET b=2: LET total=0
                               10 BORDER 1: PLAY "c"
                               20 FOR i=1 TO 10 STEP 2: PRINT AT i,0;"X": NEXT i
                               30 IF a<=b AND (b<>a) THEN GOTO 40
                               35 STOP
                               40 PRINT AT 12,0;"TO BE";SIN 0;"[";INT (RND*6)+1>0
                               50 REM done: all ok
                               """;
        await StartBasic128();

        var result = await _tools.TypeBasic(program);

        result.ShouldBe("Entered 7 line(s): 5, 10, 20, 30, 35, 40, 50.");
        (await _tools.ListBasic()).ShouldBe("""
                                            5 LET a=1: LET b=2: LET total=0
                                            10 BORDER 1: PLAY "c"
                                            20 FOR i=1 TO 10 STEP 2: PRINT AT i,0;"X": NEXT i
                                            30 IF a<=b AND (b<>a) THEN GO TO 40
                                            35 STOP
                                            40 PRINT AT 12,0;"TO BE";SIN 0;"[";INT (RND*6)+1>0
                                            50 REM done: all ok
                                            """);

        await _tools.TypeText("run\n");
        await _tools.RunFrames(100);

        var lines = (await _tools.ReadScreenText()).Split('\n');

        foreach (var row in new[] { 1, 3, 5, 7, 9 })
        {
            lines[row].ShouldStartWith("X");
        }

        lines[12].ShouldStartWith("TO BE0[1");
        lines[23].ShouldBe("0 OK, 50:1");
    }

    [Fact]
    public async Task TypeBasic_ShouldInsertReplaceAndDeleteLines()
    {
        await StartBasic128();

        await _tools.TypeBasic("20 PRINT 2\n10 PRINT 1\n30 PRINT 3");
        await _tools.TypeBasic("15 PRINT 15\n20 PRINT 20\n30");

        (await _tools.ListBasic()).ShouldBe("10 PRINT 1\n15 PRINT 15\n20 PRINT 20");
    }

    [Fact]
    public async Task TypeBasic_ShouldReportRejectedLineAndClearIt()
    {
        await StartBasic128();

        var ex = await Should.ThrowAsync<McpException>(() => _tools.TypeBasic("10 PRINT 1\n20 PRINT 1: NEXT : PRINT 3\n30 PRINT 3"));

        ex.Message.ShouldBe("Line 20 was rejected by the ROM (syntax error, '?' marks the position): " +
                            "20 PRINT 1: NEXT?: PRINT 3 Entered 1 line(s): 10.");

        await _tools.TypeBasic("20 PRINT 2");

        (await _tools.ListBasic()).ShouldBe("10 PRINT 1\n20 PRINT 2");
    }

    [Fact]
    public async Task TypeBasic_ShouldClearTextLeftAtCursor()
    {
        await StartBasic128();
        await _tools.TypeText("print \"abc");

        var result = await _tools.TypeBasic("10 CLS");

        result.ShouldBe("Entered 1 line(s): 10. Cleared 10 character(s) that were already in the edit line.");
        (await _tools.ListBasic()).ShouldBe("10 CLS");
    }

    [Fact]
    public async Task TypeBasic_ShouldOpenNewRowWhenCursorIsOnProgramLine()
    {
        await StartBasic128();
        await _tools.TypeBasic("10 PRINT 1\n20 PRINT 2");
        await _tools.PressKeys(["UP", "UP"]);

        await _tools.TypeBasic("15 PRINT 15");

        (await _tools.ListBasic()).ShouldBe("10 PRINT 1\n15 PRINT 15\n20 PRINT 2");
    }

    [Fact]
    public async Task TypeBasic_ShouldFailWhenProgramLineIsBeingEdited()
    {
        await StartBasic128();
        await _tools.TypeBasic("10 PRINT 1");
        await _tools.PressKeys(["UP"]);
        await _tools.TypeText("2");

        var ex = await Should.ThrowAsync<McpException>(() => _tools.TypeBasic("20 CLS"));

        ex.Message.ShouldStartWith("Line 10 is being edited at the cursor.");
    }

    [Fact]
    public async Task TypeBasic_ShouldTypeLowerCaseWithCapsLockOnAndRestoreIt()
    {
        await StartBasic128();
        await _tools.PressKeys(["CAPSLOCK"]);

        await _tools.TypeBasic("10 PRINT \"abc\"");

        (await _tools.ListBasic()).ShouldBe("10 PRINT \"abc\"");
        (_host.Emulator!.Memory.Read(Flags2) & 0x08).ShouldBe(0x08);
    }

    [Fact]
    public async Task TypeBasic_ShouldFailInMainMenu()
    {
        await _tools.RunFrames(150);

        var ex = await Should.ThrowAsync<McpException>(() => _tools.TypeBasic("10 CLS"));

        ex.Message.ShouldStartWith("A 128 menu is shown.");
    }

    [Fact]
    public async Task TypeBasic_ShouldFailWhileProgramIsRunning()
    {
        await StartBasic128();
        await _tools.TypeBasic("10 GO TO 10");
        await _tools.TypeText("run\n");

        var ex = await Should.ThrowAsync<McpException>(() => _tools.TypeBasic("20 CLS"));

        ex.Message.ShouldStartWith(Basic128Editor.NotReadyMessage);
    }

    [Fact]
    public async Task TypeText_ShouldTypeWholeProgramWhileEditorIsBusy()
    {
        await StartBasic128();

        await _tools.TypeText("10 border 1: cls\n20 for i=1 to 10\n30 print i\n40 next i\n");

        (await _tools.ListBasic()).ShouldBe("10 BORDER 1: CLS\n20 FOR i=1 TO 10\n30 PRINT i\n40 NEXT i");
    }

    [Theory]
    [InlineData("10 PRINT a", "10 print a")]
    [InlineData("10 IF a$=\"x\" THEN GO TO 20", "10 if a$=\"x\" then go to 20")]
    [InlineData("10 PRINT INKEY$;PI", "10 print inkey$ ;pi")]
    [InlineData("10 LET a=INT (RND*6)", "10 let a=int (rnd *6)")]
    [InlineData("10 PLAY \"c\": REM x: y", "10 play \"c\":rem x: y")]
    [InlineData("10 REM", "10 rem")]
    public void FormatForTyping_ShouldSpellOutKeywords(string listing, string expected)
    {
        Basic128Editor.FormatForTyping(BasicLineParser.ParseLine(listing, isBasic128: true)).ShouldBe(expected);
    }

    private async Task StartBasic128()
    {
        await _tools.RunFrames(150);
        await _tools.PressKeys(["DOWN", "ENTER"]);
        await _tools.RunFrames(100);
    }
}
