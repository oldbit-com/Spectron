using System;
using Avalonia.Threading;
using OldBit.Spectron.Emulation.Devices.Beta128.Events;
using OldBit.Spectron.Emulation.Devices.Interface1.Microdrives.Events;
using OldBit.Spectron.Emulation.Tape;

namespace OldBit.Spectron.ViewModels;

partial class MainWindowViewModel
{
    private void HandleTapeTapeChanged(TapeChangedEventArgs e)
    {
        if (e.Action == TapeAction.Ejected)
        {
            RecentFilesViewModel.CurrentFileName = string.Empty;
        }

        StatusBarViewModel.IsTapeInserted = e.Action != TapeAction.Ejected;
        StatusBarViewModel.TapeLoadProgress = string.Empty;

        UpdateWindowTitle();
    }

    private void HandleDiskActivity(EventArgs e)
    {
        StatusBarViewModel.AnimateDiskActivity();
    }

    private void HandleCartridgeChanged(CartridgeChangedEventArgs e) => RefreshStatusBar();

    private void HandleFloppyDiskChanged(DiskChangedEventArgs e) => RefreshStatusBar();

    private void HandleSetTapeLoadingSpeed(TapeSpeed tapeSpeed) => TapeLoadSpeed = tapeSpeed;
}