using OldBit.Spectron.Debugger.Breakpoints;
using OldBit.Spectron.Debugger.Tests.Fixtures;
using OldBit.Spectron.Emulation;

namespace OldBit.Spectron.Debugger.Tests.Breakpoints;

public class BreakpointHandlerTests : IDisposable
{
    private const Word CodeAddress = 0x8000;

    private readonly Emulator _emulator = TestEmulator.Create();
    private readonly BreakpointHandler _handler;
    private readonly List<BreakpointHitEventArgs> _hits = [];

    public BreakpointHandlerTests()
    {
        // LD A,5; LD (9000h),A; JR $
        _emulator.WriteMemory(CodeAddress, 0x3E, 0x05, 0x32, 0x00, 0x90, 0x18, 0xFE);
        _emulator.Cpu.Registers.PC = CodeAddress;
        _emulator.Cpu.IFF1 = false;

        _handler = new BreakpointHandler(_emulator);
        _handler.BreakpointHit += (_, e) => _hits.Add(e);
    }

    [Fact]
    public void WhenNotEnabled_ShouldNotCheckBreakpoints()
    {
        _handler.BreakpointManager.AddBreakpoint(new RegisterBreakpoint(Register.PC, CodeAddress + 2));

        _emulator.Step();
        _emulator.Step();

        _hits.ShouldBeEmpty();
        _emulator.IsDebuggerBreak.ShouldBeFalse();
    }

    [Fact]
    public void WhenUpdatedWhileDisabled_ShouldStayDisabled()
    {
        var other = TestEmulator.Create();
        other.WriteMemory(CodeAddress, 0x00, 0x00, 0x00);
        other.Cpu.Registers.PC = CodeAddress;
        other.Cpu.IFF1 = false;
        _handler.BreakpointManager.AddBreakpoint(new RegisterBreakpoint(Register.PC, CodeAddress + 1));

        _handler.Update(other);
        other.Step();
        other.Step();

        _hits.ShouldBeEmpty();
    }

    [Fact]
    public void WhenRegisterBreakpointHit_ShouldBreakBeforeInstructionAndReportBreakpoint()
    {
        var breakpoint = new RegisterBreakpoint(Register.PC, CodeAddress + 2);
        _handler.BreakpointManager.AddBreakpoint(breakpoint);
        _handler.IsEnabled = true;

        _emulator.RunFrame();

        _emulator.IsDebuggerBreak.ShouldBeTrue();
        _emulator.Cpu.Registers.PC.ShouldBe((Word)(CodeAddress + 2));
        _hits.Count.ShouldBe(1);
        _hits[0].Breakpoint.ShouldBe(breakpoint);
        _hits[0].PreviousAddress.ShouldBe(CodeAddress);
    }

    [Fact]
    public void WhenMemoryBreakpointHit_ShouldReportBreakpointAndWritingInstruction()
    {
        var breakpoint = new MemoryBreakpoint(0x9000, 5);
        _handler.BreakpointManager.AddBreakpoint(breakpoint);
        _handler.IsEnabled = true;

        _emulator.RunFrame();

        _emulator.IsDebuggerBreak.ShouldBeTrue();
        _emulator.Cpu.Registers.PC.ShouldBe((Word)(CodeAddress + 5));
        _hits.Count.ShouldBe(1);
        _hits[0].Breakpoint.ShouldBe(breakpoint);
        _hits[0].PreviousAddress.ShouldBe((Word)(CodeAddress + 2));
    }

    [Fact]
    public void WhenStepping_ShouldStopAtBreakpointOnNextInstruction()
    {
        var breakpoint = new RegisterBreakpoint(Register.PC, CodeAddress + 2);
        _handler.BreakpointManager.AddBreakpoint(breakpoint);
        _handler.IsEnabled = true;

        _emulator.Step();

        _emulator.Cpu.Registers.PC.ShouldBe((Word)(CodeAddress + 2));
        _hits.Count.ShouldBe(1);

        _emulator.Step();

        _emulator.Cpu.Registers.PC.ShouldBe((Word)(CodeAddress + 5));
        _hits.Count.ShouldBe(1);
    }

    public void Dispose()
    {
        _handler.Dispose();
        GC.SuppressFinalize(this);
    }
}
