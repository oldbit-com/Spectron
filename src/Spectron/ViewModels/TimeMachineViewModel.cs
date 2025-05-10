using System;
using System.Reactive;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Commands;
using OldBit.Spectron.Emulation.Devices.Gamepad;
using OldBit.Spectron.Emulation.Devices.Joystick;
using OldBit.Spectron.Emulation.Extensions;
using ReactiveUI;

namespace OldBit.Spectron.ViewModels;

public class TimeMachineViewModel : ReactiveObject, IDisposable
{
    private readonly TimeMachine _timeMachine;
    private readonly JoystickManager _joystickManager;
    private readonly CommandManager _commandManager;
    private readonly ILogger _logger;
    private const int PreviewHeight = 192;
    private const int PreviewWidth = 256;

    public ReactiveCommand<Unit, TimeMachineEntry?> TimeTravelCommand { get; private set; }
    public Control? PreviewControl { get; set; }
    public Action Close { get; set; } = () => { };

    public TimeMachineViewModel(
        TimeMachine timeMachine,
        JoystickManager joystickManager,
        CommandManager commandManager,
        ILogger logger)
    {
        _timeMachine = timeMachine;
        _joystickManager = joystickManager;
        _commandManager = commandManager;
        _logger = logger;

        TimeTravelCommand = ReactiveCommand.Create(HandleTimeTravel);

        this.WhenAny(x => x.CurrentEntryIndex, x => x.Value)
            .Subscribe(_ => UpdatePreview());

        EntriesCount = _timeMachine.Entries.Count - 1;
        CurrentEntryIndex = EntriesCount;

        _joystickManager.JoystickButtonChanged += JoystickManagerOnJoystickButtonChanged;
        _commandManager.CommandReceived += CommandManagerOnCommandReceived;
    }

    private void JoystickManagerOnJoystickButtonChanged(object? sender, JoystickButtonEventArgs e)
    {
        Console.WriteLine($"Input: {e.Input} State: {e.State}");
    }

    private void CommandManagerOnCommandReceived(object? sender, CommandEventArgs e)
    {
        if (e.Command is not GamepadActionCommand gamepadCommand)
        {
            return;
        }

        if (gamepadCommand.State == InputState.Pressed)
        {
            return;
        }

        if (gamepadCommand.Action == GamepadAction.TimeTravel)
        {
            Dispatcher.UIThread.Post(() => Close.Invoke());
        }
    }

    private TimeMachineEntry? HandleTimeTravel()
    {
        if (CurrentEntryIndex >= EntriesCount)
        {
            return null;
        }

        var timeMachineEntry = GetSelectedEntry();

        return timeMachineEntry ?? null;
    }

    private void UpdatePreview()
    {
        try
        {
            var timeMachineEntry = GetSelectedEntry();

            if (timeMachineEntry == null)
            {
                return;
            }

            var snapshot = timeMachineEntry.GetSnapshot();

            if (snapshot == null)
            {
                return;
            }

            var screenshot = snapshot.GetScreenshot();

            using (var bitmap = ScreenPreview.Lock())
            {
                Marshal.Copy(screenshot, 0, bitmap.Address, screenshot.Length);
            }

            ScreenBorderBrush = new SolidColorBrush(snapshot.BorderColor.Argb);

            PreviewControl?.InvalidateVisual();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update preview");
        }
    }

    private TimeMachineEntry? GetSelectedEntry()
    {
        var index = (int)CurrentEntryIndex;

        if (index >= 0 && index < _timeMachine.Entries.Count)
        {
            return _timeMachine.Entries[index];
        }

        return null;
    }

    private int _entriesCount;
    public int EntriesCount
    {
        get => _entriesCount;
        set => this.RaiseAndSetIfChanged(ref _entriesCount, value);
    }

    private double _currentEntryIndex;
    public double CurrentEntryIndex
    {
        get => _currentEntryIndex;
        set => this.RaiseAndSetIfChanged(ref _currentEntryIndex, value);
    }

    private WriteableBitmap _screenPreview = new(
        new PixelSize(PreviewWidth, PreviewHeight),
        new Vector(96, 96),
        PixelFormats.Rgba8888);

    public WriteableBitmap ScreenPreview
    {
        get => _screenPreview;
        set => this.RaiseAndSetIfChanged(ref _screenPreview, value);
    }

    private Brush _screenBorderBrush = new SolidColorBrush(Colors.Black);
    public Brush ScreenBorderBrush
    {
        get => _screenBorderBrush;
        set => this.RaiseAndSetIfChanged(ref _screenBorderBrush, value);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        _joystickManager.JoystickButtonChanged -= JoystickManagerOnJoystickButtonChanged;
        _commandManager.CommandReceived -= CommandManagerOnCommandReceived;

        _screenPreview.Dispose();
        TimeTravelCommand.Dispose();
    }
}