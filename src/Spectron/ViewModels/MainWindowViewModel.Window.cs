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

        ThemeManager.SelectTheme(CommandLineArgs?.Theme ?? _preferences.Theme);

        IsAudioMuted = CommandLineArgs?.IsAudioMuted ?? _preferences.Audio.IsMuted;

        IsTimeMachineEnabled = _preferences.TimeMachine.IsEnabled;
        _timeMachine.SnapshotInterval = _preferences.TimeMachine.SnapshotInterval;
        _timeMachine.MaxDuration = _preferences.TimeMachine.MaxDuration;
        TimeMachineCountdownSeconds = _preferences.TimeMachine.CountdownSeconds;

        await RecentFilesViewModel.LoadAsync();

        UpdateShiftKeys(_preferences.Keyboard);
        HandleChangeBorderSize(CommandLineArgs?.BorderSize ?? _preferences.BorderSize);

        TapeLoadSpeed = CommandLineArgs?.TapeLoadSpeed ?? _preferences.Tape.LoadSpeed;

        if (await ResumeEmulatorSession())
        {
            return;
        }

        if (CommandLineArgs?.FilePath != null)
        {
            await HandleLoadFileAsync(CommandLineArgs?.FilePath);
        }

        if (CommandLineArgs?.IsAyEnabled != null)
        {
            Emulator!.AudioManager.IsAyEnabled = CommandLineArgs.IsAyEnabled.Value;
            StatusBarViewModel.IsAyEnabled = CommandLineArgs.IsAyEnabled.Value;
        }

        if (Emulator == null)
        {
            CreateEmulator(
                CommandLineArgs?.ComputerType ??_preferences.ComputerType,
                CommandLineArgs?.RomType ?? _preferences.RomType);

            Emulator.SetTapeSettings(_preferences.Tape);
        }
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

        _preferences.Audio.IsMuted = IsAudioMuted;
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