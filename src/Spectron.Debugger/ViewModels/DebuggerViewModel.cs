using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OldBit.Spectron.Debugger.Breakpoints;
using OldBit.Spectron.Debugger.Messages;
using OldBit.Spectron.Debugger.Settings;
using OldBit.Spectron.Debugger.Stepping;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Extensions;

namespace OldBit.Spectron.Debugger.ViewModels;

public partial class DebuggerViewModel : ObservableObject, IDisposable
{
    private readonly DebuggerContext _debuggerContext;
    private readonly DebuggerSettings _debuggerSettings;
    private readonly BreakpointHandler _breakpointHandler;

    private Emulator Emulator { get; set; } = null!;

    public StackViewModel StackViewModel { get; } = new();
    public CpuViewModel CpuViewModel { get; } = new();
    public MemoryViewModel MemoryViewModel { get; private set; } = null!;

    [ObservableProperty]
    public partial CodeListViewModel CodeListViewModel { get; set; } = null!;

    [ObservableProperty]
    public partial ImmediateViewModel ImmediateViewModel { get; set; } = null!;

    [ObservableProperty]
    public partial BreakpointListViewModel BreakpointListViewModel { get; set; } = null!;

    [ObservableProperty]
    public partial LoggingViewModel LoggingViewModel { get; set; } = new();

    [ObservableProperty]
    public partial bool IsPaused { get; set; }

    public DebuggerViewModel(
        Emulator emulator,
        DebuggerContext debuggerContext,
        DebuggerSettings debuggerSettings,
        BreakpointHandler breakpointHandler,
        BreakpointHitEventArgs? breakpointHitEventArgs)
    {
        _debuggerContext = debuggerContext;
        _debuggerSettings = debuggerSettings;
        _breakpointHandler = breakpointHandler;

        ConfigureEmulator(emulator, breakpointHitEventArgs);
    }

    public void ConfigureEmulator(Emulator emulator, BreakpointHitEventArgs? breakpointHitEventArgs = null)
    {
        Emulator = emulator;

        _breakpointHandler.BreakpointHit -= OnBreakpointHit;
        _breakpointHandler.BreakpointHit += OnBreakpointHit;

        MemoryViewModel = new MemoryViewModel(Emulator);

        // Null on the first call from the constructor
        BreakpointListViewModel?.Dispose();
        BreakpointListViewModel = new BreakpointListViewModel(_breakpointHandler.BreakpointManager, _debuggerSettings.NumberFormat);
        CodeListViewModel = new CodeListViewModel(_breakpointHandler.BreakpointManager);
        ImmediateViewModel = new ImmediateViewModel(_debuggerContext, _debuggerSettings.NumberFormat, emulator,
            () => Refresh(),
            list => CodeListViewModel.Update(
                Emulator.Memory,
                list.Address,
                Emulator.Cpu.Registers.PC,
                _debuggerSettings,
                breakpointHitEventArgs));

        LoggingViewModel.Configure(Emulator);

        HandlePause(Emulator.IsPaused, breakpointHitEventArgs);
    }

    private void OnBreakpointHit(object? sender, BreakpointHitEventArgs e)
    {
        Emulator.Pause();

        // A step over or out stopped somewhere else, so its single-use breakpoint is no longer wanted
        _breakpointHandler.BreakpointManager.RemoveSingleUseBreakpoints();

        Dispatcher.UIThread.Post(() =>
        {
            IsPaused = true;

            Refresh(breakpointHitEventArgs: e);
        });
    }

    public void HandlePause(bool isPaused, BreakpointHitEventArgs? breakpointHitEventArgs = null)
    {
        IsPaused = isPaused;

        if (!isPaused)
        {
            return;
        }

        WeakReferenceMessenger.Default.Send(new PauseForDebugMessage());

        Emulator.Pause();
        _breakpointHandler.BreakpointManager.RemoveSingleUseBreakpoints();

        Refresh(breakpointHitEventArgs: breakpointHitEventArgs);
    }

    [RelayCommand(CanExecute = nameof(CanExecuteStepCommands))]
    private void DebuggerStepOver()
    {
        var returnAddress = StepOver.GetReturnAddress(Emulator.Memory, Emulator.Cpu.Registers.PC);

        if (returnAddress != null)
        {
            _breakpointHandler.BreakpointManager.AddSingleUseBreakpoint(returnAddress.Value);
            DebuggerResume();
        }
        else
        {
            DebuggerStepInto();
        }
    }

    [RelayCommand(CanExecute = nameof(CanExecuteStepCommands))]
    private void DebuggerStepInto()
    {
        Emulator.Step();

        Refresh(refreshMemory: false);
    }

    [RelayCommand(CanExecute = nameof(CanExecuteStepCommands))]
    private void DebuggerStepOut()
    {
        var sp = Emulator.Cpu.Registers.SP;
        var returnAddress = Emulator.Memory.ReadWord(sp);

        _breakpointHandler.BreakpointManager.AddSingleUseBreakpoint(returnAddress);

        DebuggerResume();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteStepCommands))]
    private void DebuggerResume()
    {
        WeakReferenceMessenger.Default.Send(new ResumeFromDebugMessage());

        IsPaused = false;
    }

    [RelayCommand]
    private void TogglePause() => HandlePause(!IsPaused);

    [RelayCommand]
    private static void Reset() => WeakReferenceMessenger.Default.Send(new ResetEmulatorMessage());

    [RelayCommand]
    private static void HardReset() => WeakReferenceMessenger.Default.Send(new ResetEmulatorMessage(HardReset: true));

    private bool CanExecuteStepCommands() => IsPaused;

    private void Close()
    {
        LoggingViewModel.Dispose();
        MemoryViewModel.Dispose();
        BreakpointListViewModel.Dispose();

        _breakpointHandler.BreakpointHit -= OnBreakpointHit;
    }

    /// <summary>
    /// Refreshes the registers, stack, code and memory views, e.g. after an MCP tool changed the machine state.
    /// Must be called on the UI thread.
    /// </summary>
    public void Refresh(bool refreshMemory = true, BreakpointHitEventArgs? breakpointHitEventArgs = null)
    {
        RefreshCpu();
        RefreshStack();
        RefreshCode(breakpointHitEventArgs);

        if (refreshMemory)
        {
            RefreshMemory();
        }
    }

    private void RefreshCpu() => CpuViewModel.Update(Emulator.Cpu);

    private void RefreshStack() => StackViewModel.Update(Emulator.Memory, Emulator.Cpu.Registers.SP);

    private void RefreshCode(BreakpointHitEventArgs? breakpointHitEventArgs = null) => CodeListViewModel.Update(
        Emulator.Memory,
        Emulator.Cpu.Registers.PC,
        Emulator.Cpu.Registers.PC,
        _debuggerSettings,
        breakpointHitEventArgs);

    private void RefreshMemory() => MemoryViewModel.Update(Emulator.Memory);

    public void Dispose()
    {
        Close();

        GC.SuppressFinalize(this);
    }
}