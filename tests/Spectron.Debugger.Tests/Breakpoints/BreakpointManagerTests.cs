using OldBit.Spectron.Debugger.Breakpoints;
using OldBit.Spectron.Debugger.Tests.Fixtures;
using OldBit.Spectron.Emulation;

namespace OldBit.Spectron.Debugger.Tests.Breakpoints;

public class BreakpointManagerTests
{
    private readonly Emulator _emulator = TestEmulator.Create();
    private readonly BreakpointManager _manager;
    private readonly List<BreakpointsChangedEventArgs> _changes = [];

    public BreakpointManagerTests()
    {
        _manager = new BreakpointManager(_emulator.Cpu);
        _manager.Changed += (_, e) => _changes.Add(e);
    }

    [Fact]
    public void AddBreakpoint_ShouldAssignUniqueIdsAndRaiseChanged()
    {
        var first = new RegisterBreakpoint(Register.PC, 0x8000);
        var second = new MemoryBreakpoint(0x4000);

        _manager.AddBreakpoint(first);
        _manager.AddBreakpoint(second);

        first.Id.ShouldBe(1);
        second.Id.ShouldBe(2);
        _manager.Breakpoints.ShouldBe([first, second]);
        _changes.Select(e => (e.Change, e.Breakpoint)).ShouldBe([(BreakpointChange.Added, first), (BreakpointChange.Added, second)]);
    }

    [Fact]
    public void AddBreakpoint_WhenAlreadyAdded_ShouldNotRaiseChanged()
    {
        var breakpoint = new RegisterBreakpoint(Register.PC, 0x8000);

        _manager.AddBreakpoint(breakpoint);
        _manager.AddBreakpoint(breakpoint);

        _manager.Breakpoints.Count.ShouldBe(1);
        _changes.Count.ShouldBe(1);
    }

    [Fact]
    public void RemoveBreakpoint_ShouldRaiseChanged()
    {
        var breakpoint = new RegisterBreakpoint(Register.PC, 0x8000);
        _manager.AddBreakpoint(breakpoint);

        _manager.RemoveBreakpoint(breakpoint);
        _manager.RemoveBreakpoint(breakpoint);

        _manager.Breakpoints.ShouldBeEmpty();
        _changes.Select(e => e.Change).ShouldBe([BreakpointChange.Added, BreakpointChange.Removed]);
    }

    [Fact]
    public void UpdateBreakpoint_ShouldKeepIdAndRaiseChanged()
    {
        var original = new RegisterBreakpoint(Register.PC, 0x8000);
        var updated = new RegisterBreakpoint(Register.PC, 0x9000);
        _manager.AddBreakpoint(original);

        _manager.UpdateBreakpoint(original, updated);

        updated.Id.ShouldBe(original.Id);
        _manager.Breakpoints.ShouldBe([updated]);
        _changes[^1].Change.ShouldBe(BreakpointChange.Updated);
        _changes[^1].Original.ShouldBe(original);
        _changes[^1].Breakpoint.ShouldBe(updated);
    }

    [Fact]
    public void SingleUseBreakpoints_ShouldBeHiddenAndNotRaiseChanged()
    {
        _manager.AddSingleUseBreakpoint(0x8000);

        _manager.Breakpoints.ShouldBeEmpty();
        _manager.ContainsBreakpoint(Register.PC, 0x8000).ShouldBeFalse();
        _changes.ShouldBeEmpty();

        _emulator.Cpu.Registers.PC = 0x8000;
        _manager.FindRegisterBreakpointHit().ShouldNotBeNull();

        _emulator.Cpu.Registers.PC = 0x8001;
        _manager.FindRegisterBreakpointHit();
        _emulator.Cpu.Registers.PC = 0x8000;
        _manager.FindRegisterBreakpointHit().ShouldBeNull();
        _changes.ShouldBeEmpty();
    }

