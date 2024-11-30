using System.Collections.ObjectModel;
using OldBit.JoyPad;
using OldBit.JoyPad.Controls;

namespace OldBit.Spectron.Emulation.Devices.Joystick.Gamepad;

public record GamepadPreferences(Guid ControllerId, JoystickType JoystickType, List<GamepadMapping> Mappings);

public sealed class GamepadManager
{
    private readonly JoyPadManager _joyPadManager;
    private GamepadPreferences? _activeGamepad;

    private bool _initialized;

    public ObservableCollection<GamepadController> Controllers { get; } = [];

    public GamepadManager()
    {
        _joyPadManager = new JoyPadManager();

        _joyPadManager.ControllerConnected += JoyPadManagerOnControllerConnected;
        _joyPadManager.ControllerDisconnected += JoyPadManagerOnControllerDisconnected;

        Controllers.Add(GamepadController.None);
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

    public void Setup(GamepadPreferences gamepad)
    {
        if (gamepad.ControllerId == Guid.Empty || gamepad.JoystickType == JoystickType.None)
        {
            _activeGamepad = null;
            return;
        }

        _activeGamepad = gamepad;
    }

    public void Update()
    {
        if (_activeGamepad == null)
        {
            return;
        }

        _joyPadManager.Update(_activeGamepad.ControllerId);
    }

    public void Update(Guid controllerId) => _joyPadManager.Update(controllerId);

    private void JoyPadManagerOnControllerConnected(object? sender, JoyPadControllerEventArgs e)
    {
        if (Controllers.Any(controller => controller.Id == e.Controller.Id))
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

        Controllers.Add(new GamepadController(e.Controller, buttons));
    }

    private void JoyPadManagerOnControllerDisconnected(object? sender, JoyPadControllerEventArgs e)
    {
        var existingController = Controllers.FirstOrDefault(x => x.Id == e.Controller.Id);

        if (existingController != null)
        {
            Controllers.Remove(existingController);
        }
    }
}