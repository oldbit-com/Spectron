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

        GamePadControllers.Add(new GamePadController(Guid.Empty, "None"));
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