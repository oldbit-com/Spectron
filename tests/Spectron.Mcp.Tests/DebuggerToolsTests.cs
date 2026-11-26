using ModelContextProtocol;
using OldBit.Spectron.Debugger.Breakpoints;
using OldBit.Spectron.Mcp.Tests.Fixtures;
using OldBit.Spectron.Mcp.Tools;

namespace OldBit.Spectron.Mcp.Tests;

public class DebuggerToolsTests
{
    // $8000  LD A,5
    // $8002  CALL $8008
    // $8005  INC A
    // $8006  JR $
    // $8008  INC A          ; subroutine
    // $8009  LD ($9000),A
    // $800C  RET
    private const string Routine = "3E 05 CD 08 80 3C 18 FE 3C 32 00 90 C9";

    private readonly TestEmulatorHost _host = new();
    private readonly EmulatorTools _emulatorTools;
    private readonly DebuggerTools _tools;

    public DebuggerToolsTests()
    {
        var controller = new EmulatorController(_host);

        _emulatorTools = new EmulatorTools(controller);
        _tools = new DebuggerTools(controller);
    }

    private BreakpointManager Manager => _host.Breakpoints!.BreakpointManager;

    private Word PC => _host.Emulator!.Cpu.Registers.PC;

    private byte A => _host.Emulator!.Cpu.Registers.A;

    private async Task LoadRoutineAsync()
    {
        // Interrupts stay disabled after reset, so the routine runs undisturbed
        await _emulatorTools.WriteMemory("$8000", Routine);
        await _emulatorTools.DebuggerCommand("SP = $FF00; GOTO $8000");
    }

    [Fact]
    public async Task SetBreakpoint_ShouldAddToSharedListAndTurnBreakpointsOn()
    {
        var result = await _tools.SetBreakpoint("pc==32776");

        result.ShouldBe("Breakpoint #1 set: PC == $8008. Breakpoints were turned off, turned them on.");
        _host.Breakpoints!.IsEnabled.ShouldBeTrue();
        Manager.Breakpoints.ShouldHaveSingleItem().Condition.ShouldBe("PC == $8008");
    }

    [Fact]
    public async Task SetBreakpoint_ShouldReturnExistingIdForSameCondition()
    {
        await _tools.SetBreakpoint("PC==$8008");

        var result = await _tools.SetBreakpoint("PC == 0x8008");

        result.ShouldBe("Breakpoint #1 already exists: PC == $8008.");
        Manager.Breakpoints.Count.ShouldBe(1);
    }

    [Fact]
    public async Task SetBreakpoint_ShouldEnableExistingDisabledBreakpoint()
    {
        await _tools.SetBreakpoint("$9000");
        Manager.Breakpoints[0].IsEnabled = false;

        var result = await _tools.SetBreakpoint("$9000");

        result.ShouldStartWith("Breakpoint #1 already existed but was disabled, enabled it");
        Manager.Breakpoints.ShouldHaveSingleItem().IsEnabled.ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("PC=$8000")]
    [InlineData("XY==5")]
    [InlineData("A==$1234")]
    [InlineData("$9000==$100")]
    public async Task SetBreakpoint_ShouldRejectInvalidCondition(string condition)
    {
        var exception = await Should.ThrowAsync<McpException>(() => _tools.SetBreakpoint(condition));

        exception.Message.ShouldStartWith($"Invalid condition '{condition}'.");
        exception.Message.ShouldContain("PC==$8000");
        Manager.Breakpoints.ShouldBeEmpty();
    }

    [Fact]
    public async Task ListBreakpoints_ShouldListIdsConditionsAndState()
    {
        await _tools.SetBreakpoint("PC==$8008");
        await _tools.SetBreakpoint("$9000==6");
        Manager.Breakpoints[1].IsEnabled = false;

        var result = await _tools.ListBreakpoints();

        result.ShouldBe("""
                        Breakpoints are on.
                        #1 PC == $8008
                        #2 $9000 == $06 (disabled)
                        """);
    }

