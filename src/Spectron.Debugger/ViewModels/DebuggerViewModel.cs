using System.Reactive;
using Avalonia.Threading;
using OldBit.Spectron.Debugger.Breakpoints;
using OldBit.Spectron.Emulation;
using ReactiveUI;

namespace OldBit.Spectron.Debugger.ViewModels;

public class DebuggerViewModel : ReactiveObject, IDisposable
{
    private readonly DebuggerContext _debuggerContext;
    private readonly BreakpointHandler _breakpointHandler;
    private readonly BreakpointManager _breakpointManager;

    private Emulator Emulator { get; }

    public CodeListViewModel CodeListViewModel { get; }
    public StackViewModel StackViewModel { get; } = new();
    public CpuViewModel CpuViewModel { get; } = new();
    public ImmediateViewModel ImmediateViewModel { get; }
    public BreakpointListViewModel BreakpointListViewModel { get; }
    public LoggingViewModel LoggingViewModel { get; }

    public ReactiveCommand<Unit, Unit> DebuggerStepCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> DebuggerResumeCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> TogglePauseCommand { get; private set; }

    public DebuggerViewModel(DebuggerContext debuggerContext, Emulator emulator)
    {
        _debuggerContext = debuggerContext;
        _breakpointManager = new BreakpointManager(emulator.Cpu);

        _breakpointHandler = new BreakpointHandler(_breakpointManager, emulator);
        _breakpointHandler.BreakpointHit += OnBreakpointHit;

        Emulator = emulator;

        BreakpointListViewModel = new BreakpointListViewModel(_debuggerContext, _breakpointManager);
        CodeListViewModel = new CodeListViewModel(_breakpointManager, BreakpointListViewModel);
        ImmediateViewModel = new ImmediateViewModel(debuggerContext, emulator, Refresh);
        LoggingViewModel = new LoggingViewModel(emulator);

        DebuggerStepCommand = ReactiveCommand.Create(HandleDebuggerStep);
        DebuggerResumeCommand = ReactiveCommand.Create(HandleDebuggerResume);
        TogglePauseCommand = ReactiveCommand.Create(() => HandlePause(!IsPaused));

        BreakpointListViewModel.Breakpoints.CollectionChanged += (_, args) =>
            CodeListViewModel.UpdateBreakpoints(args);
    }

    private void OnBreakpointHit(object? sender, EventArgs e)
    {
        Emulator.Pause();
        IsPaused = true;

        Dispatcher.UIThread.Post(Refresh);
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

    private void Close()
    {
        LoggingViewModel.Dispose();

        _debuggerContext.Breakpoints.Clear();

        foreach (var breakpoint in _breakpointManager.Breakpoints)
        {
            _debuggerContext.Breakpoints.Add(breakpoint);
        }
    }

    private void HandleDebuggerStep()
    {
        Emulator.Cpu.Step();
        Refresh();
    }

    private void HandleDebuggerResume()
    {
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
        Close();

        _breakpointHandler.Dispose();
        GC.SuppressFinalize(this);
    }
}