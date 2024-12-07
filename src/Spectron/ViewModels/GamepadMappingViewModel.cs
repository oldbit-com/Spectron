using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Timers;
using Avalonia.Threading;
using DynamicData;
using OldBit.Joypad.Controls;
using OldBit.Spectron.Emulation.Devices.Joystick.Gamepad;
using OldBit.Spectron.Settings;
using ReactiveUI;

namespace OldBit.Spectron.ViewModels;

public class GamepadMappingViewModel : ViewModelBase, IDisposable
{
    private readonly Timer _timer;
    private readonly GamepadManager _gamepadManager;

    private GamepadController _controller = GamepadController.None;

    public ReactiveCommand<Unit, Unit> SetDefaultMappingCommand { get; }

    public ObservableCollection<GamepadButtonMappingViewModel> Mappings { get; } = [];

    private GamepadButtonMappingViewModel? _selectedMapping;
    public GamepadButtonMappingViewModel? SelectedMapping
    {
        get => _selectedMapping;
        set => this.RaiseAndSetIfChanged(ref _selectedMapping, value);
    }

    public Action<GamepadButtonMappingViewModel> ScrollIntoView { get; set; } = _ => { };

    public GamepadMappingViewModel(GamepadManager gamepadManager)
    {
        _gamepadManager = gamepadManager;

        SetDefaultMappingCommand = ReactiveCommand.Create(() =>
        {
            var defaultMappings = DefaultMappings();
            SetupGridView(defaultMappings);
        });

        _timer = new Timer(100) { AutoReset = false };
        _timer.Elapsed += GamepadUpdate;
        _timer.Start();
    }

    public void UpdateView(Guid controllerId, GamepadSettings settings)
    {
        _controller.ValueChanged -= ControllerOnValueChanged;

        _controller = _gamepadManager.Controllers
            .FirstOrDefault(controller => controller.ControllerId == controllerId, GamepadController.None);

        _controller.ValueChanged += ControllerOnValueChanged;

        if (!settings.Mappings.TryGetValue(controllerId, out var mappings))
        {
            mappings = DefaultMappings();
        }

        SetupGridView(mappings);
    }

    private void ControllerOnValueChanged(object? sender, ValueChangedEventArgs e)
    {
        if (!e.IsPressed)
        {
            return;
        }

        var mapping = Mappings.FirstOrDefault(m => m.Button.ButtonId == e.ControlId &&
                                                   m.Button.Direction == e.Direction);
        if (mapping == null)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            SelectedMapping = mapping;
            ScrollIntoView(mapping);
        });
    }

    public List<GamepadMapping> GetConfiguredMappings() => Mappings
        .Where(m => m.SelectedAction.Action != GamepadAction.None)
        .Select(m => new GamepadMapping(m.Button, m.SelectedAction.Action))
        .ToList();

    private void SetupGridView(List<GamepadMapping> mappings)
    {
        var actions = GetAllActions().ToList();

        var mappedViewModels = _controller.Buttons
            .Select(button =>
                new GamepadButtonMappingViewModel(
                    button,
                    actions.FirstOrDefault(
                        mapping => mapping.Action == mappings.FirstOrDefault(
                            g => g.ControlId == button.ButtonId &&
                                 g.Direction == button.Direction)?.Action,
                        actions.First()),
                    actions));

        Mappings.Clear();
        Mappings.AddOrInsertRange(mappedViewModels, 0);
    }

    private void GamepadUpdate(object? sender, ElapsedEventArgs e)
    {
        if (_controller != GamepadController.None)
        {
            _gamepadManager.Update(_controller.ControllerId);
        }

        _timer.Start();
    }

    private static IEnumerable<GamepadActionMappingItem> GetAllActions()
    {
        var actions = Enum.GetValues<GamepadAction>();

        foreach (var action in actions)
        {
            if (action == GamepadAction.Pause)
            {
                yield return new GamepadActionMappingSeparatorItem("Actions");
            }

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

        _controller.ValueChanged -= ControllerOnValueChanged;

        _timer.Stop();
        _timer.Dispose();
    }
}