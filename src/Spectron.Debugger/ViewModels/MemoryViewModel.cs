using OldBit.Spectron.Emulation.Extensions;
using OldBit.Z80Cpu;
using ReactiveUI;

namespace OldBit.Spectron.Debugger.ViewModels;

public class MemoryViewModel : ReactiveObject
{
    private byte[] _memory = [];
    public byte[] Memory
    {
        get => _memory;
        set => this.RaiseAndSetIfChanged(ref _memory, value);
    }

    public void Update(IMemory memory)
    {
        Memory = memory.GetMemory();
    }
}