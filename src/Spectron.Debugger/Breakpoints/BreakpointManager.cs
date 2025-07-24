using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Debugger.Breakpoints;

public class BreakpointManager
{
    private readonly BreakpointList  _breakpoints = new();
    private readonly Func<Word>[] _registerValueIndex = new Func<Word>[(int)Register.Last];

    public IReadOnlyList<Breakpoint> Breakpoints => _breakpoints.Breakpoints;

    public BreakpointManager(Z80 cpu) => CreateRegistersIndex(cpu);

    public void UpdateBreakpoint(Breakpoint original, Breakpoint updated) =>
        _breakpoints.Replace(original, updated);

    public void Update(Z80 cpu) => CreateRegistersIndex(cpu);

    public void AddBreakpoint(Breakpoint breakpoint) =>
        _breakpoints.AddIfNotExists(breakpoint);

    public void AddSingleUseBreakpoint(Word address)
    {
        var breakpoint = new RegisterBreakpoint(Register.PC, address) { IsSingleUse = true };

        _breakpoints.Add(breakpoint);
    }

    public void RemoveBreakpoint(Breakpoint breakpoint) => _breakpoints.Remove(breakpoint);

    public bool ContainsBreakpoint(Register register, Word address) => _breakpoints.Register
        .Any(breakpoint => breakpoint.Register == register && breakpoint.Value == address && !breakpoint.IsSingleUse);

    public bool IsRegisterBreakpointHit()
    {
        // ReSharper disable once ForCanBeConvertedToForeach - foreach in this case is not memory efficient
        for (var index = 0; index < _breakpoints.Register.Count; index++)
        {
            var breakpoint = _breakpoints.Register[index];

            if (!breakpoint.IsEnabled)
            {
                continue;
            }

            var value = _registerValueIndex[(int)breakpoint.Register]();

            if (breakpoint.Value == value && breakpoint.LastValue != value)
            {
                breakpoint.LastValue = value;

                if (breakpoint.IsSingleUse)
                {
                    RemoveBreakpoint(breakpoint);
                }

                return true;
            }

            breakpoint.LastValue = value;
        }

        return false;
    }

    public bool IsMemoryBreakpointHit(Word address, IMemory memory)
    {
        // ReSharper disable once ForCanBeConvertedToForeach - foreach in this case is not memory efficient
        for (var index = 0; index < _breakpoints.Memory.Count; index++)
        {
            var breakpoint = _breakpoints.Memory[index];

            if (!breakpoint.IsEnabled || breakpoint.Address != address)
            {
                continue;
            }

            if (breakpoint.Value != null)
            {
                return breakpoint.Value == memory.Read(address);
            }

            return true;
        }

        return false;
    }

    private void CreateRegistersIndex(Z80 cpu)
    {
        _registerValueIndex[(int)Register.A] = () => cpu.Registers.A;
        _registerValueIndex[(int)Register.B] = () => cpu.Registers.B;
        _registerValueIndex[(int)Register.C] = () => cpu.Registers.C;
        _registerValueIndex[(int)Register.D] = () => cpu.Registers.D;
        _registerValueIndex[(int)Register.E] = () => cpu.Registers.E;
        _registerValueIndex[(int)Register.H] = () => cpu.Registers.H;
        _registerValueIndex[(int)Register.L] = () => cpu.Registers.L;
        _registerValueIndex[(int)Register.IXH] = () => cpu.Registers.IXH;
        _registerValueIndex[(int)Register.IXL] = () => cpu.Registers.IXL;
        _registerValueIndex[(int)Register.IYH] = () => cpu.Registers.IYH;
        _registerValueIndex[(int)Register.IYL] = () => cpu.Registers.IYL;
        _registerValueIndex[(int)Register.AF] = () => cpu.Registers.AF;
        _registerValueIndex[(int)Register.AFPrime] = () => cpu.Registers.Prime.AF;
        _registerValueIndex[(int)Register.BC] = () => cpu.Registers.BC;
        _registerValueIndex[(int)Register.BCPrime] = () => cpu.Registers.Prime.BC;
        _registerValueIndex[(int)Register.DE] = () => cpu.Registers.DE;
        _registerValueIndex[(int)Register.DEPrime] = () => cpu.Registers.Prime.DE;
        _registerValueIndex[(int)Register.HL] = () => cpu.Registers.HL;
        _registerValueIndex[(int)Register.HLPrime] = () => cpu.Registers.Prime.HL;
        _registerValueIndex[(int)Register.SP] = () => cpu.Registers.SP;
        _registerValueIndex[(int)Register.PC] = () => cpu.Registers.PC;
        _registerValueIndex[(int)Register.IX] = () => cpu.Registers.IX;
        _registerValueIndex[(int)Register.IY] = () => cpu.Registers.IY;
    }
}