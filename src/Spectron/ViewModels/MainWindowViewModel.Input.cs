using Avalonia;
using Avalonia.Input;
using Avalonia.Threading;
using OldBit.Spectron.Emulation.Commands;
using OldBit.Spectron.Emulation.Devices.Gamepad;
using OldBit.Spectron.Emulation.Devices.Joystick;
using OldBit.Spectron.Emulation.Devices.Mouse;
using OldBit.Spectron.Input;
using OldBit.Spectron.Settings;

namespace OldBit.Spectron.ViewModels;

partial class MainWindowViewModel
{
    private void HandleKeyDown(KeyEventArgs e)
    {
        if (MainWindow?.IsActive != true)
        {
            return;
        }

        switch (e)
        {
            case { Key: Key.Escape }:
                if (IsPaused)
                {
                    HandleTogglePause();
                }
                break;

            case { Key: Key.F1, KeyModifiers: KeyModifiers.None }:
                ShowKeyboardHelpWindow();
                return;

            case { Key: Key.F5, KeyModifiers: KeyModifiers.Control }:
                HandleMachineReset(hardReset: true);
                return;
        }
    }

    private void HandleSpectrumKeyPressed(object? sender, SpectrumKeyEventArgs e)
    {
        if (MainWindow?.IsActive != true)
        {
            return;
        }

        if (JoystickHandled(e))
        {
            return;
        }

        Emulator?.KeyboardState.KeyDown(e.Keys);
    }

    private void HandleSpectrumKeyReleased(object? sender, SpectrumKeyEventArgs e)
    {
        if (MainWindow?.IsActive != true || IsPaused)
        {
            return;
        }

        if (JoystickHandled(e))
        {
            return;
        }

        Emulator?.KeyboardState.KeyUp(e.Keys);
    }

    private void UpdateShiftKeys(KeyboardSettings settings) => _keyboardHook.UpdateShiftKeys(
        settings.CapsShiftKey,
        settings.SymbolShiftKey);

    private bool JoystickHandled(SpectrumKeyEventArgs e)
    {
        if (!IsKeyboardJoystickEmulationEnabled)
        {
            return false;
        }

        var joystickInput = KeyboardHook.ToJoystickAction(e.KeyCode, _preferences.Joystick.FireKey);

        if (joystickInput == JoystickInput.None)
        {
            return false;
        }

        if (e.IsKeyPressed)
        {
            Emulator?.JoystickManager.Pressed(joystickInput);
        }
        else
        {
            Emulator?.JoystickManager.Released(joystickInput);
        }

        return true;
    }

    private void CommandManagerOnCommandReceived(object? sender, CommandEventArgs e)
    {
        if (e.Command is not GamepadActionCommand gamepadCommand)
        {
            return;
        }

        if (gamepadCommand.State == InputState.Pressed)
        {
            return;
        }

        if (MainWindow?.IsActive != true)
        {
            return;
        }

        switch (gamepadCommand.Action)
        {
            case GamepadAction.Pause:
                HandleTogglePause();
                break;

            case GamepadAction.TimeTravel:
                Dispatcher.UIThread.InvokeAsync(async () => await OpenTimeMachineWindow());
                break;

            case GamepadAction.QuickSave:
                HandleQuickSave();
                break;

            case GamepadAction.QuickLoad:
                HandleQuickLoad();
                break;

            case GamepadAction.NMI:
                Emulator?.RequestNmi();
                break;
        }
    }

    partial void OnJoystickTypeChanged(JoystickType value)
    {
        StatusBarViewModel.JoystickType = value;
        Emulator?.JoystickManager.SetupJoystick(value);
    }

    partial void OnMouseTypeChanged(MouseType value)
    {
        StatusBarViewModel.IsMouseEnabled = value != MouseType.None;

        SetMouseCursor();

        if (Emulator == null)
        {
            return;
        }

        Emulator.MouseManager.SetupMouse(value);
        _mouseHelper = new MouseHelper(Emulator.MouseManager);
    }

    private bool IsKeyboardJoystickEmulationEnabled =>
        JoystickType != JoystickType.None && _preferences.Joystick.EmulateUsingKeyboard;

    public void HandleMouseMoved(Point position, Rect bounds) =>
        _mouseHelper?.MouseMoved(BorderSize, position, bounds);

    public void HandleMouseButtonStateChanged(PointerPoint point, Rect bounds) =>
        _mouseHelper?.ButtonsStateChanged(point);

    private void SetMouseCursor() => MouseCursor = MouseType != MouseType.None && _preferences.Mouse.IsStandardMousePointerHidden
        ? Cursor.Parse("None")
        : Cursor.Default;
}