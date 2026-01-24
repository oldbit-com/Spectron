using System.Text;
using Avalonia.Input.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OldBit.Spectron.Debugger.Controls.Hex;
using OldBit.Spectron.Debugger.ViewModels.Overlays;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Extensions;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Debugger.ViewModels;

public sealed partial class MemoryViewModel : ObservableObject, IDisposable
{
    private readonly Emulator _emulator;

    public GoToOverlayViewModel GoToOverlay { get; } = new();
    public FindOverlayViewModel FindOverlay { get; } = new();

    public HexViewer? Viewer { get; set; }
    public IClipboard? Clipboard { get; set; }

    public Action<Word, byte> OnMemoryUpdated { get; set; } = (_, _) => { };
    public Action<Word> GoTo { get; set; } = _ => { };

    [ObservableProperty]
    private byte[] _memory = [];

    public MemoryViewModel(Emulator emulator)
    {
        _emulator = emulator;

        Memory = emulator.Memory.ToBytes();
        emulator.Memory.MemoryUpdated += MemoryUpdated;

        GoToOverlay.GoTo = GoToAddress;
    }

    public void Update(IMemory memory) => Memory = memory.ToBytes();

    [RelayCommand]
    private void ShowGoToOverlay()
    {
        FindOverlay.Hide();
        GoToOverlay.Show();
    }

    [RelayCommand]
    private void ShowFindOverlay()
    {
        GoToOverlay.Hide();
        FindOverlay.Show();
    }

    [RelayCommand]
    private void FindNext()
    {
        Console.WriteLine("Find next");
    }

    [RelayCommand]
    private void CopyHex()
    {
        if (Clipboard is null)
        {
            return;
        }

        var bytes = GetSelectedBytes();

        Console.WriteLine("Copy hex");
    }

    [RelayCommand]
    private void CopyAscii()
    {
        if (Clipboard is null)
        {
            return;
        }

        var bytes = GetSelectedBytes();

        Console.WriteLine("Copy ascii");
    }

    private byte[] GetSelectedBytes()
    {
        if (Viewer is null)
        {
            return [];
        }

        var bytes = new byte[Viewer.SelectedIndexes.Length];

        foreach (var index in Viewer.SelectedIndexes)
        {
            bytes[index] = Memory[index];
        }

        return bytes;
    }

    private void GoToAddress(Word address) => GoTo(address);

    private void MemoryUpdated(Word address, byte value) =>
        OnMemoryUpdated.Invoke(address, value);

    public void Dispose() => _emulator.Memory.MemoryUpdated -= MemoryUpdated;
}