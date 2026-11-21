using System.Buffers.Binary;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using OldBit.Spectron.Mcp.Tests.Fixtures;
using OldBit.Spectron.Mcp.Tools;

namespace OldBit.Spectron.Mcp.Tests;

public class EmulatorToolsTests
{
    private readonly TestEmulatorHost _host = new();
    private readonly EmulatorTools _tools;

    public EmulatorToolsTests()
    {
        _tools = new EmulatorTools(new EmulatorController(_host));
    }

    [Fact]
    public async Task RunFrames_ShouldPauseAndStayPaused()
    {
        var result = await _tools.RunFrames(10);

        result.ShouldStartWith("Ran 10 frame(s).");
        _host.PauseCount.ShouldBe(1);
        _host.ResumeCount.ShouldBe(0);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(EmulatorController.MaxFramesPerCall + 1)]
    public async Task RunFrames_ShouldRejectInvalidCount(int count)
    {
        await Should.ThrowAsync<McpException>(() => _tools.RunFrames(count));
    }

    [Fact]
    public async Task ReadOnlyTools_ShouldRestoreRunningState()
    {
        await _tools.GetRegisters();

        _host.PauseCount.ShouldBe(1);
        _host.ResumeCount.ShouldBe(1);
    }

    [Fact]
    public async Task TypeText_ShouldEnterAndRunBasicCommand()
    {
        await _tools.RunFrames(150);

        // 'p' at the K cursor produces the PRINT keyword in 48K BASIC
        await _tools.TypeText("p2*21\n");
        await _tools.RunFrames(10);

        var lines = (await _tools.ReadScreenText()).Split('\n');

        lines[0].ShouldBe("42");
        lines[23].ShouldBe("0 OK, 0:1");
    }

    [Fact]
    public async Task PressKeys_ShouldEnterKeywordsUsingKeyNames()
    {
        await _tools.RunFrames(150);

        await _tools.PressKeys(["P", "SYM+P", "CAPS+H", "I", "SYM+P", "ENTER"]);
        await _tools.RunFrames(10);

        var text = await _tools.ReadScreenText();

        text.ShouldStartWith("Hi\n");
    }

    [Fact]
    public async Task TypeText_ShouldRejectUnsupportedCharacters()
    {
        await Should.ThrowAsync<McpException>(() => _tools.TypeText("{"));
    }

    [Fact]
    public async Task ReadMemory_ShouldReturnHexDump()
    {
        var result = await _tools.ReadMemory("$0000", 4);

        result.ShouldBe("$0000: F3 AF 11 FF                                      ....");
    }

    [Fact]
    public async Task WriteMemory_ShouldPokeBytes()
    {
        var result = await _tools.WriteMemory("0x8000", "3E 01 C9");

        result.ShouldBe("Wrote 3 byte(s) at $8000.");
        (await _tools.ReadMemory("32768", 3)).ShouldStartWith("$8000: 3E 01 C9");
    }

    [Fact]
    public async Task Disassemble_ShouldListInstructions()
    {
        var result = await _tools.Disassemble("0", 2);

        result.ShouldBe("$0000  F3           DI\n$0001  AF           XOR A");
    }

    [Fact]
    public async Task DebuggerCommand_ShouldAssignAndPrintRegister()
    {
        var result = await _tools.DebuggerCommand("HL = $1234; PRINT HL");

        result.ShouldBe("HL=$1234 (4660)");
    }

    [Fact]
    public async Task DebuggerCommand_ShouldPeekMemory()
    {
        var result = await _tools.DebuggerCommand("PEEK 0");

        result.ShouldBe("243 ($F3)");
    }

    [Fact]
    public async Task DebuggerCommand_ShouldReportSyntaxErrors()
    {
        await Should.ThrowAsync<McpException>(() => _tools.DebuggerCommand("POKE 1"));
    }

    [Fact]
    public async Task Screenshot_ShouldReturnScaledPng()
    {
        var result = await _tools.Screenshot(scale: 2);

        var image = result.Content.ShouldHaveSingleItem().ShouldBeOfType<ImageContentBlock>();
        image.MimeType.ShouldBe("image/png");

        var png = image.DecodedData.ToArray();
        png[..8].ShouldBe(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });
        BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(16)).ShouldBe(512);
        BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(20)).ShouldBe(384);
    }

    [Fact]
    public async Task LoadFile_ShouldRejectMissingFile()
    {
        await Should.ThrowAsync<McpException>(() => _tools.LoadFile("/no/such/file.tap"));
    }

    [Theory]
    [InlineData("16384", 0x4000)]
    [InlineData("$4000", 0x4000)]
    [InlineData("#4000", 0x4000)]
    [InlineData("0x4000", 0x4000)]
    [InlineData("4000h", 0x4000)]
    [InlineData(" 65535 ", 0xFFFF)]
    public void ParseAddress_ShouldAcceptCommonFormats(string address, int expected)
    {
        EmulatorTools.ParseAddress(address).ShouldBe((Word)expected);
    }

    [Theory]
    [InlineData("65536")]
    [InlineData("-1")]
    [InlineData("$XYZ")]
    public void ParseAddress_ShouldRejectInvalidValues(string address)
    {
        Should.Throw<McpException>(() => EmulatorTools.ParseAddress(address));
    }

    [Theory]
    [InlineData("3E 01 C9", new byte[] { 0x3E, 0x01, 0xC9 })]
    [InlineData("3e01c9", new byte[] { 0x3E, 0x01, 0xC9 })]
    [InlineData("0x3E,01", new byte[] { 0x3E, 0x01 })]
    public void ParseHexBytes_ShouldAcceptCommonFormats(string data, byte[] expected)
    {
        EmulatorTools.ParseHexBytes(data).ShouldBe(expected);
    }
}
