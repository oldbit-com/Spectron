using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using OldBit.Spectron.Emulation.Devices.Joystick.Gamepad;
using OldBit.Spectron.Settings;
using ReactiveUI;

namespace OldBit.Spectron.ViewModels;

public class GamepadMappingViewModel : ViewModelBase
{
    private readonly GamepadManager _manager;

    public ReactiveCommand<Unit, GamepadSettings> UpdateMappingCommand { get; }

    public IReadOnlyList<GamepadButtonMappingViewModel> Mappings { get; private set; }

    public GamepadMappingViewModel(
        GamepadController controller,
        GamepadManager manager,
        GamepadSettings settings)
    {
        _manager = manager;

        var currentMappings = settings.Mappings.ToDictionary(x => x.ButtonId, y => y.Action);

        var actions = GetAllActions().ToList();

        Mappings = controller.Buttons.Select(button =>
            new GamepadButtonMappingViewModel(
                button,
                actions.FirstOrDefault(
                    a => a.Action == (currentMappings.GetValueOrDefault(button.Id))) ?? actions.First(),
                actions))
            .ToList();

        UpdateMappingCommand = ReactiveCommand.Create(UpdateMapping);
    }

    private static IEnumerable<GamepadActionMapping> GetAllActions()
    {
        var actions = Enum.GetValues<GamepadAction>();

        foreach (var action in actions)
        {
            if (action == GamepadAction.D0)
            {
                yield return new GamepadActionHeading("Keys");
            }

            yield return new GamepadActionMapping(action.GetName(), action);

            if (action == GamepadAction.None)
            {
                yield return new GamepadActionHeading("Joystick");
            }
        }
    }

    private GamepadSettings UpdateMapping() => new()
    {
        Mappings = Mappings
            .Where(m => m.SelectedAction.Action != GamepadAction.None)
            .Select(m => new GamepadMapping(m.Button.Id, m.SelectedAction.Action))
            .ToList()
    };
}