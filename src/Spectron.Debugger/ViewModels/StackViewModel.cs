using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using OldBit.Spectron.Emulation.Extensions;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Debugger.ViewModels;

public record StackItem(string Address, string Value, bool IsCurrent);

public class StackViewModel : ObservableObject
{
    public ObservableCollection<StackItem> Items { get; } = [];

    public void Update(IMemory memory, Word sp)
    {
        Items.Clear();

        for (var i = 9; i >= 0; i--)
        {
            var address = (Word)(sp + (2 * i));
            var value = memory.ReadWord(address);

            Items.Add(new StackItem(
                address.ToString("X4"),
                value.ToString("X4"),
                i == 0));
        }
    }
}