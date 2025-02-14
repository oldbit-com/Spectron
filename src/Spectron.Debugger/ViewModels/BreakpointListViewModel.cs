using System.Collections;
using System.Collections.ObjectModel;
using System.Reactive;
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

        AddBreakpointCommand = ReactiveCommand.Create(() => AddBreakpoint(0x1000));
        RemoveBreakpointCommand = ReactiveCommand.Create<IList>(RemoveBreakpoints);
    }

    public void AddBreakpoint(Word address)
    {
        var breakpoint = new Breakpoint(Register.PC, address);
        var viewModel = new BreakpointViewModel($"PC == ${address:X4}", breakpoint);

        Breakpoints.Add(viewModel);
        _breakpointManager.AddBreakpoint(breakpoint);
    }

    public void RemoveBreakpoint(Word address)
    {
        var breakpoint = Breakpoints.FirstOrDefault(x => x.Address == address);

        if (breakpoint is not null)
        {
            _breakpointManager.RemoveBreakpoint(breakpoint.Id);
            Breakpoints.Remove(breakpoint);
        }
    }

    public void UpdateBreakpoint(BreakpointViewModel breakpointViewModel)
    {
        if (!BreakpointParser.TryParseCondition(breakpointViewModel.Condition, out var breakpoint))
        {
            return;
        }

        _breakpointManager.UpdateBreakpoint(breakpointViewModel.Id, breakpoint);
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