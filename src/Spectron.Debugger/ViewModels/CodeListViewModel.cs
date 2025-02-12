using System.Collections.ObjectModel;
using System.Collections.Specialized;
using OldBit.Spectron.Emulation.Extensions;
using OldBit.Z80Cpu;
using OldBit.Z80Cpu.Dasm;
using ReactiveUI;

namespace OldBit.Spectron.Debugger.ViewModels;

public class CodeListViewModel(
    DebuggerContext debuggerContext,
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
            CodeLines.Add(new CodeLineViewModel(breakpointListViewModel)
            {
                Address = instructions[i].Address,
                Code = instructions[i].Code,
                IsCurrent = i == 0,
                IsBreakpoint = debuggerContext.Breakpoints.Contains(instructions[i].Address)
            });
        }
    }

    public void UpdateBreakpoints(NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
            {
                foreach (var breakpoint in e.NewItems?.Cast<BreakpointViewModel>() ?? [])
                {
                    if (CodeLines.FirstOrDefault(x => x.Address == breakpoint.Address) is { } codeLine)
                    {
                        codeLine.IsBreakpoint = true;
                    }
                }

                break;
            }

            case NotifyCollectionChangedAction.Remove:
            {
                foreach (var breakpoint in e.OldItems?.Cast<BreakpointViewModel>() ?? [])
                {
                    if (CodeLines.FirstOrDefault(x => x.Address == breakpoint.Address) is { } codeLine)
                    {
                        codeLine.IsBreakpoint = false;
                    }
                }

                break;
            }
        }
    }
}