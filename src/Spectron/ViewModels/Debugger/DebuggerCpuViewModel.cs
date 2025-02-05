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
        SP = cpu.Registers.SP;

        IFF1 = cpu.IFF1 ? "1" : "0";
        IFF2 = cpu.IFF2 ? "1" : "0";
        IM = ((int)cpu.IM).ToString();
    }

    private Word _af;
    public Word AF
    {
        get => _af;
        set => this.RaiseAndSetIfChanged(ref _af, value);
    }

    private Word _afPrime;
    public Word AFPrime
    {
        get => _afPrime;
        set => this.RaiseAndSetIfChanged(ref _afPrime, value);
    }

    private Word _bc;
    public Word BC
    {
        get => _bc;
        set => this.RaiseAndSetIfChanged(ref _bc, value);
    }

    private Word _bcPrime;
    public Word BCPrime
    {
        get => _bcPrime;
        set => this.RaiseAndSetIfChanged(ref _bcPrime, value);
    }

    private Word _de;
    public Word DE
    {
        get => _de;
        set => this.RaiseAndSetIfChanged(ref _de, value);
    }

    private Word _dePrime;
    public Word DEPrime
    {
        get => _dePrime;
        set => this.RaiseAndSetIfChanged(ref _dePrime, value);
    }

    private Word _hl;
    public Word HL
    {
        get => _hl;
        set => this.RaiseAndSetIfChanged(ref _hl, value);
    }

    private Word _hlPrime;
    public Word HLPrime
    {
        get => _hlPrime;
        set => this.RaiseAndSetIfChanged(ref _hlPrime, value);
    }

    private Word _ix;
    public Word IX
    {
        get => _ix;
        set => this.RaiseAndSetIfChanged(ref _ix, value);
    }

    private Word _iy;
    public Word IY
    {
        get => _iy;
        set => this.RaiseAndSetIfChanged(ref _iy, value);
    }

    private Word _pc;
    public Word PC
    {
        get => _pc;
        set => this.RaiseAndSetIfChanged(ref _pc, value);
    }

    private Word _sp;
    public Word SP
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

    private string _iff1 = string.Empty;
    public string IFF1
    {
        get => _iff1;
        set => this.RaiseAndSetIfChanged(ref _iff1, value);
    }

    private string _iff2 = string.Empty;
    public string IFF2
    {
        get => _iff2;
        set => this.RaiseAndSetIfChanged(ref _iff2, value);
    }

    private string _im = string.Empty;
    public string IM
    {
        get => _im;
        set => this.RaiseAndSetIfChanged(ref _im, value);
    }
}