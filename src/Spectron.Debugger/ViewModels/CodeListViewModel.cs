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

    public void Update(IMemory memory, Word address, Word pc, DebuggerSettings debuggerSettings, BreakpointHitEventArgs? breakpointHitEventArgs)
    {
        CodeLines.Clear();

        var startAddress = DetermineStartAddress(address, breakpointHitEventArgs);

        var disassembly = new Disassembler(
            memory.ToBytes(),
            startAddress,
            maxCount: 25,
            new DisassemblerOptions { NumberFormat = debuggerSettings.NumberFormat });

        var instructions = disassembly.Disassemble();

        foreach (var instruction in instructions)
        {
            var isBreakpoint = breakpointManager.ContainsBreakpoint(Register.PC, instruction.Address);
            var isCurrent = instruction.Address == pc;

            CodeLines.Add(new CodeLineViewModel(
                instruction.Address,
                instruction.Code,
                isCurrent,
                isBreakpoint));
        }
    }

    private static Word DetermineStartAddress(Word address, BreakpointHitEventArgs? breakpointHitEventArgs)
    {
        if (breakpointHitEventArgs != null &&
            address - breakpointHitEventArgs.PreviousAddress < 5 &&
            address - breakpointHitEventArgs.PreviousAddress > 0)
        {
            return breakpointHitEventArgs.PreviousAddress;
        }

        return address;
    }
}