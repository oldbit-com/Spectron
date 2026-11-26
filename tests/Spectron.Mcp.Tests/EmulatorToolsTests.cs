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
    public async Task GetStatus_ShouldReportRunningStateFromBeforeTheCall()
    {
        var result = await _tools.GetStatus();

        result.ShouldContain("Paused: False");
        _host.ResumeCount.ShouldBe(1);
    }

    [Fact]
    public async Task GetStatus_ShouldReportPausedStateWithoutResuming()
    {
        _host.IsPaused = true;

        var result = await _tools.GetStatus();

        result.ShouldContain("Paused: True");
        _host.PauseCount.ShouldBe(0);
        _host.ResumeCount.ShouldBe(0);
    }

    [Fact]
    public async Task RunFrames_ShouldStopWhenUserResumes()
    {
        OnFrame(5, () => _host.IsPaused = false);

        var result = await _tools.RunFrames(100);

        result.ShouldStartWith("Ran 5 frame(s).");
        result.ShouldEndWith("Stopped because the user resumed emulation.");
    }

    [Fact]
    public async Task TypeText_ShouldStopAndNotResumeWhenEmulatorReplaced()
    {
        var replacement = new TestEmulatorHost().Emulator;
        OnFrame(4, () => _host.Emulator = replacement);

        var result = await _tools.TypeText("abc");

        result.ShouldStartWith("Ran 4 frame(s).");
        result.ShouldEndWith("Stopped because the machine was replaced (file loaded or machine reset).");
        _host.ResumeCount.ShouldBe(0);
    }

    [Fact]
    public async Task TypeText_ShouldStopAndNotResumeAtBreakpoint()
    {
        var emulator = _host.Emulator!;
        OnFrame(2, () => emulator.Break());

        var result = await _tools.TypeText("abc");

        result.ShouldStartWith("Ran 2 frame(s).");
        result.ShouldContain("Stopped at a breakpoint");
        _host.ResumeCount.ShouldBe(0);
        emulator.KeyboardState.Read(0x00FE).ShouldBe((byte)0xFF);
    }

    [Fact]
    public async Task ReadOnlyTools_ShouldRetryWhenEmulatorReplacedWhilePausing()
    {
        var original = _host.Emulator;
        var replacement = new TestEmulatorHost().Emulator;
        var host = new ReplacingHost(_host, replacement!);
        var tools = new EmulatorTools(new EmulatorController(host));

        await tools.GetRegisters();

        host.PauseCount.ShouldBe(2);
        _host.Emulator.ShouldNotBe(original);
    }

    [Theory]
    [InlineData(new[] { "EXTEND", "SYM+H" }, "10 CIRCLE")]
    [InlineData(new[] { "EXTEND", "H" }, "10SQR")]
    [InlineData(new[] { "EXTEND", "SYM+X" }, "10 INK")]
    public async Task PressKeys_ShouldEnterExtendedModeKeywords(string[] keys, string expected)
    {
        await _tools.RunFrames(150);

        await _tools.TypeText("10");
        await _tools.PressKeys(keys);

        var lines = (await _tools.ReadScreenText()).Split('\n');

        lines.Last(line => line.Length > 0).ShouldStartWith(expected);
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

    private void OnFrame(int frame, Action action)
    {
        var frames = 0;

        _host.Emulator!.FrameCompleted += (_, _) =>
        {
            if (++frames == frame)
            {
                action();
            }
        };
    }

    /// <summary>
    /// Simulates the user loading a file while the first pause is in progress.
    /// </summary>
    private sealed class ReplacingHost(TestEmulatorHost inner, OldBit.Spectron.Emulation.Emulator replacement) : IEmulatorHost
    {
        public OldBit.Spectron.Emulation.Emulator? Emulator => inner.Emulator;
        public bool IsPaused => inner.IsPaused;
        public int PauseCount { get; private set; }

        public async Task PauseAsync()
        {
            if (PauseCount++ == 0)
            {
                inner.Emulator = replacement;
                await inner.PauseAsync();
                inner.IsPaused = false;
                return;
            }

            await inner.PauseAsync();
        }

        public Task ResumeAsync() => inner.ResumeAsync();
        public Task ResetAsync(bool hardReset) => inner.ResetAsync(hardReset);
        public Task<OldBit.Spectron.Emulation.Files.FileType> LoadFileAsync(string filePath, string? archiveEntry) =>
            inner.LoadFileAsync(filePath, archiveEntry);
        public OldBit.Spectron.Debugger.Breakpoints.BreakpointHandler? Breakpoints => inner.Breakpoints;
        public Task EnableBreakpointsAsync() => inner.EnableBreakpointsAsync();
        public void NotifyStateChanged() => inner.NotifyStateChanged();
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
