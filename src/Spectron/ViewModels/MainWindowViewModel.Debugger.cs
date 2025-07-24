using System;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using OldBit.Spectron.Debugger.Breakpoints;
using OldBit.Spectron.Debugger.ViewModels;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Messages;

namespace OldBit.Spectron.ViewModels;

partial class MainWindowViewModel
{
    [ObservableProperty]
    private bool _breakpointsEnabled;
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

    private void ConfigureDebugging(Emulator emulator)
    {
        if (_breakpointHandler == null)
        {
            _breakpointHandler = new BreakpointHandler(emulator);
            _breakpointHandler.BreakpointHit += OnBreakpointHit;
        }
        else
        {
            _breakpointHandler.Update(emulator);
        }
    }

    private void OnBreakpointHit(object? sender, BreakpointHitEventArgs e)
    {
        if (_debuggerViewModel != null)
        {
            return;
        }

        Dispatcher.UIThread.Post(() => OpenDebuggerWindow(e));
    }

    private void DebuggerWindowClosed()
    {
        Resume(isDebuggerResume: true);

        _debuggerViewModel = null;
        _breakpointHitEventArgs = null;
    }

    private void ResumeFromDebug() => Resume(isDebuggerResume: true);

    private void PauseForDebug() => Pause(showOverlay: false);

    partial void OnBreakpointsEnabledChanged(bool value)
    {
        if (_breakpointHandler == null)
        {
            return;
        }

        _breakpointHandler.IsEnabled = value;
    }
}