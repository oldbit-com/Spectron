using System.Collections.ObjectModel;
using System.Collections.Specialized;
using OldBit.Spectron.Debugger.Breakpoints;
using OldBit.Spectron.Emulation.Extensions;
using OldBit.Z80Cpu;
using OldBit.Z80Cpu.Dasm;
using ReactiveUI;

namespace OldBit.Spectron.Debugger.ViewModels;

public class CodeListViewModel(
    BreakpointManager breakpointManager,
    BreakpointListViewModel breakpointListViewModel) : ReactiveObject
{
    public ObservableCollection<CodeLineViewModel> CodeLines { get; } = [];

    public void Update(IMemory memory, Word pc)
    {
        CodeLines.Clear();

        var disassembly = new Disassembler(memory.GetMemory(), pc, 20);
        var instructions = disassembly.Disassemble();

        for (var i = 0; i < instructions.Count; i++)
        {
            var isBreakpoint = breakpointManager.HasBreakpoint(Register.PC, instructions[i].Address);

            CodeLines.Add(new CodeLineViewModel(
                breakpointListViewModel,
                instructions[i].Address,
                instructions[i].Code,
                isCurrent: i == 0,
                isBreakpoint));
        }
    }

    public void UpdateBreakpoints(NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
            {
                foreach (var viewModel in e.NewItems?.Cast<BreakpointViewModel>() ?? [])
                {
                    if (CodeLines.FirstOrDefault(x => x.Address == viewModel.Breakpoint.Value ) is { } codeLine)
                    {
                        codeLine.IsBreakpoint = true;
                    }
                }

                break;
            }

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
            {
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
}