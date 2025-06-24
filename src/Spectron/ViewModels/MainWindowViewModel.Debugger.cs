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

    private void OpenDebuggerWindow()
    {
        _debuggerViewModel = new DebuggerViewModel(_debuggerContext, Emulator!, _preferences.Debugger, _breakpointHandler!);

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
            _breakpointHandler = new BreakpointHandler(emulator.Cpu, emulator.Memory);
            _breakpointHandler.BreakpointHit += OnBreakpointHit;
        }
        else
        {
            _breakpointHandler.Update(emulator.Cpu);
        }
    }

    private void OnBreakpointHit(object? sender, EventArgs e)
    {
        if (_debuggerViewModel != null)
        {
            return;
        }

        Dispatcher.UIThread.Post(OpenDebuggerWindow);
    }

    private void DebuggerWindowClosed()
    {
        Resume();

        _debuggerViewModel = null;
    }

    partial void OnBreakpointsEnabledChanged(bool value)
    {
        if (_breakpointHandler == null)
        {
            return;
        }

        _breakpointHandler.IsEnabled = value;
    }
}