    [Fact]
    public async Task ListBreakpoints_ShouldSayWhenThereAreNone()
    {
        var result = await _tools.ListBreakpoints();

        result.ShouldBe("No breakpoints.");
    }

    [Fact]
    public async Task ClearBreakpoint_ShouldRemoveById()
    {
        await _tools.SetBreakpoint("PC==$8008");
        await _tools.SetBreakpoint("$9000");

        var result = await _tools.ClearBreakpoint(id: 1);

        result.ShouldBe("Removed breakpoint #1: PC == $8008.");
        Manager.Breakpoints.ShouldHaveSingleItem().Id.ShouldBe(2);
    }

    [Fact]
    public async Task ClearBreakpoint_ShouldRemoveAll()
    {
        await _tools.SetBreakpoint("PC==$8008");
        await _tools.SetBreakpoint("$9000");

        var result = await _tools.ClearBreakpoint(all: true);

        result.ShouldBe("Removed 2 breakpoint(s).");
        Manager.Breakpoints.ShouldBeEmpty();
    }

    [Fact]
    public async Task ClearBreakpoint_ShouldRejectUnknownIdOrMissingArguments()
    {
        await Should.ThrowAsync<McpException>(() => _tools.ClearBreakpoint(id: 7));
        await Should.ThrowAsync<McpException>(() => _tools.ClearBreakpoint());
    }

    [Fact]
    public async Task RunFrames_ShouldStopAtPcBreakpointAndSayWhichOne()
    {
        await LoadRoutineAsync();
        await _tools.SetBreakpoint("PC==$8008");

        var result = await _emulatorTools.RunFrames(10);

        result.ShouldStartWith("Ran 1 frame(s). PC=$8008. Stopped at breakpoint #1 (PC == $8008), previous PC=$8002, frame T-state ");
        PC.ShouldBe((Word)0x8008);
        A.ShouldBe((byte)5);
        _host.Emulator!.IsDebuggerBreak.ShouldBeTrue();

        var status = await _emulatorTools.GetStatus();

        status.ShouldContain("Breakpoints: 1 (on)");
        status.ShouldContain("Stopped at breakpoint #1 (PC == $8008)");
        (await _tools.ListBreakpoints()).ShouldEndWith("Stopped at breakpoint #1 (PC == $8008), previous PC=$8002, frame T-state " +
                                                      $"{_host.Emulator.Cpu.Clock.FrameTicks}.");
    }

    [Fact]
    public async Task RunFrames_ShouldContinueAfterBreakpoint()
    {
        await LoadRoutineAsync();
        await _tools.SetBreakpoint("PC==$8008");
        await _emulatorTools.RunFrames(10);

        var result = await _emulatorTools.RunFrames(2);

        result.ShouldBe("Ran 2 frame(s). PC=$8006.");
        _host.Emulator!.IsDebuggerBreak.ShouldBeFalse();
        _host.Emulator.Memory.Read(0x9000).ShouldBe((byte)6);
        A.ShouldBe((byte)7);
        (await _emulatorTools.GetStatus()).ShouldNotContain("Stopped at");
    }

    [Fact]
    public async Task RunFrames_ShouldStopAfterMemoryWrite()
    {
        await LoadRoutineAsync();
        await _tools.SetBreakpoint("$9000");

        var result = await _emulatorTools.RunFrames(10);

        result.ShouldStartWith("Ran 1 frame(s). PC=$800C. Stopped at breakpoint #1 ($9000), previous PC=$8009");
        _host.Emulator!.Memory.Read(0x9000).ShouldBe((byte)6);
    }

    [Fact]
    public async Task RunFrames_ShouldIgnoreMemoryWriteOfOtherValue()
    {
        await LoadRoutineAsync();
        await _tools.SetBreakpoint("$9000==7");

        var result = await _emulatorTools.RunFrames(2);

        result.ShouldBe("Ran 2 frame(s). PC=$8006.");
    }

