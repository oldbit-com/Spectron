using System.Collections.Generic;
using System.Linq;
using OldBit.Spectron.Emulation.Devices.Joystick.GamePad;
using ReactiveUI;

namespace OldBit.Spectron.ViewModels;

public record GamePadActionMapping(string Name, GamePadAction Action);

public record GamePadActionHeading(string Name) : GamePadActionMapping(Name, GamePadAction.None) { }

public class GamePadControlMappingViewModel(string name, List<GamePadActionMapping> actions) : ViewModelBase
{
    public string Name { get; } = name;

    public List<GamePadActionMapping> Actions { get; } = actions;

    private GamePadActionMapping _selectedAction = actions.First();
    public GamePadActionMapping SelectedAction
    {
        get => _selectedAction;
        set => this.RaiseAndSetIfChanged(ref _selectedAction, value);
    }
}