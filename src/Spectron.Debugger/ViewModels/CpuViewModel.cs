using CommunityToolkit.Mvvm.ComponentModel;
using OldBit.Z80Cpu;
using OldBit.Z80Cpu.Registers;

namespace OldBit.Spectron.Debugger.ViewModels;

// ReSharper disable InconsistentNaming
public partial class CpuViewModel : ObservableObject
{
    [ObservableProperty]
    private Word _AF;

    [ObservableProperty]
    private Word _AFPrime;

    [ObservableProperty]
    private Word _BC;

    [ObservableProperty]
    private Word _BCPrime;

    [ObservableProperty]
    private Word _DE;

    [ObservableProperty]
    private Word _DEPrime;

    [ObservableProperty]
    private Word _HL;

    [ObservableProperty]
    private Word _HLPrime;

    [ObservableProperty]
    private Word _IX;

    [ObservableProperty]
    private Word _IY;

    [ObservableProperty]
    private Word _PC;

    [ObservableProperty]
    private Word _SP;

    [ObservableProperty]
    private byte _I;

    [ObservableProperty]
    private Flags _F;

    [ObservableProperty]
    private byte _R;

    [ObservableProperty]
    private string _IFF1 = string.Empty;

    [ObservableProperty]
    private string _IFF2 = string.Empty;

    [ObservableProperty]
    private string _IM = string.Empty;

    [ObservableProperty]
    private int _T;

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