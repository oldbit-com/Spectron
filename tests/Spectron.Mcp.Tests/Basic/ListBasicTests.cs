using System.Text.RegularExpressions;
using ModelContextProtocol;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Mcp.Tests.Fixtures;
using OldBit.Spectron.Mcp.Tools;

namespace OldBit.Spectron.Mcp.Tests.Basic;

public partial class ListBasicTests
{
    private readonly TestEmulatorHost _host;
    private readonly EmulatorTools _tools;

    public ListBasicTests() : this(ComputerType.Spectrum48K)
    {
    }

    private ListBasicTests(ComputerType computerType)
    {
        _host = new TestEmulatorHost(computerType);
        _tools = new EmulatorTools(new EmulatorController(_host));
    }

    [Fact]
    public async Task ListBasic_ShouldListProgramEnteredWithTypeBasic()
    {
        await _tools.RunFrames(150);
        await _tools.TypeBasic("""
                               5 LET a=1: LET b=2
                               10 BORDER 1: CIRCLE 128,88,50
                               20 FOR i=1 TO 10 STEP 2: PRINT AT i,0;"X": NEXT i
                               30 IF a<=b THEN GO TO 40
                               35 STOP
                               40 INK 2: PAPER 7: PRINT AT 12,0;"TO BE";SIN 0;" ";INT (RND*6)+1>0
                               50 REM done: all ok
                               60 RANDOMIZE
                               """);

        var listing = await _tools.ListBasic();

        listing.ShouldBe("""
                         5 LET a=1: LET b=2
                         10 BORDER 1: CIRCLE 128,88,50
                         20 FOR i=1 TO 10 STEP 2: PRINT AT i,0;"X": NEXT i
                         30 IF a<=b THEN GO TO 40
                         35 STOP
                         40 INK 2: PAPER 7: PRINT AT 12,0;"TO BE";SIN 0;" ";INT (RND*6)+1>0
                         50 REM done: all ok
                         60 RANDOMIZE
                         """.ReplaceLineEndings("\n"));
    }

    [Fact]
    public async Task ListBasic_ShouldMatchRomListOutput()
    {
        const string program = """
                               10 BORDER 1: CIRCLE 128,88,50
                               20 IF a<=b THEN GO TO 10
                               30 PRINT INKEY$;RND;PI;SIN 0
                               40 LET a$=STR$ 1+CHR$ 65
                               50 OPEN #4,"p": CLOSE #4
                               60 PRINT AT 1,2;"TO BE";TAB 3
                               70 REM note: a:b
                               80 DEF FN f(x)=x*2: CLS
                               90 PRINT 1 AND NOT 0 OR 1
                               """;
        await _tools.RunFrames(150);
        await _tools.TypeBasic(program);

        var listing = (await _tools.ListBasic()).Split('\n');

        await _tools.TypeText("k\n");
        await _tools.RunFrames(50);

        var screen = (await _tools.ReadScreenText()).Split('\n')
            .Take(listing.Length)
            .Select(line => CurrentLineMarker().Replace(line.TrimStart(), "$1 "));

        screen.ShouldBe(listing);
    }

    [Fact]
    public async Task ListBasic_ShouldListLineRange()
    {
        await _tools.RunFrames(150);
        await _tools.TypeBasic("10 CLS\n20 PRINT 2\n30 PRINT 3\n40 STOP");

        (await _tools.ListBasic(fromLine: 20, toLine: 30)).ShouldBe("20 PRINT 2\n30 PRINT 3");
        (await _tools.ListBasic(fromLine: 11, toLine: 19)).ShouldBe("There are no lines between 11 and 19.");
    }

    [Fact]
    public async Task ListBasic_ShouldReportEmptyProgram()
    {
        await _tools.RunFrames(150);

        (await _tools.ListBasic()).ShouldBe("There is no BASIC program in memory.");
    }

    [Fact]
    public async Task ListBasic_ShouldRejectInvalidRange()
    {
        await Should.ThrowAsync<McpException>(() => _tools.ListBasic(fromLine: 20, toLine: 10));
        await Should.ThrowAsync<McpException>(() => _tools.ListBasic(fromLine: -1));
    }

    [Fact]
    public async Task ListBasic_ShouldReportInvalidSystemVariables()
    {
        await _tools.RunFrames(150);
        await _tools.WriteMemory("23635", "00 00");

        var ex = await Should.ThrowAsync<McpException>(() => _tools.ListBasic());

        ex.Message.ShouldStartWith("The system variables PROG ($0000) and VARS");
    }

    [Fact]
    public async Task ListBasic_ShouldStopAtLineRunningPastVars()
    {
        await _tools.RunFrames(150);
        await _tools.TypeBasic("10 CLS\n20 PRINT 2");

        var vars = ReadWord(23627);
        await _tools.WriteMemory("23627", $"{(vars - 1) & 0xFF:X2} {(vars - 1) >> 8:X2}");

        var listing = await _tools.ListBasic();

        listing.ShouldStartWith("10 CLS\n(Listing stopped: Line 20 at $");
        listing.ShouldEndWith($"runs past VARS (${vars - 1:X4}). The program area may be corrupted.)");
    }

    [Fact]
    public async Task ListBasic_ShouldList128BasicProgram()
    {
        var test = new ListBasicTests(ComputerType.Spectrum128K);
        await test._tools.RunFrames(150);
        await test._tools.PressKeys(["DOWN", "ENTER"]);
        await test._tools.RunFrames(100);
        await test._tools.TypeText("10 play \"cde\": print \"ok\"\n");
        await test._tools.RunFrames(50);

        (await test._tools.ListBasic()).ShouldBe("10 PLAY \"cde\": PRINT \"ok\"");
    }

    private int ReadWord(int address) =>
        _host.Emulator!.Memory.Read((Word)address) | _host.Emulator.Memory.Read((Word)(address + 1)) << 8;

    // LIST marks the current line with '>' after the number, e.g. "10>BORDER 1"
    [GeneratedRegex(@"^(\d+)>")]
    private static partial Regex CurrentLineMarker();
}
