using System.Collections.ObjectModel;
using OldBit.JoyPad;
using OldBit.JoyPad.Controls;

namespace OldBit.Spectron.Emulation.Devices.Joystick.Gamepad;

public record GamepadPreferences(Guid ControllerId, JoystickType JoystickType, List<GamepadMapping> Mappings);

public sealed class GamepadManager
{
    private readonly JoyPadManager _joyPadManager;
    private readonly List<Guid> _enabledControllers = [];

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

    public void SetupGamepad(GamepadPreferences player1, GamepadPreferences player2)
    {
        _enabledControllers.Clear();

        if ((player1.ControllerId == Guid.Empty || player1.JoystickType == JoystickType.None) &&
            (player2.ControllerId == Guid.Empty || player2.JoystickType == JoystickType.None))
        {
            return;
        }

        if (GamepadControllers.Any(x => x.Id == player1.ControllerId))
        {
            _enabledControllers.Add(player1.ControllerId);
        }
    }

    public void Update()
    {
        foreach (var controllerId in _enabledControllers)
        {
            _joyPadManager.Update(controllerId);
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