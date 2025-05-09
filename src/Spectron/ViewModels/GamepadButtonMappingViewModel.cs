using System.Collections.Generic;
using OldBit.Spectron.Emulation.Devices.Gamepad;
using ReactiveUI;

namespace OldBit.Spectron.ViewModels;

public record GamepadActionMappingItem(string Name, GamepadAction Action);

public record GamepadActionMappingSeparatorItem(string Name) : GamepadActionMappingItem(Name, GamepadAction.None) { }

public class GamepadButtonMappingViewModel(
    GamepadButton button,
    GamepadActionMappingItem selectedAction,
    List<GamepadActionMappingItem> actions) : ReactiveObject
{
    public GamepadButton Button { get; } = button;

    public string Name { get; } = button.Name;

    public List<GamepadActionMappingItem> Actions { get; } = actions;

    private GamepadActionMappingItem _selectedAction = selectedAction;
    public GamepadActionMappingItem SelectedAction
    {
        get => _selectedAction;
        set => this.RaiseAndSetIfChanged(ref _selectedAction, value);
    }
}