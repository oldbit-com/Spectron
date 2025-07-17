using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OldBit.Spectron.Debugger.Breakpoints;
using OldBit.Spectron.Debugger.Messages;
using OldBit.Spectron.Debugger.Settings;
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
    public MemoryViewModel MemoryViewModel { get; } = new();

    [ObservableProperty]
    private CodeListViewModel _codeListViewModel = null!;

    [ObservableProperty]
    private ImmediateViewModel _immediateViewModel = null!;

    [ObservableProperty]
    private BreakpointListViewModel _breakpointListViewModel = null!;

    [ObservableProperty]
    private LoggingViewModel _loggingViewModel = new();

    [ObservableProperty]
    private bool _isPaused;

    public DebuggerViewModel(
        Emulator emulator,
        DebuggerContext debuggerContext,
        DebuggerSettings debuggerSettings,
        BreakpointHandler breakpointHandler)
    {
        _debuggerContext = debuggerContext;
        _debuggerSettings = debuggerSettings;
        _breakpointHandler = breakpointHandler;

        ConfigureEmulator(emulator);
    }

    public void ConfigureEmulator(Emulator emulator)
    {
        Emulator = emulator;

        _breakpointHandler.BreakpointHit -= OnBreakpointHit;
        _breakpointHandler.BreakpointHit += OnBreakpointHit;

        BreakpointListViewModel = new BreakpointListViewModel(_breakpointHandler.BreakpointManager);
        CodeListViewModel = new CodeListViewModel(_breakpointHandler.BreakpointManager);
        ImmediateViewModel = new ImmediateViewModel(_debuggerContext, _debuggerSettings.PreferredNumberFormat, emulator,
            () => Refresh(),
            list => CodeListViewModel.Update(
                Emulator.Memory,
                list.Address,
                emulator.Cpu.Registers.PC,
                _breakpointHandler,
                _debuggerSettings));

        LoggingViewModel.Configure(emulator);

        HandlePause(Emulator.IsPaused);
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

        if (!isPaused)
        {
            return;
        }

        WeakReferenceMessenger.Default.Send(new PauseForDebugMessage());

        Emulator.Pause();

        Refresh();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteStepCommands))]
    private void DebuggerStepOver()
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
            _breakpointHandler.BreakpointManager.AddSingleUseBreakpoint((Word)returnAddress.Value);
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
        Emulator.Cpu.Step();

        if (Emulator.Cpu.Clock.IsFrameComplete)
        {
            Emulator.Cpu.Clock.NewFrame(Emulator.TicksPerFrame);
        }

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

        _breakpointHandler.BreakpointHit -= OnBreakpointHit;
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

    private void RefreshCode() => CodeListViewModel.Update(
        Emulator.Memory,
        Emulator.Cpu.Registers.PC,
        Emulator.Cpu.Registers.PC,
        _breakpointHandler,
        _debuggerSettings);

    private void RefreshMemory() => MemoryViewModel.Update(Emulator.Memory);

    private static bool IsCallInstruction(byte opCode) => opCode == 0xCD || (opCode & 0b1100_0111) == 0b1100_0100;

    private static bool IsDjnzInstruction(byte opCode) => opCode == 0x10;

    private static bool IsJumpConditionInstruction(byte opCode) => (opCode & 0b1100_0111) == 0b1100_0010;

    private static bool IsShortJumpConditionInstruction(byte opCode) => opCode is 0x20 or 0x28 or 0x30 or 0x38;

    private static bool IsBlockTransferInstruction(byte opCode) => opCode is 0xB0 or 0xB8;

    public void Dispose()
    {
        Close();

        GC.SuppressFinalize(this);
    }
}