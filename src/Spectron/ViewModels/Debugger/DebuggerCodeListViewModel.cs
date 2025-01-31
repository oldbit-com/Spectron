using System.Collections.ObjectModel;
using OldBit.Spectron.Emulation.Debugger;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.ViewModels.Debugger;

public class DebuggerCodeListViewModel(DebuggerContext debuggerContext) : ViewModelBase
{
    public ObservableCollection<DebuggerCodeLineViewModel> CodeLines { get; } = [];

    public void Update(IMemory memory, Word pc)
    {
        CodeLines.Clear();

        for (var i = 0; i < 25; i++)
        {
            var address = (Word)(pc + i);
            var code = memory.Read(address);

            CodeLines.Add(new DebuggerCodeLineViewModel(debuggerContext)
            {
                Address = address,
                Code = code.ToString("X2"),
                IsCurrent = i == 0,
                IsBreakpoint = debuggerContext.Breakpoints.Contains(address)
            });
        }
    }
}