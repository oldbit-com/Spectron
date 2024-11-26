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
    private readonly GamePadManager _gamePadManager;

    public ReactiveCommand<Unit, GamePadSettings> UpdateGamePadMappingCommand { get; }

    public GamePadMappingViewModel(GamePadController controller, GamePadManager gamePadManager)
    {
        _gamePadManager = gamePadManager;

        var actions = GetAllActions().ToList();

        Mappings = controller.Buttons.Select(button => new GamePadControlMappingViewModel(
            button.Name,
            actions)).ToList();

        UpdateGamePadMappingCommand = ReactiveCommand.Create(UpdateGamePadMapping);

    }

    public IReadOnlyList<GamePadControlMappingViewModel> Mappings { get; private set; }

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

    private GamePadSettings UpdateGamePadMapping()
    {
        return new GamePadSettings();
    }
}