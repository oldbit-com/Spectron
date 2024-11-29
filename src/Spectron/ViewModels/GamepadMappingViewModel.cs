using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Timers;
using DynamicData;
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

    public ReactiveCommand<Unit, List<GamepadMapping>> UpdateMappingCommand { get; }
    public ReactiveCommand<Unit, Unit> SetDefaultMappingCommand { get; }

    public ObservableCollection<GamepadButtonMappingViewModel> Mappings { get; } = [];

    public GamepadMappingViewModel(
        GamepadController controller,
        GamepadManager gamepadManager,
        GamepadSettings settings)
    {
        _controller = controller;
        _gamepadManager = gamepadManager;

        if (!settings.MappingsByController.TryGetValue(controller.Id, out var mappings))
        {
            mappings = DefaultMappings();
        }

        SetupGridView(mappings);

        UpdateMappingCommand = ReactiveCommand.Create(GetConfiguredMappings);
        SetDefaultMappingCommand = ReactiveCommand.Create(() => { SetupGridView(DefaultMappings()); });

        _timer = new Timer(100) { AutoReset = false };
        _timer.Elapsed += GamepadUpdate;
        _timer.Start();
    }

    private void SetupGridView(List<GamepadMapping> mappings)
    {
        var actions = GetAllActions().ToList();

        var mappedViewModels = _controller.Buttons
            .Select(button =>
                new GamepadButtonMappingViewModel(
                    button,
                    actions.FirstOrDefault(
                        mapping => mapping.Action == mappings.FirstOrDefault(
                            g => g.ButtonId == button.ButtonId &&
                                 g.Direction == button.Direction)?.Action,
                        actions.First()),
                    actions));

        Mappings.Clear();
        Mappings.AddOrInsertRange(mappedViewModels, 0);
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

    private List<GamepadMapping> GetConfiguredMappings() => Mappings
        .Where(m => m.SelectedAction.Action != GamepadAction.None)
        .Select(m => new GamepadMapping(m.Button, m.SelectedAction.Action))
        .ToList();

    private List<GamepadMapping> DefaultMappings()
    {
        var mappings = _controller.Buttons
            .Where(x => x.Name is "Button 1" or "Button 2" or "Button 3" or "Button 4" or "A" or "B" or "X" or "Y")
            .Select(button => new GamepadMapping(button, GamepadAction.JoystickFire))
            .ToList();

        var dpadButtons = _controller.Buttons.Where(x => x.Direction != DirectionalPadDirection.None);

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