    [Fact]
    public async Task RunFrames_ShouldNotStopWhenBreakpointsAreTurnedOff()
    {
        await LoadRoutineAsync();
        await _tools.SetBreakpoint("PC==$8008");
        _host.Breakpoints!.IsEnabled = false;

        var result = await _emulatorTools.RunFrames(2);

        result.ShouldBe("Ran 2 frame(s). PC=$8006.");
        (await _tools.ListBreakpoints()).ShouldStartWith("Breakpoints are turned off");
    }

    [Fact]
    public async Task StepInto_ShouldExecuteOneInstruction()
    {
        await LoadRoutineAsync();

        var result = await _tools.Step();

        result.ShouldStartWith("""
                               Stepped into 1 instruction(s):
                                 $8000  3E 05        LD A,$05
                               PC=$8002, previous PC=$8000, frame T-state
                               """);
        result.ShouldContain("Next: $8002  CD 08 80     CALL $8008");
        result.ShouldContain("AF=$05");
        PC.ShouldBe((Word)0x8002);
        A.ShouldBe((byte)5);
    }

    [Fact]
    public async Task StepInto_ShouldEnterCalls()
    {
        await LoadRoutineAsync();

        await _tools.Step("into", count: 2);

        PC.ShouldBe((Word)0x8008);
        _host.Emulator!.Cpu.Registers.SP.ShouldBe((Word)0xFEFE);
    }

    [Fact]
    public async Task StepInto_ShouldStopAtBreakpoint()
    {
        await LoadRoutineAsync();
        await _tools.SetBreakpoint("PC==$8008");

        var result = await _tools.Step("into", count: 10);

        result.ShouldStartWith("Stepped into 2 instruction(s):");
        result.ShouldContain("Stopped at breakpoint #1 (PC == $8008), previous PC=$8002");
        PC.ShouldBe((Word)0x8008);
    }

    [Fact]
    public async Task StepOver_ShouldRunCallAsOneStepWithoutAddingBreakpoints()
    {
        await LoadRoutineAsync();
        await _tools.Step();

        var result = await _tools.Step("over");

        result.ShouldStartWith("""
                               Stepped over 1 instruction(s), ran 1 frame(s):
                                 $8002  CD 08 80     CALL $8008  (stepped over)
                               PC=$8005, previous PC=$800C, frame T-state
                               """);
        PC.ShouldBe((Word)0x8005);
        A.ShouldBe((byte)6);
        _host.Emulator!.IsDebuggerBreak.ShouldBeFalse();
        Manager.Breakpoints.ShouldBeEmpty();
        _host.Breakpoints!.IsEnabled.ShouldBeFalse();
    }

    [Fact]
    public async Task StepOver_ShouldStepSimpleInstructions()
    {
        await LoadRoutineAsync();

        var result = await _tools.Step("over", count: 3);

        result.ShouldContain("$8000  3E 05        LD A,$05\n");
        result.ShouldContain("CALL $8008  (stepped over)\n");
        result.ShouldContain("$8005  3C           INC A\n");
        PC.ShouldBe((Word)0x8006);
        A.ShouldBe((byte)7);
    }

    [Fact]
    public async Task StepOver_ShouldStopAtBreakpointInsideCall()
    {
        await LoadRoutineAsync();
        await _tools.Step();
        await _tools.SetBreakpoint("PC==$8009");

        var result = await _tools.Step("over");

        result.ShouldContain("Stopped at breakpoint #1 (PC == $8009), previous PC=$8008");
        PC.ShouldBe((Word)0x8009);
        _host.Emulator!.IsDebuggerBreak.ShouldBeTrue();
    }

