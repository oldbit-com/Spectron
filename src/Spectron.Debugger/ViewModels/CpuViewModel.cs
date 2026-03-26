using CommunityToolkit.Mvvm.ComponentModel;
using OldBit.Z80Cpu;
using OldBit.Z80Cpu.Registers;

namespace OldBit.Spectron.Debugger.ViewModels;

// ReSharper disable InconsistentNaming
public partial class CpuViewModel : ObservableObject
{
    [ObservableProperty]
    public partial Word AF { get; set; }

    [ObservableProperty]
    public partial Word AFPrime { get; set; }

    [ObservableProperty]
    public partial Word BC { get; set; }

    [ObservableProperty]
    public partial Word BCPrime { get; set; }

    [ObservableProperty]
    public partial Word DE { get; set; }

    [ObservableProperty]
    public partial Word DEPrime { get; set; }

    [ObservableProperty]
    public partial Word HL { get; set; }

    [ObservableProperty]
    public partial Word HLPrime { get; set; }

    [ObservableProperty]
    public partial Word IX { get; set; }

    [ObservableProperty]
    public partial Word IY { get; set; }

    [ObservableProperty]
    public partial Word PC { get; set; }

    [ObservableProperty]
    public partial Word SP { get; set; }

    [ObservableProperty]
    public partial byte I { get; set; }

    [ObservableProperty]
    public partial Flags F { get; set; }

    [ObservableProperty]
    public partial byte R { get; set; }

    [ObservableProperty]
    public partial string IFF1 { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string IFF2 { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string IM { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int T { get; set; }

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

        T = cpu.Clock.FrameTicks;
    }
}