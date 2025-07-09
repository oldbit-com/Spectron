using System.Threading.Tasks;
using Avalonia.Controls;
using OldBit.Spectron.Extensions;
using OldBit.Spectron.Theming;

namespace OldBit.Spectron.ViewModels;

partial class MainWindowViewModel
{
    private async Task WindowOpenedAsync()
    {
        _preferences = await _preferencesService.LoadAsync();

        ThemeManager.SelectTheme(_preferences.Theme);

        IsMuted = _preferences.Audio.IsMuted;

        IsTimeMachineEnabled = _preferences.TimeMachine.IsEnabled;
        _timeMachine.SnapshotInterval = _preferences.TimeMachine.SnapshotInterval;
        _timeMachine.MaxDuration = _preferences.TimeMachine.MaxDuration;
        TimeMachineCountdownSeconds = _preferences.TimeMachine.CountdownSeconds;

        await RecentFilesViewModel.LoadAsync();

        UpdateShiftKeys(_preferences.Keyboard);
        HandleChangeBorderSize(_preferences.BorderSize);

        if (_preferences.Resume.IsResumeEnabled)
        {
            var snapshot = await _sessionService.LoadAsync();

            if (snapshot != null)
            {
                CreateEmulator(snapshot);
                UpdateWindowTitle();
            }
        }

        if (Emulator == null)
        {
            CreateEmulator(_preferences.ComputerType, _preferences.RomType);
        }

        TapeLoadSpeed = _preferences.Tape.LoadSpeed;
        Emulator?.SetTapeSettings(_preferences.Tape);
    }

    private async Task WindowClosingAsync(WindowClosingEventArgs args)
    {
        if (_canClose)
        {
            return;
        }

        args.Cancel = true;

        Emulator?.Shutdown(isAppClosing: true);
        _keyboardHook?.Dispose();
        _frameRateCalculator.Dispose();

        _preferences.Audio.IsMuted = IsMuted;
        _preferences.BorderSize = BorderSize;

        await Task.WhenAll(
            _preferencesService.SaveAsync(_preferences),
            RecentFilesViewModel.SaveAsync(),
            _sessionService.SaveAsync(Emulator, _preferences.Resume));

        _canClose = true;
        MainWindow?.Close();
    }

    private void UpdateWindowTitle()
    {
        if (RecentFilesViewModel.CurrentFileName == string.Empty)
        {
            Title = DefaultTitle;

            return;
        }

        Title = $"{DefaultTitle} [{RecentFilesViewModel.CurrentFileName}]";
    }
}