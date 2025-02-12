using System.Reactive;
using OldBit.Spectron.Debugger.Breakpoints;
using OldBit.Spectron.Emulation;
using ReactiveUI;

namespace OldBit.Spectron.Debugger.ViewModels;

public class DebuggerViewModel : ReactiveObject, IDisposable
{
    private readonly BreakpointHandler _breakpointHandler;

    private Emulator Emulator { get; }
    private bool _;

    public CodeListViewModel CodeListViewModel { get; }
    public StackViewModel StackViewModel { get; } = new();
    public CpuViewModel CpuViewModel { get; } = new();
    public ImmediateViewModel ImmediateViewModel { get; }
    public BreakpointListViewModel BreakpointListViewModel { get; }

    public ReactiveCommand<Unit, Unit> DebuggerStepCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> DebuggerResumeCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> TogglePauseCommand { get; private set; }

    public DebuggerViewModel(DebuggerContext debuggerContext, Emulator emulator)
    {
        _breakpointHandler = new BreakpointHandler(debuggerContext, emulator);
        _breakpointHandler.BreakpointHit += OnBreakpointHit;

        Emulator = emulator;

        BreakpointListViewModel = new BreakpointListViewModel(debuggerContext);
        CodeListViewModel = new CodeListViewModel(debuggerContext, BreakpointListViewModel);
        ImmediateViewModel = new ImmediateViewModel(debuggerContext, emulator, Refresh);

        DebuggerStepCommand = ReactiveCommand.Create(HandleDebuggerStep);
        DebuggerResumeCommand = ReactiveCommand.Create(HandleDebuggerResume);
        TogglePauseCommand = ReactiveCommand.Create(() => HandlePause(!IsPaused));
    }

    private void OnBreakpointHit(object? sender, EventArgs e)
    {
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
        _breakpointHandler.Resume();

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
        _breakpointHandler.Dispose();
    }
}