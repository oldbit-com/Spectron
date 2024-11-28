using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Timers;
using OldBit.JoyPad.Controls;
using OldBit.Spectron.Emulation.Devices.Joystick.Gamepad;
using OldBit.Spectron.Settings;
using ReactiveUI;

namespace OldBit.Spectron.ViewModels;

public class GamepadMappingViewModel : ViewModelBase, IDisposable
{
    private readonly Timer _timer;
    private readonly GamepadController _controller;
    private readonly GamepadManager _gamepadManager;

    public ReactiveCommand<Unit, GamepadSettings> UpdateMappingCommand { get; }

    public IReadOnlyList<GamepadButtonMappingViewModel> Mappings { get; }

    public GamepadMappingViewModel(
        GamepadController controller,
        GamepadManager gamepadManager,
        GamepadSettings settings)
    {
        _controller = controller;
        _gamepadManager = gamepadManager;

        settings.Mappings ??= DefaultMappings(controller.Buttons);

        var actions = GetAllActions().ToList();

        Mappings = controller.Buttons
            .Select(button =>
                new GamepadButtonMappingViewModel(
                    button,
                    actions.FirstOrDefault(
                        mapping => mapping.Action == settings.Mappings.FirstOrDefault(
                            g => g.ButtonId == button.Id &&
                                 g.Direction == button.Direction)?.Action,
                        actions.First()),
                    actions))
            .ToList();

        UpdateMappingCommand = ReactiveCommand.Create(UpdateMapping);

        _timer = new Timer(100) { AutoReset = false };
        _timer.Elapsed += GamepadUpdate;
        _timer.Start();
    }

    private void GamepadUpdate(object? sender, ElapsedEventArgs e)
    {
        _gamepadManager.Update(_controller.Id);
        _timer.Start();
    }

    private static IEnumerable<GamepadActionMappingItem> GetAllActions()
    {
        var actions = Enum.GetValues<GamepadAction>();

        foreach (var action in actions)
        {
            if (action == GamepadAction.D0)
            {
                yield return new GamepadActionMappingSeparatorItem("Keys");
            }

            yield return new GamepadActionMappingItem(action.GetName(), action);

            if (action == GamepadAction.None)
            {
                yield return new GamepadActionMappingSeparatorItem("Joystick");
            }
        }
    }

    private GamepadSettings UpdateMapping() => new()
    {
        Mappings = Mappings
            .Where(m => m.SelectedAction.Action != GamepadAction.None)
            .Select(m => new GamepadMapping(m.Button, m.SelectedAction.Action))
            .ToList()
    };

    private static List<GamepadMapping> DefaultMappings(IReadOnlyList<GamepadButton> buttons)
    {
        var mappings = new List<GamepadMapping>();

        var button = buttons.FirstOrDefault(x => x.Name == "A") ??
                     buttons.FirstOrDefault(x => x.Name == "Button 1");

        if (button != null)
        {
            mappings.Add(new GamepadMapping(button, GamepadAction.JoystickFire));
        }

        var dpadButtons = buttons.Where(x => x.Direction != DirectionalPadDirection.None);

        foreach (var dpadButton in dpadButtons)
        {
            var action = dpadButton.Direction switch
            {
                DirectionalPadDirection.Left => GamepadAction.JoystickLeft,
                DirectionalPadDirection.Up => GamepadAction.JoystickUp,
                DirectionalPadDirection.Right => GamepadAction.JoystickRight,
                DirectionalPadDirection.Down => GamepadAction.JoystickDown,
                _ => GamepadAction.None
            };

            mappings.Add(new GamepadMapping(dpadButton, action));
        }

        return mappings;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        _timer.Stop();
        _timer.Dispose();
    }
}