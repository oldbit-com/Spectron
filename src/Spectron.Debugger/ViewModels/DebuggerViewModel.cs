using System.Reactive;
using Avalonia.Threading;
using OldBit.Spectron.Debugger.Breakpoints;
using OldBit.Spectron.Debugger.Settings;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Extensions;
using ReactiveUI;

namespace OldBit.Spectron.Debugger.ViewModels;

public class DebuggerViewModel : ReactiveObject, IDisposable
{
    private readonly DebuggerContext _debuggerContext;
    private readonly DebuggerSettings _debuggerSettings;

    private BreakpointHandler _breakpointHandler = null!;
    private BreakpointManager? _breakpointManager;

    private CodeListViewModel _codeListViewModel= null!;
    private ImmediateViewModel _immediateViewModel = null!;
    private BreakpointListViewModel _breakpointListViewModel = null!;
    private LoggingViewModel _loggingViewModel = new();

    private Emulator Emulator { get; set; } = null!;

    public StackViewModel StackViewModel { get; } = new();
    public CpuViewModel CpuViewModel { get; } = new();
    public MemoryViewModel MemoryViewModel { get; } = new();

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

    public ReactiveCommand<Unit, Unit> DebuggerStepOverCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> DebuggerStepIntoCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> DebuggerStepOutCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> DebuggerResumeCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> TogglePauseCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> ResetCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> HardResetCommand { get; private set; }

    public DebuggerViewModel(DebuggerContext debuggerContext, Emulator emulator, DebuggerSettings debuggerSettings)
    {
        _debuggerContext = debuggerContext;
        _debuggerSettings = debuggerSettings;

        var isPaused = this.WhenAnyValue(x => x.IsPaused, isPaused => isPaused == true);

        DebuggerStepOverCommand = ReactiveCommand.Create(HandleDebuggerStepOver, canExecute: isPaused);
        DebuggerStepIntoCommand = ReactiveCommand.Create(HandleDebuggerStepInto, canExecute: isPaused);
        DebuggerStepOutCommand = ReactiveCommand.Create(HandleStepOutCommand, canExecute: isPaused);
        DebuggerResumeCommand = ReactiveCommand.Create(HandleDebuggerResume, canExecute: isPaused);
        TogglePauseCommand = ReactiveCommand.Create(() => HandlePause(!IsPaused));
        ResetCommand = ReactiveCommand.Create(() => { });
        HardResetCommand = ReactiveCommand.Create(() => { });

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
        ImmediateViewModel = new ImmediateViewModel(_debuggerContext, _debuggerSettings.PreferredNumberFormat, emulator,
            () => Refresh(),
            list => CodeListViewModel.Update(Emulator.Memory, list.Address, emulator.Cpu.Registers.PC, _debuggerSettings));

        LoggingViewModel.Configure(emulator);

        BreakpointListViewModel.Breakpoints.CollectionChanged += (_, args) =>
            CodeListViewModel.UpdateBreakpoints(args);
    }

    private void OnBreakpointHit(object? sender, EventArgs e)
    {
        Emulator.Pause();

        Dispatcher.UIThread.Post(() =>
        {
            IsPaused = true;
            Refresh();
        });
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

    private void HandleDebuggerStepOver()
    {
        var opCode = Emulator.Memory.Read(Emulator.Cpu.Registers.PC);
        var nextOpCode = Emulator.Memory.Read((Word)(Emulator.Cpu.Registers.PC + 1));

        int? returnAddress = null;

        if (IsCallInstruction(opCode) || IsJumpConditionInstruction(opCode))
        {
            returnAddress = Emulator.Cpu.Registers.PC + 3;
        }
        else if (IsDjnzInstruction(opCode) || IsShortJumpConditionInstruction(opCode) ||
                 opCode == 0xED && IsBlockTransferInstruction(nextOpCode))
        {
            returnAddress = Emulator.Cpu.Registers.PC + 2;
        }

        if (returnAddress != null)
        {
            _breakpointManager?.AddBreakpoint(new Breakpoint(Register.PC, returnAddress.Value) { ShouldRemoveOnHit = true });
            HandleDebuggerResume();
        }
        else
        {
            HandleDebuggerStepInto();
        }
    }

    private void HandleDebuggerStepInto()
    {
        Emulator.Cpu.Step();

        if (Emulator.Cpu.Clock.IsFrameComplete)
        {
            Emulator.Cpu.Clock.NewFrame(Emulator.TicksPerFrame);
        }

        Refresh(refreshMemory: false);
    }

    private void HandleStepOutCommand()
    {
        var sp = Emulator.Cpu.Registers.SP;
        var returnAddress = Emulator.Memory.ReadWord(sp);

        _breakpointManager?.AddBreakpoint(new Breakpoint(Register.PC, returnAddress) { ShouldRemoveOnHit = true });
        HandleDebuggerResume();
    }

    private void HandleDebuggerResume()
    {
        Emulator.Resume(isDebuggerResume: true);
        IsPaused = false;
    }

    private void Refresh(bool refreshMemory = true)
    {
        RefreshCpu();
        RefreshStack();
        RefreshCode();

        if (refreshMemory)
        {
            RefreshMemory();
        }
    }

    private void RefreshCpu() => CpuViewModel.Update(Emulator.Cpu);

    private void RefreshStack() => StackViewModel.Update(Emulator.Memory, Emulator.Cpu.Registers.SP);

    private void RefreshCode() => CodeListViewModel.Update(Emulator.Memory, Emulator.Cpu.Registers.PC, Emulator.Cpu.Registers.PC, _debuggerSettings);

    private void RefreshMemory() => MemoryViewModel.Update(Emulator.Memory);

    private static bool IsCallInstruction(byte opCode) => opCode == 0xCD || (opCode & 0b1100_0111) == 0b1100_0100;

    private static bool IsDjnzInstruction(byte opCode) => opCode == 0x10;

    private static bool IsJumpConditionInstruction(byte opCode) => (opCode & 0b1100_0111) == 0b1100_0010;

    private static bool IsShortJumpConditionInstruction(byte opCode) => opCode is 0x20 or 0x28 or 0x30 or 0x38;

    private static bool IsBlockTransferInstruction(byte opCode) => opCode is 0xB0 or 0xB8;

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