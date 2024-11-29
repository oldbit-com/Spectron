using System.Collections.ObjectModel;
using OldBit.JoyPad;
using OldBit.JoyPad.Controls;

namespace OldBit.Spectron.Emulation.Devices.Joystick.Gamepad;

public record GamepadPreferences(Guid ControllerId, JoystickType JoystickType, List<GamepadMapping> Mappings);

public sealed class GamepadManager
{
    private readonly JoyPadManager _joyPadManager;
    private readonly List<GamepadPreferences> _enabledGamepads = [];

    private bool _initialized;

    public ObservableCollection<GamepadController> GamepadControllers { get; } = [];

    public GamepadManager()
    {
        _joyPadManager = new JoyPadManager();

        _joyPadManager.ControllerConnected += JoyPadManagerOnControllerConnected;
        _joyPadManager.ControllerDisconnected += JoyPadManagerOnControllerDisconnected;

        GamepadControllers.Add(GamepadController.None);
    }

    public void Initialize()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        _joyPadManager.Start();
    }

    public void Stop()
    {
        _joyPadManager.Stop();
        _joyPadManager.Dispose();
    }

    public void SetupGamepad(GamepadPreferences gamepad)
    {
        _enabledGamepads.Clear();

        if ((gamepad.ControllerId == Guid.Empty || gamepad.JoystickType == JoystickType.None))
        {
            return;
        }

        if (GamepadControllers.Any(x => x.Id == gamepad.ControllerId))
        {
            _enabledGamepads.Add(gamepad);
        }
    }

    public void Update()
    {
        foreach (var gamepad in _enabledGamepads)
        {
            _joyPadManager.Update(gamepad.ControllerId);
        }
    }

    public void Update(Guid controllerId) => _joyPadManager.Update(controllerId);

    private void JoyPadManagerOnControllerConnected(object? sender, JoyPadControllerEventArgs e)
    {
        if (GamepadControllers.Any(x => x.Id == e.Controller.Id))
        {
            return;
        }

        var buttons = e.Controller.Controls
            .Where(control => control.ControlType is ControlType.Button)
            .Select(button => new GamepadButton(button.Id, button.Name))
            .ToList();

        var dpad = e.Controller.Controls
            .FirstOrDefault(control => control.ControlType is ControlType.DirectionalPad);

        if (dpad != null)
        {
            buttons.Insert(0, new GamepadButton(dpad.Id, "D-Pad Left", DirectionalPadDirection.Left));
            buttons.Insert(1, new GamepadButton(dpad.Id, "D-Pad Up", DirectionalPadDirection.Up));
            buttons.Insert(2, new GamepadButton(dpad.Id, "D-Pad Right", DirectionalPadDirection.Right));
            buttons.Insert(3, new GamepadButton(dpad.Id, "D-Pad Down", DirectionalPadDirection.Down));
        }

        GamepadControllers.Add(new GamepadController(e.Controller, buttons));
    }

    private void JoyPadManagerOnControllerDisconnected(object? sender, JoyPadControllerEventArgs e)
    {
        var existingController = GamepadControllers.FirstOrDefault(x => x.Id == e.Controller.Id);

        if (existingController != null)
        {
            GamepadControllers.Remove(existingController);
        }
    }
}