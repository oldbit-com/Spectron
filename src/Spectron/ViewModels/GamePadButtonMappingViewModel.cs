using System.Collections.Generic;
using System.Linq;
using OldBit.Spectron.Emulation.Devices.Joystick.GamePad;
using ReactiveUI;

namespace OldBit.Spectron.ViewModels;

public record GamePadActionMapping(string Name, GamePadAction Action);

public record GamePadActionHeading(string Name) : GamePadActionMapping(Name, GamePadAction.None) { }

public class GamePadButtonMappingViewModel(
    GamePadButton button,
    GamePadActionMapping selectedAction,
    List<GamePadActionMapping> actions) : ViewModelBase
{
    public GamePadButton Button { get; } = button;
    public string Name { get; } = button.Name;
    public List<GamePadActionMapping> Actions { get; } = actions;

    private GamePadActionMapping _selectedAction = selectedAction;
    public GamePadActionMapping SelectedAction
    {
        get => _selectedAction;
        set => this.RaiseAndSetIfChanged(ref _selectedAction, value);
    }
}