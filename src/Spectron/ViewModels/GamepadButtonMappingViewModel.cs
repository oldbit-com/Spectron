using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using OldBit.Spectron.Emulation.Devices.Gamepad;

namespace OldBit.Spectron.ViewModels;

public record GamepadActionMappingItem(string Name, GamepadAction Action);

public record GamepadActionMappingSeparatorItem(string Name) : GamepadActionMappingItem(Name, GamepadAction.None) { }

public partial class GamepadButtonMappingViewModel(
    GamepadButton button,
    GamepadActionMappingItem selectedAction,
    List<GamepadActionMappingItem> actions) : ObservableObject
{
    [ObservableProperty]
    private GamepadActionMappingItem _selectedAction = selectedAction;

    public GamepadButton Button { get; } = button;

    public string Name { get; } = button.Name;

    public List<GamepadActionMappingItem> Actions { get; } = actions;
}