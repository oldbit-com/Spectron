using System.Collections;
using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OldBit.Spectron.Debugger.Breakpoints;
using OldBit.Spectron.Debugger.Messages;
using OldBit.Spectron.Disassembly.Formatters;

namespace OldBit.Spectron.Debugger.ViewModels;

/// <summary>
/// Shows the breakpoints of a <see cref="BreakpointManager"/>. The list follows the manager's
/// <see cref="BreakpointManager.Changed"/> event, so it also shows changes made elsewhere (e.g. by an MCP agent).
/// </summary>
public partial class BreakpointListViewModel : ObservableObject,
    IRecipient<ToggleBreakpointMessage>, IDisposable
{
    private readonly BreakpointManager _breakpointManager;
    private readonly NumberFormatter _numberFormatter;

    public ObservableCollection<BreakpointViewModel> Breakpoints { get; } = [];

    public BreakpointListViewModel(BreakpointManager breakpointManager, NumberFormat numberFormat)
    {
        _breakpointManager = breakpointManager;
        _numberFormatter = new NumberFormatter(numberFormat);

        foreach (var breakpoint in _breakpointManager.Breakpoints)
        {
            Breakpoints.Add(new BreakpointViewModel(breakpoint));
        }

        _breakpointManager.Changed += BreakpointManagerOnChanged;

        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    [RelayCommand]
    private void AddBreakpoint() => AddBreakpoint(Register.PC, 56);

    [RelayCommand]
    private void RemoveBreakpoint(IList breakpoints)
    {
        foreach (var breakpoint in breakpoints.OfType<BreakpointViewModel>().ToList())
        {
            RemoveBreakpoint(breakpoint);
        }
    }

    private void AddBreakpoint(Breakpoint breakpoint) => _breakpointManager.AddBreakpoint(breakpoint);

    private void AddBreakpoint(Register register, Word value)
    {
        var breakpoint = new RegisterBreakpoint(register, value)
        {
            Condition = $"{register} == {_numberFormatter.Format(value)}"
        };

        AddBreakpoint(breakpoint);
    }

    private void RemoveBreakpoint(Register register, Word address)
    {
        var breakpoint = Breakpoints.FirstOrDefault(vm =>
            vm.Breakpoint is RegisterBreakpoint registerBreakpoint &&
            registerBreakpoint.Value == address &&
            registerBreakpoint.Register == register);

        if (breakpoint != null)
        {
            RemoveBreakpoint(breakpoint);
        }
    }

    private void RemoveBreakpoint(BreakpointViewModel breakpointViewModel) =>
        _breakpointManager.RemoveBreakpoint(breakpointViewModel.Breakpoint);

    public void UpdateBreakpoint(BreakpointViewModel breakpointViewModel)
    {
        if (!BreakpointParser.TryParse(breakpointViewModel.Condition, out var updated))
        {
            return;
        }

        var original = breakpointViewModel.Breakpoint;

        updated.IsEnabled = original.IsEnabled;
        _breakpointManager.UpdateBreakpoint(original, updated);
    }

    private void BreakpointManagerOnChanged(object? sender, BreakpointsChangedEventArgs e)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            ApplyChange(e);
        }
        else
        {
            Dispatcher.UIThread.Post(() => ApplyChange(e));
        }
    }

    internal void ApplyChange(BreakpointsChangedEventArgs e)
    {
        switch (e.Change)
        {
            case BreakpointChange.Added:
                if (Breakpoints.All(vm => vm.Breakpoint != e.Breakpoint))
                {
                    Breakpoints.Add(new BreakpointViewModel(e.Breakpoint));
                }

                WeakReferenceMessenger.Default.Send(new BreakpointAddedMessage(e.Breakpoint));
                break;

            case BreakpointChange.Removed:
                var removed = Breakpoints.FirstOrDefault(vm => vm.Breakpoint == e.Breakpoint);

                if (removed != null)
                {
                    Breakpoints.Remove(removed);
                }

                WeakReferenceMessenger.Default.Send(new BreakpointRemovedMessage(e.Breakpoint));
                break;

            case BreakpointChange.Updated:
                var updated = Breakpoints.FirstOrDefault(vm => vm.Breakpoint == e.Original);

                if (updated != null)
                {
                    updated.Breakpoint = e.Breakpoint;
                }

                WeakReferenceMessenger.Default.Send(new BreakpointUpdatedMessage(e.Original!, e.Breakpoint));
                break;
        }
    }

    public void Receive(ToggleBreakpointMessage message)
    {
        if (message.IsEnabled)
        {
            AddBreakpoint(message.Register, message.Address);
        }
        else
        {
            RemoveBreakpoint(message.Register, message.Address);
        }
    }

    public void Dispose()
    {
        _breakpointManager.Changed -= BreakpointManagerOnChanged;
        WeakReferenceMessenger.Default.UnregisterAll(this);

        GC.SuppressFinalize(this);
    }
}
