using System.Reactive;
using Avalonia.Threading;
using OldBit.Spectron.Debugger.Breakpoints;
using OldBit.Spectron.Emulation;
using ReactiveUI;

namespace OldBit.Spectron.Debugger.ViewModels;

public class DebuggerViewModel : ReactiveObject, IDisposable
{
    private readonly DebuggerContext _debuggerContext;

    private BreakpointHandler _breakpointHandler = null!;
    private BreakpointManager? _breakpointManager;

    private CodeListViewModel _codeListViewModel= null!;
    private ImmediateViewModel _immediateViewModel = null!;
    private BreakpointListViewModel _breakpointListViewModel = null!;
    private LoggingViewModel _loggingViewModel = null!;

    private Emulator Emulator { get; set; } = null!;

    public StackViewModel StackViewModel { get; } = new();
    public CpuViewModel CpuViewModel { get; } = new();

    public CodeListViewModel CodeListViewModel
    {
        get => _codeListViewModel;
        set => this.RaiseAndSetIfChanged(ref _codeListViewModel, value);
    }

    public ImmediateViewModel ImmediateViewModel
    {
        get => _immediateViewModel;
        set => this.RaiseAndSetIfChanged(ref _immediateViewModel, value);
    }

    public BreakpointListViewModel BreakpointListViewModel
    {
        get => _breakpointListViewModel;
        set => this.RaiseAndSetIfChanged(ref _breakpointListViewModel, value);
    }

    public LoggingViewModel LoggingViewModel
    {
        get => _loggingViewModel;
        set => this.RaiseAndSetIfChanged(ref _loggingViewModel, value);
    }

    public ReactiveCommand<Unit, Unit> DebuggerStepCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> DebuggerResumeCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> TogglePauseCommand { get; private set; }

    public DebuggerViewModel(DebuggerContext debuggerContext, Emulator emulator)
    {
        _debuggerContext = debuggerContext;

        DebuggerStepCommand = ReactiveCommand.Create(HandleDebuggerStep);
        DebuggerResumeCommand = ReactiveCommand.Create(HandleDebuggerResume);
        TogglePauseCommand = ReactiveCommand.Create(() => HandlePause(!IsPaused));

        ConfigureEmulator(emulator);
    }

    public void ConfigureEmulator(Emulator emulator)
    {
        Emulator = emulator;

        UpdateContextBreakpoints();

        _breakpointManager = new BreakpointManager(emulator.Cpu);

        _breakpointHandler = new BreakpointHandler(_breakpointManager, emulator.Cpu);
        _breakpointHandler.BreakpointHit += OnBreakpointHit;

        BreakpointListViewModel = new BreakpointListViewModel(_debuggerContext, _breakpointManager);
        CodeListViewModel = new CodeListViewModel(_breakpointManager, BreakpointListViewModel);
        ImmediateViewModel = new ImmediateViewModel(_debuggerContext, emulator, Refresh);
        LoggingViewModel = new LoggingViewModel(emulator);

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

        UpdateContextBreakpoints();

        _breakpointHandler.Dispose();
    }

    private void UpdateContextBreakpoints()
    {
        _debuggerContext.Breakpoints.Clear();

        if (_breakpointManager == null)
        {
            return;
        }

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

        GC.SuppressFinalize(this);
    }
}