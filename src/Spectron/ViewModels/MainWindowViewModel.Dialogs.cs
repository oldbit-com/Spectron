using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using OldBit.Spectron.Debugger.ViewModels;
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

            IsUlaPlusEnabled = preferences.IsUlaPlusEnabled;
            Emulator.SetUlaPlus(IsUlaPlusEnabled);

            TapeLoadSpeed = preferences.Tape.LoadSpeed;
            Emulator.SetTapeSettings(_preferences.Tape);

            IsTimeMachineEnabled = preferences.TimeMachine.IsEnabled;
            _timeMachine.SnapshotInterval = preferences.TimeMachine.SnapshotInterval;
            _timeMachine.MaxDuration = preferences.TimeMachine.MaxDuration;
            TimeMachineCountdownSeconds = preferences.TimeMachine.CountdownSeconds;

            JoystickType = preferences.Joystick.JoystickType;
            MouseType = preferences.Mouse.MouseType;
            SetMouseCursor();

            UpdateShiftKeys(_preferences.Keyboard);
            ConfigureEmulatorSettings();
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

            CreateEmulator(snapshot);
        }

        _isTimeMachineOpen = false;

        if (!IsTimeMachineCountdownVisible)
        {
            Resume();
        }
    }

    private static void OpenAboutWindow() =>
        WeakReferenceMessenger.Default.Send(new ShowAboutViewMessage());

    private static void ShowKeyboardHelpWindow() =>
        WeakReferenceMessenger.Default.Send(new ShowKeyboardViewMessage());

    private void OpenScreenshotViewer() =>
        WeakReferenceMessenger.Default.Send(new ShowScreenshotViewMessage(_screenshotViewModel));

    private void OpenTrainersWindow() =>
        WeakReferenceMessenger.Default.Send(new ShowTrainerViewMessage(Emulator!, _pokeFile));

    private void OpenPrintOutputViewer() =>
        WeakReferenceMessenger.Default.Send(new ShowPrintOutputViewMessage(Emulator!.Printer));
}