using System;
using System.Collections.Generic;
using System.Reactive;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Debugger;
using OldBit.Z80Cpu.Events;
using ReactiveUI;

namespace OldBit.Spectron.ViewModels.Debugger;

public class DebuggerViewModel : ViewModelBase, IDisposable
{
    private readonly DebuggerContext _debuggerContext;
    private readonly List<IDisposable> _disposables = [];

    private Emulator Emulator => MainWindowViewModel.Emulator!;
    private Word? _skipDebuggerBreakpointAddress = null;

    public MainWindowViewModel MainWindowViewModel { get; }
    public DebuggerCodeListViewModel CodeListViewModel { get; }
    public DebuggerStackViewModel StackViewModel { get; } = new();
    public DebuggerCpuViewModel CpuViewModel { get; } = new();
    public DebuggerImmediateViewModel ImmediateViewModel { get; }

    public ReactiveCommand<Unit, Unit> DebuggerStepCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> DebuggerResumeCommand { get; private set; }

    public DebuggerViewModel(
        MainWindowViewModel mainWindowViewModel,
        DebuggerContext debuggerContext)
    {
        _debuggerContext = debuggerContext;
        MainWindowViewModel = mainWindowViewModel;

        CodeListViewModel = new DebuggerCodeListViewModel(debuggerContext);
        ImmediateViewModel = new DebuggerImmediateViewModel(debuggerContext);

        DebuggerStepCommand = ReactiveCommand.Create(HandleDebuggerStep);
        DebuggerResumeCommand = ReactiveCommand.Create(HandleDebuggerResume);

        var disposable = mainWindowViewModel
            .WhenAny(x => x.IsPaused, x => x.Value)
            .Subscribe(y => HandlePause());

        _disposables.Add(disposable);

        Emulator.Cpu.BeforeInstruction += BeforeInstruction;
    }

    private void BeforeInstruction(BeforeInstructionEventArgs e)
    {
        if (_skipDebuggerBreakpointAddress == e.PC)
        {
            _skipDebuggerBreakpointAddress = null;

            return;
        }

        if (!_debuggerContext.HasBreakpoint(e.PC))
        {
            return;
        }

        e.IsBreakpoint = true;

        Emulator.Pause();
        MainWindowViewModel.IsPaused = true;

        Refresh();
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
        Emulator.Cpu.Step();
        Refresh();
    }

    private void HandleDebuggerResume()
    {
        // Prevent the breakpoint from being hit again for current instruction
        _skipDebuggerBreakpointAddress = Emulator.Cpu.Registers.PC;

        Emulator.Resume();
        MainWindowViewModel.IsPaused = false;
    }

    private void Refresh()
    {
        RefreshCpu();
        RefreshStack();
        RefreshCode();
    }

    private void RefreshCpu() => CpuViewModel.Update(Emulator.Cpu);

    private void RefreshStack() => StackViewModel.Update(Emulator.Memory, Emulator.Cpu.Registers.SP);

    private void RefreshCode() => CodeListViewModel.Update(Emulator.Memory, Emulator.Cpu.Registers.PC);

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        Emulator.Cpu.BeforeInstruction -= BeforeInstruction;

        foreach (var disposable in _disposables)
        {
            disposable.Dispose();
        }

        _disposables.Clear();
    }
}