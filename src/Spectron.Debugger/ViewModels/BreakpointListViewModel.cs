using System.Collections;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OldBit.Spectron.Debugger.Breakpoints;

namespace OldBit.Spectron.Debugger.ViewModels;

public partial class BreakpointListViewModel : ObservableObject
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
    }

    [RelayCommand]
    private void AddBreakpoint() => AddBreakpoint(Register.PC, 0x1000);

    [RelayCommand]
    private void RemoveBreakpoint(IList breakpoints)
    {
        foreach (var breakpoint in breakpoints.OfType<BreakpointViewModel>().ToList())
        {
            _breakpointManager.RemoveBreakpoint(breakpoint.Id);

            Breakpoints.Remove(breakpoint);
        }
    }

    private void AddBreakpoint(Breakpoint breakpoint)
    {
        var viewModel = new BreakpointViewModel(breakpoint);

        Breakpoints.Add(viewModel);
        _breakpointManager.AddBreakpoint(breakpoint);
    }

    public void AddBreakpoint(Register register, Word value)
    {
        var breakpoint = new Breakpoint(register, value);

        AddBreakpoint(breakpoint);
    }

    public void RemoveBreakpoint(Register register, Word value)
    {
        var breakpoint = Breakpoints.FirstOrDefault(x =>
            x.Breakpoint.Value == value &&
            x.Breakpoint.Register == register);

        if (breakpoint != null)
        {
            _breakpointManager.RemoveBreakpoint(breakpoint.Breakpoint.Id);
            Breakpoints.Remove(breakpoint);
        }
    }

    public void UpdateBreakpoint(BreakpointViewModel breakpoint)
    {
        if (!BreakpointParser.TryParseCondition(breakpoint.Condition, out var updated))
        {
            return;
        }

        _breakpointManager.UpdateBreakpoint(
            breakpoint.Breakpoint.Id,
            updated.Value.Register,
            updated.Value.Address,
            breakpoint.IsEnabled);

        var existingIndex = Breakpoints.IndexOf(breakpoint);

        if (existingIndex >= 0)
        {
            Breakpoints[existingIndex] = breakpoint;
        }
    }
}