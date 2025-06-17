using System.Collections;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OldBit.Spectron.Debugger.Breakpoints;
using OldBit.Spectron.Debugger.Messages;

namespace OldBit.Spectron.Debugger.ViewModels;

public partial class BreakpointListViewModel : ObservableObject,
    IRecipient<ToggleBreakpointMessage>
{
    private readonly BreakpointManager _breakpointManager;

    public ObservableCollection<BreakpointViewModel> Breakpoints { get; } = [];

    public BreakpointListViewModel(DebuggerContext debuggerContext, BreakpointManager breakpointManager)
    {
        _breakpointManager = breakpointManager;

        foreach (var breakpoint in debuggerContext.Breakpoints)
        {
            AddBreakpoint(breakpoint);
        }

        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    [RelayCommand]
    private void AddBreakpoint() => AddBreakpoint(Register.PC, 0x1000);

    [RelayCommand]
    private void RemoveBreakpoint(IList breakpoints)
    {
        foreach (var breakpoint in breakpoints.OfType<BreakpointViewModel>().ToList())
        {
            RemoveBreakpoint(breakpoint);
        }
    }

    private void AddBreakpoint(Breakpoint breakpoint)
    {
        var viewModel = new BreakpointViewModel(breakpoint);

        Breakpoints.Add(viewModel);
        _breakpointManager.AddBreakpoint(breakpoint);

        WeakReferenceMessenger.Default.Send(new BreakpointAddedMessage((Word)breakpoint.Value));
    }

    private void AddBreakpoint(Register register, Word value)
    {
        var breakpoint = new Breakpoint(register, value);

        AddBreakpoint(breakpoint);
    }

    private void RemoveBreakpoint(Register register, Word value)
    {
        var breakpoint = Breakpoints.FirstOrDefault(x =>
            x.Breakpoint.Value == value &&
            x.Breakpoint.Register == register);

        if (breakpoint != null)
        {
            RemoveBreakpoint(breakpoint);
        }
    }

    private void RemoveBreakpoint(BreakpointViewModel breakpoint)
    {
        _breakpointManager.RemoveBreakpoint(breakpoint.Id);
        Breakpoints.Remove(breakpoint);

        WeakReferenceMessenger.Default.Send(new BreakpointRemovedMessage(breakpoint.Breakpoint.Register, (Word)breakpoint.Breakpoint.Value));
    }

    public void UpdateBreakpoint(BreakpointViewModel breakpoint)
    {
        if (!BreakpointParser.TryParseCondition(breakpoint.Condition, out var updated))
        {
            return;
        }

        var (newRegister, newAddress) = updated.Value;
        var oldAddress = breakpoint.Breakpoint.Value;

        _breakpointManager.UpdateBreakpoint(
            breakpoint.Breakpoint.Id,
            newRegister,
            newAddress,
            breakpoint.IsEnabled);

        WeakReferenceMessenger.Default.Send(new BreakpointUpdatedMessage(newRegister, (Word)oldAddress, (Word)newAddress));
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
}