using System.Collections.Generic;
using System.Linq;
using OldBit.Spectron.Emulation.Devices.Joystick.Gamepad;
using ReactiveUI;

namespace OldBit.Spectron.ViewModels;

public record GamepadActionMapping(string Name, GamepadAction Action);

public record GamepadActionHeading(string Name) : GamepadActionMapping(Name, GamepadAction.None) { }

public class GamepadButtonMappingViewModel(
    GamepadButton button,
    GamepadActionMapping selectedAction,
    List<GamepadActionMapping> actions) : ViewModelBase
{
    public GamepadButton Button { get; } = button;
    public string Name { get; } = button.Name;
    public List<GamepadActionMapping> Actions { get; } = actions;

    private GamepadActionMapping _selectedAction = selectedAction;
    public GamepadActionMapping SelectedAction
    {
        get => _selectedAction;
        set => this.RaiseAndSetIfChanged(ref _selectedAction, value);
    }
}