using System.Collections.ObjectModel;
using System.Collections.Specialized;
using OldBit.Spectron.Debugger.Breakpoints;
using OldBit.Spectron.Debugger.Settings;
using OldBit.Spectron.Disassembly;
using OldBit.Spectron.Emulation.Extensions;
using OldBit.Z80Cpu;
using ReactiveUI;

namespace OldBit.Spectron.Debugger.ViewModels;

public class CodeListViewModel(
    BreakpointManager breakpointManager,
    BreakpointListViewModel breakpointListViewModel) : ReactiveObject
{
    public ObservableCollection<CodeLineViewModel> CodeLines { get; } = [];

    public void Update(IMemory memory, Word address, Word pc, DebuggerSettings debuggerSettings)
    {
        CodeLines.Clear();

        var disassembly = new Disassembler(
            memory.GetBytes(),
            address,
            maxCount: 25,
            new DisassemblerOptions { NumberFormat = debuggerSettings.PreferredNumberFormat });

        var instructions = disassembly.Disassemble();

        for (var i = 0; i < instructions.Count; i++)
        {
            var isBreakpoint = breakpointManager.HasBreakpoint(Register.PC, instructions[i].Address);
            var isCurrent = instructions[i].Address == pc;

            CodeLines.Add(new CodeLineViewModel(
                breakpointListViewModel,
                instructions[i].Address,
                instructions[i].Code,
                isCurrent,
                isBreakpoint));
        }
    }

    public void UpdateBreakpoints(NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:

                foreach (var viewModel in e.NewItems?.Cast<BreakpointViewModel>() ?? [])
                {
                    if (CodeLines.FirstOrDefault(x => x.Address == viewModel.Breakpoint.Value ) is { } codeLine)
                    {
                        codeLine.IsBreakpoint = true;
                    }
                }
                break;

            case NotifyCollectionChangedAction.Replace:

                foreach (var viewModel in e.NewItems?.Cast<BreakpointViewModel>() ?? [])
                {
                    if (CodeLines.FirstOrDefault(x => x.Address == viewModel.Breakpoint.Value) is { } codeLine)
                    {
                        codeLine.UpdateBreakpoint(true);
                    }
                }
                break;

            case NotifyCollectionChangedAction.Remove:

                foreach (var viewModel in e.OldItems?.Cast<BreakpointViewModel>() ?? [])
                {
                    if (CodeLines.FirstOrDefault(x => x.Address == viewModel.Breakpoint.Value) is { } codeLine)
                    {
                        codeLine.IsBreakpoint = false;
                    }
                }
                break;
        }
    }
}