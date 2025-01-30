using System;
using System.Collections.Generic;
using System.Reactive;
using OldBit.Z80Cpu.Registers;
using ReactiveUI;

namespace OldBit.Spectron.ViewModels.Debugger;

public class DebuggerViewModel : ViewModelBase, IDisposable
{
    private readonly List<IDisposable> _disposables = [];

    public MainWindowViewModel MainWindowViewModel { get; private  set; }

    public DebuggerCodeListViewModel CodeListViewModel { get; private set; } = new();

    public DebuggerStackViewModel StackViewModel { get; private set; } = new();

    public DebuggerCpuViewModel CpuViewModel { get; private set; } = new();

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
        RefreshCpu();
        RefreshStack();
        RefreshCode();
    }

    private void RefreshCpu() =>
        CpuViewModel.Update(MainWindowViewModel.Emulator!.Cpu);

    private void RefreshStack() =>
        StackViewModel.Update(MainWindowViewModel.Emulator!.Memory,
        MainWindowViewModel.Emulator!.Cpu.Registers.SP);

    private void RefreshCode() =>
        CodeListViewModel.Update(MainWindowViewModel.Emulator!.Memory,
            MainWindowViewModel.Emulator!.Cpu.Registers.PC);

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        foreach (var disposable in _disposables)
        {
            disposable.Dispose();
        }
    }
}