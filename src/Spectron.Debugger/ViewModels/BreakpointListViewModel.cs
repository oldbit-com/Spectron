using System.Collections;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OldBit.Spectron.Debugger.Breakpoints;
using OldBit.Spectron.Debugger.Messages;
using OldBit.Spectron.Disassembly.Formatters;

namespace OldBit.Spectron.Debugger.ViewModels;

public partial class BreakpointListViewModel : ObservableObject,
    IRecipient<ToggleBreakpointMessage>
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
            AddBreakpoint(breakpoint);
        }

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

    private void AddBreakpoint(Breakpoint breakpoint)
    {
        var viewModel = new BreakpointViewModel(breakpoint);

        Breakpoints.Add(viewModel);
        _breakpointManager.AddBreakpoint(breakpoint);

        WeakReferenceMessenger.Default.Send(new BreakpointAddedMessage(breakpoint));
    }

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

    private void RemoveBreakpoint(BreakpointViewModel breakpointViewModel)
    {
        _breakpointManager.RemoveBreakpoint(breakpointViewModel.Breakpoint);
        Breakpoints.Remove(breakpointViewModel);

        WeakReferenceMessenger.Default.Send(new BreakpointRemovedMessage(breakpointViewModel.Breakpoint));
    }

    public void UpdateBreakpoint(BreakpointViewModel breakpointViewModel)
    {
        if (!BreakpointParser.TryParse(breakpointViewModel.Condition, out var updated))
        {
            return;
        }

        var original = breakpointViewModel.Breakpoint;
        breakpointViewModel.Breakpoint = updated;

        updated.IsEnabled = original.IsEnabled;
        _breakpointManager.UpdateBreakpoint(original, updated);

        WeakReferenceMessenger.Default.Send(new BreakpointUpdatedMessage(original, updated));
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