    [Fact]
    public void RemoveSingleUseBreakpoints_ShouldKeepUserBreakpoints()
    {
        var breakpoint = new RegisterBreakpoint(Register.PC, 0x9000);
        _manager.AddBreakpoint(breakpoint);
        _manager.AddSingleUseBreakpoint(0x8000);

        _manager.RemoveSingleUseBreakpoints();

        _emulator.Cpu.Registers.PC = 0x8000;
        _manager.FindRegisterBreakpointHit().ShouldBeNull();
        _manager.Breakpoints.ShouldBe([breakpoint]);
    }

    [Fact]
    public void FindRegisterBreakpointHit_ShouldReturnBreakpointWhenRegisterChangesToValue()
    {
        var breakpoint = new RegisterBreakpoint(Register.A, 5);
        _manager.AddBreakpoint(breakpoint);

        _emulator.Cpu.Registers.A = 5;
        _manager.FindRegisterBreakpointHit().ShouldBe(breakpoint);

        // Same value again is not a new hit
        _manager.FindRegisterBreakpointHit().ShouldBeNull();

        _emulator.Cpu.Registers.A = 6;
        _manager.FindRegisterBreakpointHit().ShouldBeNull();

        _emulator.Cpu.Registers.A = 5;
        _manager.FindRegisterBreakpointHit().ShouldBe(breakpoint);
    }

    [Fact]
    public void FindRegisterBreakpointHit_WhenTwoBreakpointsMatch_ShouldHitOnlyOnce()
    {
        var first = new RegisterBreakpoint(Register.PC, 0x8000);
        var second = new RegisterBreakpoint(Register.HL, 0x4000);
        _manager.AddBreakpoint(first);
        _manager.AddBreakpoint(second);

        _emulator.Cpu.Registers.PC = 0x8000;
        _emulator.Cpu.Registers.HL = 0x4000;

        _manager.FindRegisterBreakpointHit().ShouldBe(first);

        // Continuing from the same place must not stop again for the second breakpoint
        _manager.FindRegisterBreakpointHit().ShouldBeNull();
    }

    [Fact]
    public void FindRegisterBreakpointHit_WhenDisabled_ShouldReturnNull()
    {
        _manager.AddBreakpoint(new RegisterBreakpoint(Register.PC, 0x8000) { IsEnabled = false });

        _emulator.Cpu.Registers.PC = 0x8000;

        _manager.FindRegisterBreakpointHit().ShouldBeNull();
    }

    [Fact]
    public void FindMemoryBreakpointHit_ShouldMatchAddressAndValue()
    {
        var anyValue = new MemoryBreakpoint(0x8000);
        var value = new MemoryBreakpoint(0x9000, 0x42);
        _manager.AddBreakpoint(anyValue);
        _manager.AddBreakpoint(value);

        _emulator.WriteMemory(0x8000, 0x01);
        _emulator.WriteMemory(0x9000, 0x41);

        _manager.FindMemoryBreakpointHit(0x8000, _emulator.Memory).ShouldBe(anyValue);
        _manager.FindMemoryBreakpointHit(0x9000, _emulator.Memory).ShouldBeNull();
        _manager.FindMemoryBreakpointHit(0x8001, _emulator.Memory).ShouldBeNull();

        _emulator.WriteMemory(0x9000, 0x42);

        _manager.FindMemoryBreakpointHit(0x9000, _emulator.Memory).ShouldBe(value);
    }

    [Fact]
    public void FindMemoryBreakpointHit_WhenFirstBreakpointValueDoesNotMatch_ShouldCheckNextBreakpoint()
    {
        var first = new MemoryBreakpoint(0x8000, 0x01);
        var second = new MemoryBreakpoint(0x8000, 0x02);
        _manager.AddBreakpoint(first);
        _manager.AddBreakpoint(second);

        _emulator.WriteMemory(0x8000, 0x02);

        _manager.FindMemoryBreakpointHit(0x8000, _emulator.Memory).ShouldBe(second);
    }
}
