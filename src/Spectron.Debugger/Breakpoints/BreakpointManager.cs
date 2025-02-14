using OldBit.Z80Cpu;

namespace OldBit.Spectron.Debugger.Breakpoints;

public class BreakpointManager
{
    private readonly List<Breakpoint> _breakpoints = [];
    private readonly Func<Word>[] _registerValueIndex = new Func<Word>[(int)Register.Last];

    public BreakpointManager(Z80 cpu)
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

    public void UpdateBreakpoint(Guid id, Breakpoint breakpoint)
    {
        var existing = _breakpoints.FirstOrDefault(x => x.Id == id);

        if (existing == null)
        {
            return;
        }

        existing.IsEnabled = breakpoint.IsEnabled;
        existing.Register = breakpoint.Register;
        existing.Value = breakpoint.Value;
    }

    public void AddBreakpoint(Breakpoint breakpoint)
    {
        var index = _breakpoints.FindIndex(x => x.Id == breakpoint.Id);

        if (index < 0)
        {
            _breakpoints.Add(breakpoint);
        }
    }

    public void RemoveBreakpoint(Guid id)
    {
        var index = _breakpoints.FindIndex(x => x.Id == id);

        if (index >= 0)
        {
            _breakpoints.RemoveAt(index);
        }
    }

    public bool HasBreakpoint(Register register, Word address) =>
        _breakpoints.Any(x => x.Register == register && x.Value == address);

    public bool CheckHit()
    {
        foreach (var breakpoint in _breakpoints)
        {
            if (!breakpoint.IsEnabled)
            {
                continue;
            }

            var registerValue = _registerValueIndex[(int)breakpoint.Register]();

            if (registerValue == breakpoint.Value)
            {
                return true;
            }
        }

        return false;
    }
}