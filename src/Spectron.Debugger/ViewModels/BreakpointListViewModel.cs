using System.Collections;
using System.Collections.ObjectModel;
using System.Reactive;
using DynamicData;
using OldBit.Spectron.Debugger.Breakpoints;
using ReactiveUI;

namespace OldBit.Spectron.Debugger.ViewModels;

public class BreakpointListViewModel : ReactiveObject
{
    private readonly BreakpointManager _breakpointManager;

    public ObservableCollection<BreakpointViewModel> Breakpoints { get; } = [];

    public ReactiveCommand<Unit, Unit> AddBreakpointCommand { get; private set; }
    public ReactiveCommand<IList, Unit> RemoveBreakpointCommand { get; private set; }

    public BreakpointListViewModel(BreakpointManager breakpointManager)
    {
        _breakpointManager = breakpointManager;

        AddBreakpointCommand = ReactiveCommand.Create(() => AddBreakpoint(Register.PC, 0x1000));
        RemoveBreakpointCommand = ReactiveCommand.Create<IList>(RemoveBreakpoints);
    }

    public void AddBreakpoint(Register register, Word value)
    {
        var breakpoint = new Breakpoint(register, value);
        var viewModel = new BreakpointViewModel($"{register} == ${value:X4}", breakpoint);

        Breakpoints.Add(viewModel);
        _breakpointManager.AddBreakpoint(breakpoint);
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

        Breakpoints.Replace(breakpoint, breakpoint);
    }

    private void RemoveBreakpoints(IList breakpoints)
    {
        foreach (var breakpoint in breakpoints.OfType<BreakpointViewModel>().ToList())
        {
            _breakpointManager.RemoveBreakpoint(breakpoint.Id);
            Breakpoints.Remove(breakpoint);
        }
    }
}