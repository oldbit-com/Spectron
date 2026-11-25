using ModelContextProtocol;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Mcp.Basic;
using OldBit.Spectron.Mcp.Tests.Fixtures;
using OldBit.Spectron.Mcp.Tools;

namespace OldBit.Spectron.Mcp.Tests.Basic;

public class TypeBasicTests
{
    private const int Prog = 23635;
    private const int Vars = 23627;
    private const int ELine = 23641;
    private const int Mode = 23617;
    private const int Flags2 = 23658;

    private readonly TestEmulatorHost _host;
    private readonly EmulatorTools _tools;

    public TypeBasicTests() : this(ComputerType.Spectrum48K)
    {
    }

    private TypeBasicTests(ComputerType computerType)
    {
        _host = new TestEmulatorHost(computerType);
        _tools = new EmulatorTools(new EmulatorController(_host));
    }

    [Fact]
    public async Task TypeBasic_ShouldEnterProgramThatRuns()
    {
        const string program = """
                               5 LET a=1: LET b=2
                               10 BORDER 1: CIRCLE 128,88,50
                               20 FOR i=1 TO 10 STEP 2: PRINT AT i,0;"X": NEXT i
                               30 IF a<=b THEN GO TO 40
                               35 STOP
                               40 INK 2: PAPER 7: PRINT AT 12,0;"TO BE";SIN 0;" ";INT (RND*6)+1 > 0
                               50 REM done: all ok
                               """;
        await _tools.RunFrames(150);

        var result = await _tools.TypeBasic(program);

        result.ShouldBe("Entered 7 line(s): 5, 10, 20, 30, 35, 40, 50.");
        ShouldContainLines(program);

        await _tools.TypeText("r\n");
        await _tools.RunFrames(100);

        var lines = (await _tools.ReadScreenText()).Split('\n');

        foreach (var row in new[] { 1, 3, 5, 7, 9 })
        {
            lines[row].ShouldStartWith("X");
        }

        lines[12].ShouldStartWith("TO BE0 1");
        lines[23].ShouldBe("0 OK, 50:1");
    }

    [Fact]
    public async Task TypeBasic_ShouldKeepLettersAfterColonInRemark()
    {
        await _tools.RunFrames(150);

        await _tools.TypeBasic("10 REM Note: a:b: :c");

        ShouldContainLines("10 REM Note: a:b: :c");
    }

    [Fact]
    public async Task TypeBasic_ShouldTypeLowerCaseWithCapsLockOnAndRestoreIt()
    {
        await _tools.RunFrames(150);
        await _tools.PressKeys(["CAPSLOCK"]);

        await _tools.TypeBasic("10 PRINT \"abc\"");

        ShouldContainLines("10 PRINT \"abc\"");
        (Peek(Flags2) & 0x08).ShouldBe(0x08);
    }

    [Fact]
    public async Task TypeBasic_ShouldLeaveExtendedMode()
    {
        await _tools.RunFrames(150);
        await _tools.PressKeys(["EXTEND"]);
        Peek(Mode).ShouldBe((byte)1);

        await _tools.TypeBasic("10 CLS");

        ShouldContainLines("10 CLS");
    }

    [Fact]
    public async Task TypeBasic_ShouldClearLeftoverEditLine()
    {
        await _tools.RunFrames(150);
        await _tools.TypeText("12");

        var result = await _tools.TypeBasic("10 CLS");

        result.ShouldBe("Entered 1 line(s): 10. Cleared 2 character(s) that were already in the edit line.");
        ReadProgram().Keys.ShouldBe([10]);
    }

    [Fact]
    public async Task TypeBasic_ShouldReportLineRejectedByRom()
    {
        await _tools.RunFrames(150);

        var ex = await Should.ThrowAsync<McpException>(() => _tools.TypeBasic("10 CLS\n20 PRINT 1)\n30 CLS"));

        ex.Message.ShouldBe("Line 20 was rejected by the ROM (syntax error, '?' marks the position): 20 PRINT 1?) Entered 1 line(s): 10.");
        ReadProgram().Keys.ShouldBe([10]);
        Peek(ReadWord(ELine)).ShouldBe((byte)0x0D);
    }

