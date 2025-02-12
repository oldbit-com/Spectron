using System.Collections;
using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;

namespace OldBit.Spectron.Debugger.ViewModels;

public class BreakpointListViewModel : ReactiveObject
{
    private readonly DebuggerContext _debuggerContext;

    public ObservableCollection<BreakpointViewModel> Breakpoints { get; } = [];

    public ReactiveCommand<Unit, Unit> AddBreakpointCommand { get; private set; }
    public ReactiveCommand<IList, Unit> RemoveBreakpointCommand { get; private set; }
    

    public BreakpointListViewModel(DebuggerContext debuggerContext)
    {
        _debuggerContext = debuggerContext;

        AddBreakpointCommand = ReactiveCommand.Create(() => AddBreakpoint(0x1000));
        RemoveBreakpointCommand = ReactiveCommand.Create<IList>(RemoveBreakpoints);
    }

    public void AddBreakpoint(Word address)
    {
        _debuggerContext.AddBreakpoint(address);

        Breakpoints.Add(new BreakpointViewModel {
            IsEnabled = true,
            Address = address,
            Condition = $"PC == ${address:X4}" });
    }

    public void RemoveBreakpoint(Word address)
    {
        var breakpoint = Breakpoints.FirstOrDefault(x => x.Address == address);

        if (breakpoint is not null)
        {
            _debuggerContext.RemoveBreakpoint(breakpoint.Address);

            Breakpoints.Remove(breakpoint);
        }
    }

    public void UpdateBreakpoint(BreakpointViewModel breakpoint)
    {
        Console.WriteLine($"Updating breakpoint: {breakpoint.Address}");
    }

    private void RemoveBreakpoints(IList breakpoints)
    {
        foreach (var breakpoint in breakpoints.OfType<BreakpointViewModel>().ToList())
        {
            _debuggerContext.RemoveBreakpoint(breakpoint.Address);

            Breakpoints.Remove(breakpoint);
        }
    }
}