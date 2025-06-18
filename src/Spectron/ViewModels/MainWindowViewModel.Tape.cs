using OldBit.Spectron.Emulation.Tape;

namespace OldBit.Spectron.ViewModels;

partial class MainWindowViewModel
{
    private void HandleTapeStateChanged(TapeStateEventArgs args)
    {
        if (args.Action == TapeAction.TapeEjected)
        {
            RecentFilesViewModel.CurrentFileName = string.Empty;
        }

        StatusBarViewModel.IsTapeLoaded = args.Action != TapeAction.TapeEjected;
        StatusBarViewModel.TapeLoadProgress = string.Empty;

        UpdateWindowTitle();
    }

    private void HandleSetTapeLoadingSpeed(TapeSpeed tapeSpeed) => TapeLoadSpeed = tapeSpeed;
}