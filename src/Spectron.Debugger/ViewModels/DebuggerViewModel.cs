using System.Reactive;
using OldBit.Spectron.Emulation;
using OldBit.Z80Cpu.Events;
using ReactiveUI;

namespace OldBit.Spectron.Debugger.ViewModels;

public class DebuggerViewModel : ReactiveObject, IDisposable
{
    private readonly DebuggerContext _debuggerContext;

    private Emulator Emulator { get; }
    private Word? _skipDebuggerBreakpointAddress = null;

    public CodeListViewModel CodeListViewModel { get; }
    public StackViewModel StackViewModel { get; } = new();
    public CpuViewModel CpuViewModel { get; } = new();
    public ImmediateViewModel ImmediateViewModel { get; }

    public ReactiveCommand<Unit, Unit> DebuggerStepCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> DebuggerResumeCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> TogglePauseCommand { get; private set; }

    public DebuggerViewModel(DebuggerContext debuggerContext, Emulator emulator)
    {
        _debuggerContext = debuggerContext;
        Emulator = emulator;

        CodeListViewModel = new CodeListViewModel(debuggerContext);
        ImmediateViewModel = new ImmediateViewModel(debuggerContext, emulator, Refresh);

        DebuggerStepCommand = ReactiveCommand.Create(HandleDebuggerStep);
        DebuggerResumeCommand = ReactiveCommand.Create(HandleDebuggerResume);
        TogglePauseCommand = ReactiveCommand.Create(() => HandlePause(!IsPaused));

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
        IsPaused = true;

        Refresh();
    }

    public void HandlePause(bool isPaused)
    {
        IsPaused = isPaused;

        if (isPaused)
        {
            Emulator.Pause();
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
        IsPaused = false;
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

    private bool _isPaused;
    public bool IsPaused
    {
        get => _isPaused;
        set => this.RaiseAndSetIfChanged(ref _isPaused, value);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        Emulator.Cpu.BeforeInstruction -= BeforeInstruction;
    }
}