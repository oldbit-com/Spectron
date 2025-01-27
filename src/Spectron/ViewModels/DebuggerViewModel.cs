using System;
using System.Collections.Generic;
using System.Reactive;
using OldBit.Z80Cpu.Registers;
using ReactiveUI;

namespace OldBit.Spectron.ViewModels;

public class DebuggerViewModel : ViewModelBase, IDisposable
{
    private readonly List<IDisposable> _disposables = [];

    public MainWindowViewModel MainWindowViewModel { get; private  set; }

    public DebuggerStackViewModel StackViewModel { get; private set; } = new();

    public ReactiveCommand<Unit, Unit> DebuggerStepCommand { get; private set; }

    public DebuggerViewModel(MainWindowViewModel mainWindowViewModel)
    {
        MainWindowViewModel = mainWindowViewModel;

        DebuggerStepCommand = ReactiveCommand.Create(HandleDebuggerStep);

        var disposable = MainWindowViewModel
            .WhenAny(x => x.IsPaused, x => x.Value)
            .Subscribe(y => HandlePause());

        _disposables.Add(disposable);
    }

    private void HandlePause()
    {
        if (MainWindowViewModel.IsPaused)
        {
            Refresh();
        }
    }

    private void HandleDebuggerStep()
    {
        MainWindowViewModel.Emulator!.Cpu.Step();
        Refresh();
    }

    private void Refresh()
    {
        RefreshRegisters();
        RefreshStack();
    }

    private void RefreshRegisters()
    {
        AF = MainWindowViewModel.Emulator!.Cpu.Registers.AF;
        AFPrime = MainWindowViewModel.Emulator!.Cpu.Registers.Prime.AF;
        BC = MainWindowViewModel.Emulator!.Cpu.Registers.BC;
        BCPrime = MainWindowViewModel.Emulator!.Cpu.Registers.Prime.BC;
        DE = MainWindowViewModel.Emulator!.Cpu.Registers.DE;
        DEPrime = MainWindowViewModel.Emulator!.Cpu.Registers.Prime.DE;
        HL = MainWindowViewModel.Emulator!.Cpu.Registers.HL;
        HLPrime = MainWindowViewModel.Emulator!.Cpu.Registers.Prime.HL;
        IX = MainWindowViewModel.Emulator!.Cpu.Registers.IX;
        IY = MainWindowViewModel.Emulator!.Cpu.Registers.IY;

        F = MainWindowViewModel.Emulator!.Cpu.Registers.F;
        I = MainWindowViewModel.Emulator!.Cpu.Registers.I;
        R = MainWindowViewModel.Emulator!.Cpu.Registers.R;

        PC = MainWindowViewModel.Emulator!.Cpu.Registers.PC;
    }

    private void RefreshStack() =>
        StackViewModel.Update(MainWindowViewModel.Emulator!.Memory,
        MainWindowViewModel.Emulator!.Cpu.Registers.SP);

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

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        foreach (var disposable in _disposables)
        {
            disposable.Dispose();
        }
    }
}