using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using OldBit.Spectron.Extensions;
using OldBit.Spectron.Theming;

namespace OldBit.Spectron.ViewModels;

partial class MainWindowViewModel
{
    private bool _shouldResume;
    private bool IsDebuggerOpen => _debuggerViewModel != null;

    internal void WindowActivated(ActivatedEventArgs args)
    {
        if (args.Kind == ActivationKind.File && args is FileActivatedEventArgs { Files.Count: > 0 } fileArgs)
        {
            _ = HandleLoadFileAsync(fileArgs.Files[0].Path.LocalPath);
            return;
        }

        if (!_preferences.Resume.ShouldAutoSuspendResume || !_shouldResume || !IsPaused)
        {
            return;
        }

        Resume();
        _shouldResume = false;
    }

    internal void WindowDeactivated(ActivatedEventArgs args)
    {
        if (!_preferences.Resume.ShouldAutoSuspendResume || IsPaused || IsDebuggerOpen)
        {
            return;
        }

        Pause();
        _shouldResume = true;
    }

    private async Task WindowOpenedAsync()
    {
        _preferences = await _preferencesService.LoadAsync();
        _favorites = await _favoritesService.LoadAsync();

        FavoritesViewModel.Favorites = _favorites;
        FavoritesViewModel.RefreshMenu();

        ThemeManager.SelectTheme(CommandLineArgs?.Theme ?? _preferences.Theme);
        IsNativeMenuEnabled = _preferences.IsNativeMenuEnabled;

        IsAudioMuted = CommandLineArgs?.IsAudioMuted ?? _preferences.Audio.IsMuted;

        IsTimeMachineEnabled = CommandLineArgs?.IsTimeMachineEnabled ?? _preferences.TimeMachine.IsEnabled;
        _timeMachine.SnapshotInterval = _preferences.TimeMachine.SnapshotInterval;
        _timeMachine.MaxDuration = _preferences.TimeMachine.MaxDuration;
        TimeMachineCountdownSeconds = _preferences.TimeMachine.CountdownSeconds;

        await RecentFilesViewModel.LoadAsync();

        ConfigureShiftKeys(_preferences.Keyboard);
        HandleChangeBorderSize(CommandLineArgs?.BorderSize ?? _preferences.BorderSize);
        HandleChangeScreenEffect(_preferences.ScreenEffect);

        WindowState = CommandLineArgs?.IsFullScreen == true ? WindowState.FullScreen : WindowState.Normal;
        TapeLoadSpeed = CommandLineArgs?.TapeLoadSpeed ?? _preferences.Tape.LoadSpeed;

        if (await ResumeEmulatorSession())
        {
            Emulator.ConfigureTape(_preferences.Tape);
            ApplyCommandLineArguments();

            return;
        }

        if (Emulator == null)
        {
            byte[]? customRom = null;

            if (CommandLineArgs?.CustomRomFiles?.Length > 0)
            {
                customRom = await ReadCustomRom(CommandLineArgs.CustomRomFiles);
            }

            CreateEmulator(
                CommandLineArgs?.ComputerType ??_preferences.ComputerType,
                CommandLineArgs?.RomType ?? _preferences.RomType,
                customRom);

            Emulator.ConfigureTape(_preferences.Tape);
        }

        ApplyCommandLineArguments();

        if (CommandLineArgs?.FilePath != null)
        {
            await HandleLoadFileAsync(CommandLineArgs?.FilePath);
        }
    }

    private void ApplyCommandLineArguments()
    {
        if (Emulator == null)
        {
            return;
        }

        if (CommandLineArgs?.JoystickType != null)
        {
            JoystickType = CommandLineArgs.JoystickType.Value;
        }

        if (CommandLineArgs?.MouseType != null)
        {
            MouseType = CommandLineArgs.MouseType.Value;
        }

        if (CommandLineArgs?.IsDivMmcEnabled != null)
        {
            if (CommandLineArgs.IsDivMmcEnabled.Value)
            {
                Emulator.DivMmc.Enable();

                if (CommandLineArgs?.DivMmcImageFile != null)
                {
                    Emulator.DivMmc.InsertCard(CommandLineArgs.DivMmcImageFile, 0);
                }

                if (CommandLineArgs?.IsDivMmcReadOnly != null)
                {
                    Emulator.DivMmc.IsDriveWriteEnabled = !CommandLineArgs.IsDivMmcReadOnly.Value;
                }
            }
            else
            {
                Emulator.DivMmc.Disable();
            }
        }

        if (CommandLineArgs?.IsBeta128Enabled != null)
        {
            if (CommandLineArgs.IsBeta128Enabled.Value)
            {
                Emulator.Beta128.Enable();
            }
            else
            {
                Emulator.Beta128.Disable();
            }
        }

        if (CommandLineArgs?.IsInterface1Enabled != null)
        {
            if (CommandLineArgs.IsInterface1Enabled.Value)
            {
                Emulator.Interface1.Enable();
            }
            else
            {
                Emulator.Interface1.Disable();
            }

            if (CommandLineArgs?.Interface1RomVersion != null)
            {
                Emulator.Interface1.ShadowRom.Version = CommandLineArgs.Interface1RomVersion.Value;
            }
        }

        if (CommandLineArgs?.TapeLoadSpeed != null)
        {
            TapeLoadSpeed = CommandLineArgs.TapeLoadSpeed.Value;
        }

        if (CommandLineArgs?.IsUlaPlusEnabled != null)
        {
            OnIsUlaPlusEnabledChanged(CommandLineArgs.IsUlaPlusEnabled.Value);
        }

        if (CommandLineArgs?.IsAyEnabled != null)
        {
            Emulator.AudioManager.IsAyEnabled = CommandLineArgs.IsAyEnabled.Value;
            Emulator.AudioManager.StereoMode = CommandLineArgs?.AyStereoMode ?? Emulator.AudioManager.StereoMode;
        }

        if (CommandLineArgs?.IsZxPrinterEnabled != null)
        {
            Emulator.Printer.IsEnabled = CommandLineArgs.IsZxPrinterEnabled.Value;
        }

        RefreshControls();
    }

    private async Task WindowClosingAsync(WindowClosingEventArgs args)
    {
        if (_canClose)
        {
            return;
        }

        args.Cancel = true;

        Emulator?.Shutdown(isAppClosing: true);
        _frameRateCalculator.Dispose();

        _preferences.Audio.IsMuted = IsAudioMuted;
        _preferences.BorderSize = BorderSize;
        _preferences.ScreenEffect = ScreenEffect;

        await Task.WhenAll(
            _preferencesService.SaveAsync(_preferences),
            _favoritesService.SaveAsync(_favorites),
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