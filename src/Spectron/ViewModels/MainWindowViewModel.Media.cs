using OldBit.Spectron.Emulation.Devices.Interface1.Microdrive;
using OldBit.Spectron.Emulation.Tape;

namespace OldBit.Spectron.ViewModels;

partial class MainWindowViewModel
{
    private void HandleTapeStateChanged(TapeStateEventArgs e)
    {
        if (e.Action == TapeAction.Ejected)
        {
            RecentFilesViewModel.CurrentFileName = string.Empty;
        }

        StatusBarViewModel.IsTapeInserted = e.Action != TapeAction.Ejected;
        StatusBarViewModel.TapeLoadProgress = string.Empty;

        UpdateWindowTitle();
    }

    private void HandleMicrodriveStateChanged(MicrodriveStateChangedEventArgs e) =>
        RefreshInterface1State(Emulator?.Interface1.IsEnabled == true);

    private void HandleSetTapeLoadingSpeed(TapeSpeed tapeSpeed) => TapeLoadSpeed = tapeSpeed;
}