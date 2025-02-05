using System.Collections.ObjectModel;
using OldBit.Spectron.Emulation.Debugger;
using OldBit.Spectron.Emulation.Extensions;
using OldBit.Z80Cpu;
using OldBit.Z80Cpu.Dasm;

namespace OldBit.Spectron.ViewModels.Debugger;

public class DebuggerCodeListViewModel(DebuggerContext debuggerContext) : ViewModelBase
{
    public ObservableCollection<DebuggerCodeLineViewModel> CodeLines { get; } = [];

    public void Update(IMemory memory, Word pc)
    {
        CodeLines.Clear();

        var disassembly = new Disassembler(memory.GetMemory(), pc, 20);
        var instructions = disassembly.Disassemble();

        for (var i = 0; i < instructions.Count; i++)
        {
            CodeLines.Add(new DebuggerCodeLineViewModel(debuggerContext)
            {
                Address = instructions[i].Address,
                Code = instructions[i].Code,
                IsCurrent = i == 0,
                IsBreakpoint = debuggerContext.Breakpoints.Contains(instructions[i].Address)
            });
        }
    }
}