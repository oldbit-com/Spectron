using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using OldBit.Spectron.Debugger.Breakpoints;
using OldBit.Spectron.Debugger.Settings;
using OldBit.Spectron.Disassembly;
using OldBit.Spectron.Emulation.Extensions;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Debugger.ViewModels;

public class CodeListViewModel(BreakpointManager breakpointManager) : ObservableObject
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

        foreach (var instruction in instructions)
        {
            var isBreakpoint = breakpointManager.HasBreakpoint(Register.PC, instruction.Address);
            var isCurrent = instruction.Address == pc;

            CodeLines.Add(new CodeLineViewModel(
                instruction.Address,
                instruction.Code,
                isCurrent,
                isBreakpoint));
        }
    }
}