    [Fact]
    public async Task StepOut_ShouldRunUntilRoutineReturns()
    {
        await LoadRoutineAsync();
        await _tools.Step("into", count: 2);

        var result = await _tools.Step("out");

        result.ShouldStartWith("""
                               Stepped out 1 time(s), ran 1 frame(s):
                                 $8008  ... ran until the routine returned
                               PC=$8005, previous PC=$800C, frame T-state
                               """);
        PC.ShouldBe((Word)0x8005);
        A.ShouldBe((byte)6);
        _host.Emulator!.IsDebuggerBreak.ShouldBeFalse();
    }

    [Fact]
    public async Task StepOut_ShouldGiveUpAfterFrameBudget()
    {
        await LoadRoutineAsync();
        await _tools.Step("over", count: 3);

        var result = await _tools.Step("out", maxFrames: 2);

        result.ShouldContain("ran 2 frame(s)");
        result.ShouldContain("The routine did not return within 2 frame(s), stopped.");
        PC.ShouldBe((Word)0x8006);
    }

    [Theory]
    [InlineData("sideways", 1, 50)]
    [InlineData("into", 0, 50)]
    [InlineData("into", DebuggerTools.MaxStepCount + 1, 50)]
    [InlineData("over", 1, 0)]
    public async Task Step_ShouldRejectInvalidArguments(string mode, int count, int maxFrames)
    {
        await Should.ThrowAsync<McpException>(() => _tools.Step(mode, count, maxFrames));
    }

    [Fact]
    public async Task RunUntil_ShouldStopWhenConditionIsMetAndLeaveNoBreakpoint()
    {
        await LoadRoutineAsync();

        var result = await _tools.RunUntil("PC==$800C");

        result.ShouldStartWith("Condition PC == $800C met after 1 frame(s).\nPC=$800C, previous PC=$8009, frame T-state ");
        A.ShouldBe((byte)6);
        Manager.Breakpoints.ShouldBeEmpty();
        _host.Emulator!.IsDebuggerBreak.ShouldBeFalse();

        // The temporary breakpoint is gone, so running on doesn't stop there again
        (await _emulatorTools.RunFrames(2)).ShouldBe("Ran 2 frame(s). PC=$8006.");
    }

    [Fact]
    public async Task RunUntil_ShouldStopAtMemoryWrite()
    {
        await LoadRoutineAsync();

        var result = await _tools.RunUntil("$9000==6");

        result.ShouldStartWith("Condition $9000 == $06 met after 1 frame(s).\nPC=$800C, previous PC=$8009");
    }

    [Fact]
    public async Task RunUntil_ShouldStopAtUserBreakpointFirst()
    {
        await LoadRoutineAsync();
        await _tools.SetBreakpoint("PC==$8008");

        var result = await _tools.RunUntil("PC==$800C");

        result.ShouldStartWith("Stopped at breakpoint #1 (PC == $8008), previous PC=$8002");
        result.ShouldContain("The condition PC == $800C was not met yet");
        PC.ShouldBe((Word)0x8008);
    }

    [Fact]
    public async Task RunUntil_ShouldReportWhenConditionIsNotMet()
    {
        await LoadRoutineAsync();

        var result = await _tools.RunUntil("A==$FF", maxFrames: 3);

        result.ShouldStartWith("The condition A == $FF was not met within 3 frame(s).");
    }

    [Fact]
    public async Task RunUntil_ShouldRejectInvalidCondition()
    {
        var exception = await Should.ThrowAsync<McpException>(() => _tools.RunUntil("PC>$8000"));

        exception.Message.ShouldStartWith("Invalid condition 'PC>$8000'.");
    }

    [Fact]
    public async Task ToolsThatChangeState_ShouldNotifyHost()
    {
        await _emulatorTools.GetRegisters();
        await _tools.ListBreakpoints();
        _host.NotifyStateChangedCount.ShouldBe(0);

        await _emulatorTools.WriteMemory("$8000", "00");
        await _emulatorTools.DebuggerCommand("A = 1");
        await _emulatorTools.RunFrames(1);
        await _tools.Step();

        _host.NotifyStateChangedCount.ShouldBe(4);
    }
}
