using CommunityToolkit.Mvvm.ComponentModel;
using OldBit.Spectron.Emulation.Extensions;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Debugger.ViewModels;

public partial class MemoryViewModel : ObservableObject
{
    [ObservableProperty]
    private byte[] _memory = [];

    public void Update(IMemory memory) => Memory = memory.GetBytes();
}