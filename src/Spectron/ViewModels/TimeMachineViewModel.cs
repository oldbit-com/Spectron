using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Commands;
using OldBit.Spectron.Emulation.Devices.Gamepad;
using OldBit.Spectron.Emulation.Devices.Joystick;
using OldBit.Spectron.Emulation.Extensions;
using OldBit.Spectron.Emulation.TimeTravel;
using OldBit.Spectron.Messages;

namespace OldBit.Spectron.ViewModels;

public partial class TimeMachineViewModel : ObservableObject, IDisposable
{
    private readonly TimeMachine _timeMachine;
    private readonly JoystickManager _joystickManager;
    private readonly CommandManager _commandManager;
    private readonly ILogger _logger;

    private const int PreviewHeight = 192;
    private const int PreviewWidth = 256;

    [ObservableProperty]
    private int _entriesCount;

    [ObservableProperty]
    private double _currentEntryIndex;

    [ObservableProperty]
    private WriteableBitmap _screenPreview;

    [ObservableProperty]
    private Brush _screenBorderBrush;

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

        _screenPreview = new WriteableBitmap(
            new PixelSize(PreviewWidth, PreviewHeight),
            new Vector(96, 96),
            PixelFormats.Rgba8888);

        _screenBorderBrush = new SolidColorBrush(Colors.Black);

        EntriesCount = _timeMachine.Entries.Count - 1;
        CurrentEntryIndex = EntriesCount;

        _joystickManager.JoystickButtonChanged += JoystickManagerOnJoystickButtonChanged;
        _commandManager.CommandReceived += CommandManagerOnCommandReceived;
    }

    [RelayCommand]
    private void TimeTravel()
    {
        if (CurrentEntryIndex >= EntriesCount)
        {
            return;
        }

        var timeMachineEntry = GetSelectedEntry();

        if (timeMachineEntry != null)
        {
            WeakReferenceMessenger.Default.Send(new TimeTravelMessage(timeMachineEntry));
        }
    }

    partial void OnCurrentEntryIndexChanged(double value) => UpdatePreview();

    private void JoystickManagerOnJoystickButtonChanged(object? sender, JoystickButtonEventArgs e)
    {
        // TODO: Remove once implemented
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

        if (gamepadCommand.Action == GamepadAction.Rewind)
        {
            Dispatcher.UIThread.Post(() => Close.Invoke());
        }
    }

    private void UpdatePreview()
    {
        try
        {
            var timeMachineEntry = GetSelectedEntry();

            var snapshot = timeMachineEntry?.GetSnapshot();

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

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        _joystickManager.JoystickButtonChanged -= JoystickManagerOnJoystickButtonChanged;
        _commandManager.CommandReceived -= CommandManagerOnCommandReceived;

        ScreenPreview?.Dispose();
    }
}