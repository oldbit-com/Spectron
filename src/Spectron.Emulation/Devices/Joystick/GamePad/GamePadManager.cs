using System.Collections.ObjectModel;

namespace OldBit.Spectron.Emulation.Devices.Joystick.GamePad;

public sealed record GamePadController(Guid Id, string Name);

public sealed class GamePadManager
{
    public ObservableCollection<GamePadController> GamePadControllers { get; } = [];

    public GamePadManager()
    {
        GamePadControllers.Add(new GamePadController(Guid.Empty, "None"));
    }
}