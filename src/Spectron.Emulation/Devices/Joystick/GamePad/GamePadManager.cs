using System.Collections.ObjectModel;
using OldBit.JoyPad;

namespace OldBit.Spectron.Emulation.Devices.Joystick.GamePad;

public sealed record GamePadController(Guid Id, string Name);

public sealed class GamePadManager
{
    private readonly JoyPadManager _joyPadManager;
    private bool _initialized;

    public ObservableCollection<GamePadController> GamePadControllers { get; } = [];

    public GamePadManager()
    {
        _joyPadManager = new JoyPadManager();

        _joyPadManager.ControllerConnected += JoyPadManagerOnControllerConnected;
        _joyPadManager.ControllerDisconnected += JoyPadManagerOnControllerDisconnected;

        GamePadControllers.Add(new GamePadController(Guid.Empty, "None"));
    }

    private void JoyPadManagerOnControllerConnected(object? sender, ControllerEventArgs e)
    {
        if (GamePadControllers.Any(x => x.Id == e.Controller.Id))
        {
            return;
        }

        GamePadControllers.Add(new GamePadController(e.Controller.Id, e.Controller.Name));
    }

    private void JoyPadManagerOnControllerDisconnected(object? sender, ControllerEventArgs e)
    {
        var existingController = GamePadControllers.FirstOrDefault(x => x.Id == e.Controller.Id);

        if (existingController != null)
        {
            GamePadControllers.Remove(existingController);
        }
    }
    public void Initialize()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;

        _joyPadManager.StartListener();
    }

    public void Stop()
    {
        _joyPadManager.StopListener();
        _joyPadManager.Dispose();
    }
}