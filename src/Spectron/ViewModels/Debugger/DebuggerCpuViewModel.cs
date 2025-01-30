using OldBit.Z80Cpu;
using OldBit.Z80Cpu.Registers;
using ReactiveUI;

namespace OldBit.Spectron.ViewModels.Debugger;

public class DebuggerCpuViewModel : ViewModelBase
{
    public void Update(Z80 cpu)
    {
        AF = cpu.Registers.AF;
        AFPrime = cpu.Registers.Prime.AF;
        BC = cpu.Registers.BC;
        BCPrime = cpu.Registers.Prime.BC;
        DE = cpu.Registers.DE;
        DEPrime = cpu.Registers.Prime.DE;
        HL = cpu.Registers.HL;
        HLPrime = cpu.Registers.Prime.HL;
        IX = cpu.Registers.IX;
        IY = cpu.Registers.IY;

        F = cpu.Registers.F;
        I = cpu.Registers.I;
        R = cpu.Registers.R;

        PC = cpu.Registers.PC;
    }


    private ushort _af;
    public ushort AF
    {
        get => _af;
        set => this.RaiseAndSetIfChanged(ref _af, value);
    }

    private ushort _afPrime;
    public ushort AFPrime
    {
        get => _afPrime;
        set => this.RaiseAndSetIfChanged(ref _afPrime, value);
    }

    private ushort _bc;
    public ushort BC
    {
        get => _bc;
        set => this.RaiseAndSetIfChanged(ref _bc, value);
    }

    private ushort _bcPrime;
    public ushort BCPrime
    {
        get => _bcPrime;
        set => this.RaiseAndSetIfChanged(ref _bcPrime, value);
    }

    private ushort _de;
    public ushort DE
    {
        get => _de;
        set => this.RaiseAndSetIfChanged(ref _de, value);
    }

    private ushort _dePrime;
    public ushort DEPrime
    {
        get => _dePrime;
        set => this.RaiseAndSetIfChanged(ref _dePrime, value);
    }

    private ushort _hl;
    public ushort HL
    {
        get => _hl;
        set => this.RaiseAndSetIfChanged(ref _hl, value);
    }

    private ushort _hlPrime;
    public ushort HLPrime
    {
        get => _hlPrime;
        set => this.RaiseAndSetIfChanged(ref _hlPrime, value);
    }

    private ushort _ix;
    public ushort IX
    {
        get => _ix;
        set => this.RaiseAndSetIfChanged(ref _ix, value);
    }

    private ushort _iy;
    public ushort IY
    {
        get => _iy;
        set => this.RaiseAndSetIfChanged(ref _iy, value);
    }

    private ushort _pc;
    public ushort PC
    {
        get => _pc;
        set => this.RaiseAndSetIfChanged(ref _pc, value);
    }

    private ushort _sp;
    public ushort SP
    {
        get => _sp;
        set => this.RaiseAndSetIfChanged(ref _sp, value);
    }

    private byte _i;
    public byte I
    {
        get => _i;
        set => this.RaiseAndSetIfChanged(ref _i, value);
    }

    private Flags _f;
    public Flags F
    {
        get => _f;
        set => this.RaiseAndSetIfChanged(ref _f, value);
    }

    private byte _r;
    public byte R
    {
        get => _r;
        set => this.RaiseAndSetIfChanged(ref _r, value);
    }
}