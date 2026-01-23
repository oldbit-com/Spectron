using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OldBit.Spectron.Debugger.ViewModels.Overlays;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Extensions;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Debugger.ViewModels;

public sealed partial class MemoryViewModel : ObservableObject, IDisposable
{
    private readonly Emulator _emulator;

    public GoToAddressOverlayViewModel GoToAddressOverlay { get; } = new();

    public Action<Word, byte> OnMemoryUpdated { get; set; } = (_, _) => { };
    public Action<Word> GoTo { get; set; } = _ => { };

    [ObservableProperty]
    private byte[] _memory = [];

    public MemoryViewModel(Emulator emulator)
    {
        _emulator = emulator;

        Memory = emulator.Memory.ToBytes();
        emulator.Memory.MemoryUpdated += MemoryUpdated;

        GoToAddressOverlay.GoTo = GoToAddress;
    }

    public void Update(IMemory memory) => Memory = memory.ToBytes();

    [RelayCommand]
    private void ShowGoToOverlay() => GoToAddressOverlay.Show();

    private void GoToAddress(Word address) => GoTo(address);

    private void MemoryUpdated(Word address, byte value) =>
        OnMemoryUpdated.Invoke(address, value);

    public void Dispose() => _emulator.Memory.MemoryUpdated -= MemoryUpdated;
}