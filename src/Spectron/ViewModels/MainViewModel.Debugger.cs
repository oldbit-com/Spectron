using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using OldBit.Spectron.Debugger.Breakpoints;
using OldBit.Spectron.Debugger.ViewModels;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Messages;

namespace OldBit.Spectron.ViewModels;

partial class MainViewModel
{
    [ObservableProperty]
    public partial bool BreakpointsEnabled { get; set; }

    private BreakpointHitEventArgs? _breakpointHitEventArgs;

    private void OpenDebuggerWindow(BreakpointHitEventArgs? breakpointHitEventArgs = null)
    {
        _breakpointHitEventArgs = breakpointHitEventArgs;

        _debuggerViewModel = new DebuggerViewModel(Emulator!, _debuggerContext,
            _preferences.Debugger, _breakpointHandler!, breakpointHitEventArgs);

        if (!IsPaused)
        {
            Pause(showOverlay: false);
        }

        BreakpointsEnabled = true;

        WeakReferenceMessenger.Default.Send(new ShowDebuggerViewMessage(_debuggerViewModel));
    }

    private void OpenMemoryWindow()
    {
        if (Emulator == null)
        {
            return;
        }

        var memoryViewModel = new MemoryViewModel(Emulator);
        WeakReferenceMessenger.Default.Send(new ShowMemoryViewMessage(memoryViewModel));
    }

    private void ConfigureDebugging(Emulator emulator)
    {
        if (_breakpointHandler == null)
        {
            _breakpointHandler = new BreakpointHandler(emulator) { IsEnabled = BreakpointsEnabled };
            _breakpointHandler.BreakpointHit += OnBreakpointHit;
        }
        else
        {
            _breakpointHandler.Update(emulator);
        }
    }

    private void OnBreakpointHit(object? sender, BreakpointHitEventArgs e)
    {
        // An open window refreshes itself. While paused, frames only run from an MCP tool call (or a step in
        // the open window), so the hit belongs to the agent, which gets it in the tool result.
        if (_debuggerViewModel != null || IsPaused)
        {
            return;
        }

        Dispatcher.UIThread.Post(() => OpenDebuggerWindow(e));
    }

    private void DebuggerWindowClosed()
    {
        Resume();

        _debuggerViewModel?.Dispose();
        _debuggerViewModel = null;
        _breakpointHitEventArgs = null;
    }

    private void ResumeFromDebug() => Resume();

    private void PauseForDebug() => Pause(showOverlay: false);

    partial void OnBreakpointsEnabledChanged(bool value) => _breakpointHandler?.IsEnabled = value;
}