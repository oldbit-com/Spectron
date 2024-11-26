using System.Collections.ObjectModel;
using OldBit.JoyPad;

namespace OldBit.Spectron.Emulation.Devices.Joystick.Gamepad;

public sealed record GamepadController(Guid Id, string Name, IReadOnlyList<GamepadButton> Buttons);

public sealed class GamepadManager
{
    private readonly JoyPadManager _joyPadManager;
    private bool _initialized;

    public ObservableCollection<GamepadController> GamepadControllers { get; } = [];

    public GamepadManager()
    {
        _joyPadManager = new JoyPadManager();

        _joyPadManager.ControllerConnected += JoyPadManagerOnControllerConnected;
        _joyPadManager.ControllerDisconnected += JoyPadManagerOnControllerDisconnected;

        GamepadControllers.Add(new GamepadController(Guid.Empty, "None", []));
    }

    private void JoyPadManagerOnControllerConnected(object? sender, ControllerEventArgs e)
    {
        if (GamepadControllers.Any(x => x.Id == e.Controller.Id))
        {
            return;
        }

        var buttons = e.Controller.Controls.Where(x => x.ControlType == ControlType.Button)
            .Select(button => new GamepadButton(button.Id, button.Name));

        GamepadControllers.Add(new GamepadController(
            e.Controller.Id,
            e.Controller.Name,
            buttons.ToList()));
    }

    private void JoyPadManagerOnControllerDisconnected(object? sender, ControllerEventArgs e)
    {
        var existingController = GamepadControllers.FirstOrDefault(x => x.Id == e.Controller.Id);

        if (existingController != null)
        {
            GamepadControllers.Remove(existingController);
        }
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
}