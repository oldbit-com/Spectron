using System.Collections.ObjectModel;
using OldBit.Spectron.Emulation.Extensions;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.ViewModels.Debugger;

public record StackItem(string Address, string Value);

public class DebuggerStackViewModel : ViewModelBase
{
    public ObservableCollection<StackItem> Items { get; } = [];

    public void Update(IMemory memory, Word sp)
    {
        Items.Clear();

        for (var i = 0; i < 10; i++)
        {
            var address = (Word)(sp - (2 * i));
            var value = memory.ReadWord(address);

            Items.Add(new StackItem(
                address.ToString("X4"),
                value.ToString("X4")));
        }
    }
}