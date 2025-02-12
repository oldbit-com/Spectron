using System.Collections.ObjectModel;
using ReactiveUI;

namespace OldBit.Spectron.Debugger.ViewModels;

public class BreakpointListViewModel : ReactiveObject
{
    private readonly DebuggerContext _debuggerContext;

    public ObservableCollection<BreakpointViewModel> Breakpoints { get; } = [];

    public BreakpointListViewModel()
    {
        Breakpoints.Add(new BreakpointViewModel { IsEnabled = true, Address =0x1000,  Condition = "PC == 0x1000" });
        Breakpoints.Add(new BreakpointViewModel { IsEnabled = false, Address = 0x1002, Condition = "PC == 0x1002" });
        Breakpoints.Add(new BreakpointViewModel { IsEnabled = true, Address = 0x1004, Condition = "PC == 0x1004" });
    }

    public BreakpointListViewModel(DebuggerContext debuggerContext)
    {
        _debuggerContext = debuggerContext;
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
}