    [Theory]
    [InlineData("10 PRINT GO TO 5", "Line 10: GO TO can only start a statement")]
    [InlineData("10 x=1", "Line 10: a statement must start with a keyword, found 'x'")]
    [InlineData("10 PRINT 1: y=2", "Line 10: a statement must start with a keyword, found 'y'")]
    public async Task TypeBasic_ShouldReportListingErrors(string program, string message)
    {
        await _tools.RunFrames(150);

        var ex = await Should.ThrowAsync<McpException>(() => _tools.TypeBasic(program));

        ex.Message.ShouldStartWith(message);
        ex.Message.ShouldEndWith("No lines were entered.");
        Peek(ReadWord(ELine)).ShouldBe((byte)0x0D);
    }

    [Fact]
    public async Task TypeBasic_ShouldRejectEmptyAndInvalidListings()
    {
        (await Should.ThrowAsync<McpException>(() => _tools.TypeBasic(" \n "))).Message.ShouldBe("The program has no lines.");
        (await Should.ThrowAsync<McpException>(() => _tools.TypeBasic("PRINT 1"))).Message.ShouldContain("no line number");
    }

    [Fact]
    public async Task TypeBasic_ShouldFailWhenProgramIsRunning()
    {
        await _tools.RunFrames(150);
        await _tools.TypeBasic("10 GO TO 10");
        await _tools.TypeText("r\n");

        var ex = await Should.ThrowAsync<McpException>(() => _tools.TypeBasic("20 CLS"));

        ex.Message.ShouldStartWith(BasicEditor.NotReadyMessage);
    }

    [Fact]
    public async Task TypeBasic_ShouldFailIn128Basic()
    {
        var test = new TypeBasicTests(ComputerType.Spectrum128K);
        await test._tools.RunFrames(150);
        await test._tools.PressKeys(["ENTER"]);
        await test._tools.RunFrames(100);

        var ex = await Should.ThrowAsync<McpException>(() => test._tools.TypeBasic("10 CLS"));

        ex.Message.ShouldStartWith(BasicEditor.NotReadyMessage);
    }

    [Fact]
    public async Task TypeBasic_ShouldWorkIn48BasicOn128K()
    {
        var test = new TypeBasicTests(ComputerType.Spectrum128K);
        await test._tools.RunFrames(150);
        await test._tools.PressKeys(["DOWN", "DOWN", "DOWN", "ENTER"]);
        await test._tools.RunFrames(100);

        await test._tools.TypeBasic("10 CIRCLE 128,88,50: PRINT INKEY$");

        test.ShouldContainLines("10 CIRCLE 128,88,50: PRINT INKEY$");
    }

    private void ShouldContainLines(string program)
    {
        var stored = ReadProgram();

        foreach (var line in BasicLineParser.Parse(program))
        {
            stored.ShouldContainKey(line.Number);
            BasicKeywords.Detokenize(stored[line.Number]).ShouldBe(BasicKeywords.Detokenize(line.ExpectedBytes.Skip(line.Number.ToString().Length)));
            stored[line.Number].ShouldBe(line.ExpectedBytes.Skip(line.Number.ToString().Length).ToArray());
        }
    }

    /// <summary>
    /// Reads the program area: line number to line bytes, without hidden numbers and the final ENTER.
    /// </summary>
    private Dictionary<int, byte[]> ReadProgram()
    {
        var result = new Dictionary<int, byte[]>();
        var address = ReadWord(Prog);
        var end = ReadWord(Vars);

        while (address < end)
        {
            var number = Peek(address) << 8 | Peek(address + 1);
            var length = ReadWord(address + 2);
            var text = new List<byte>();

            for (var i = address + 4; i < address + 4 + length - 1; i++)
            {
                if (Peek(i) == 0x0E)
                {
                    i += 5;
                    continue;
                }

                text.Add(Peek(i));
            }

            result[number] = text.ToArray();
            address += 4 + length;
        }

        return result;
    }

    private byte Peek(int address) => _host.Emulator!.Memory.Read((Word)address);

    private int ReadWord(int address) => Peek(address) | Peek(address + 1) << 8;
}
