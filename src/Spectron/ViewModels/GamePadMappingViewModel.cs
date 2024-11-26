using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using OldBit.Spectron.Emulation.Devices.Joystick.GamePad;
using OldBit.Spectron.Settings;
using ReactiveUI;

namespace OldBit.Spectron.ViewModels;

public class GamePadMappingViewModel : ViewModelBase
{
    private readonly GamePadManager _manager;

    public ReactiveCommand<Unit, GamePadSettings> UpdateMappingCommand { get; }

    public IReadOnlyList<GamePadButtonMappingViewModel> Mappings { get; private set; }

    public GamePadMappingViewModel(
        GamePadController controller,
        GamePadManager manager,
        GamePadSettings settings)
    {
        _manager = manager;

        var currentMappings = settings.Mappings.ToDictionary(x => x.ButtonId, y => y.Action);

        var actions = GetAllActions().ToList();

        Mappings = controller.Buttons.Select(button =>
            new GamePadButtonMappingViewModel(
                button,
                actions.FirstOrDefault(
                    a => a.Action == (currentMappings.GetValueOrDefault(button.Id))) ?? actions.First(),
                actions))
            .ToList();

        UpdateMappingCommand = ReactiveCommand.Create(UpdateMapping);
    }

    private static IEnumerable<GamePadActionMapping> GetAllActions()
    {
        var actions = Enum.GetValues<GamePadAction>();

        foreach (var action in actions)
        {
            if (action == GamePadAction.D0)
            {
                yield return new GamePadActionHeading("Keys");
            }

            yield return new GamePadActionMapping(action.GetName(), action);

            if (action == GamePadAction.None)
            {
                yield return new GamePadActionHeading("Joystick");
            }
        }
    }

    private GamePadSettings UpdateMapping() => new()
    {
        Mappings = Mappings
            .Where(m => m.SelectedAction.Action != GamePadAction.None)
            .Select(m => new GamePadMapping(m.Button.Id, m.SelectedAction.Action))
            .ToList()
    };
}