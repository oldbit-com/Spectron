using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using OldBit.Spectron.Extensions;
using OldBit.Spectron.Messages;
using OldBit.Spectron.Theming;

namespace OldBit.Spectron.ViewModels;

partial class MainWindowViewModel
{
    private async Task OpenPreferencesWindow()
    {
        var resumeAfter = false;

        if (!IsPaused)
        {
            Pause();
            resumeAfter = true;
        }

        var preferences = await WeakReferenceMessenger.Default.Send(new ShowPreferencesViewMessage(_preferences, _gamepadManager));

        ThemeManager.SelectTheme(preferences?.Theme ?? _preferences.Theme);

        if (preferences != null)
        {
            _preferences = preferences;

            Emulator?.IsUlaPlusEnabled = _preferences.IsUlaPlusEnabled;
            Emulator?.IsFloatingBusEnabled = _preferences.IsFloatingBusEnabled;
            Emulator?.JoystickManager.Configure(JoystickType);
            Emulator?.Printer.IsEnabled = _preferences.Printer.IsZxPrinterEnabled;
            Emulator?.MouseManager.Configure(MouseType);
            Emulator?.ConfigureTape(_preferences.Tape);
            Emulator?.ConfigureGamepad(_preferences.Joystick);
            Emulator.ConfigureAudio(preferences.Audio);

            if (_preferences.DivMmc.IsEnabled)
            {
                Emulator?.DivMmc.Enable();
                Emulator.ConfigureDivMMc(_preferences.DivMmc);
            }
            else
            {
                Emulator?.DivMmc.Disable();
            }

            if (_preferences.Beta128.IsEnabled)
            {
                Emulator?.Beta128.Enable();
            }
            else
            {
                Emulator?.Beta128.Disable();
            }

            if (_preferences.Interface1.IsEnabled)
            {
                Emulator?.Interface1.Enable();
                Emulator?.Interface1.ShadowRom.Version = _preferences.Interface1.RomVersion;
            }
            else
            {
                Emulator?.Interface1.Disable();
            }

            ConfigureTimeMachine(_preferences.TimeMachine);
            ConfigureMouseCursor();
            ConfigureShiftKeys(_preferences.Keyboard);

            RefreshControls();
        }

        if (resumeAfter)
        {
            Resume();
        }
    }

    private async Task OpenTimeMachineWindow()
    {
        if (!_preferences.TimeMachine.IsEnabled || _isTimeMachineOpen)
        {
            return;
        }

        if (!IsPaused)
        {
            Pause();
        }

        _isTimeMachineOpen = true;

        var viewModel = new TimeMachineViewModel(_timeMachine, Emulator!.JoystickManager, Emulator.CommandManager, _logger);

        var entry = await WeakReferenceMessenger.Default.Send(new ShowTimeMachineViewMessage(viewModel));

        if (entry != null)
        {
            var snapshot = entry.GetSnapshot();

            if (snapshot == null)
            {
                _logger.LogError("Failed to get snapshot from time machine entry");
                return;
            }

            IsTimeMachineCountdownVisible = true;

            CreateEmulator(snapshot, shouldResume: !IsTimeMachineCountdownVisible);
        }
        else
        {
            Resume();
        }

        _isTimeMachineOpen = false;
    }

    private static void OpenAboutWindow() =>
        WeakReferenceMessenger.Default.Send(new ShowAboutViewMessage());

    private static void ShowKeyboardHelpWindow() =>
        WeakReferenceMessenger.Default.Send(new ShowKeyboardViewMessage());

    private void ShowLogViewWindow() =>
        WeakReferenceMessenger.Default.Send(new ShowLogViewMessage(new LogViewModel(_logStore)));

    private void OpenScreenshotViewer() =>
        WeakReferenceMessenger.Default.Send(new ShowScreenshotViewMessage(_screenshotViewModel));

    private void OpenTrainersWindow() =>
        WeakReferenceMessenger.Default.Send(new ShowTrainerViewMessage(Emulator!, _pokeFile));

    private void OpenPrintOutputViewer() =>
        WeakReferenceMessenger.Default.Send(new ShowPrintOutputViewMessage(Emulator!.Printer));
}