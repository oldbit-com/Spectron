using System;
using System.Diagnostics;
using System.Timers;
using Avalonia.Threading;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices.Joystick;
using OldBit.Spectron.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using OldBit.Spectron.Emulation.Devices.Audio;

namespace OldBit.Spectron.ViewModels;

public partial class StatusBarViewModel : ObservableObject
{
    [ObservableProperty]
    private RecordingStatus _recordingStatus = RecordingStatus.None;

    [ObservableProperty]
    private string _framesPerSecond = "0";

    [ObservableProperty]
    private string _message = string.Empty;

    [ObservableProperty]
    private string _timeElapsed = "";

    [ObservableProperty]
    private ComputerType _computerType;

    [ObservableProperty]
    private JoystickType _joystickType;

    [ObservableProperty]
    private string _speed = "100";

    [ObservableProperty]
    private bool _isDivMmcEnabled;

    [ObservableProperty]
    private bool _isMouseEnabled;

    [ObservableProperty]
    private bool _isPrinterEnabled;

    [ObservableProperty]
    private bool _isUlaPlusEnabled;

    [ObservableProperty]
    private bool _isTapeLoaded;

    [ObservableProperty]
    private string _tapeLoadProgress = string.Empty;

    [ObservableProperty]
    private bool _isAyEnabled;

    [ObservableProperty]
    private StereoMode _stereoMode;

    [ObservableProperty]
    private string _toolTipAy = string.Empty;

    private readonly Timer _timer;
    private readonly Stopwatch _stopwatch = new();

    public string ComputerName => ComputerType switch
    {
        ComputerType.Spectrum16K => "16k",
        ComputerType.Spectrum48K => "48k",
        ComputerType.Spectrum128K => "128k",
        ComputerType.Timex2048 => "Timex",
        _ => "Unknown"
    };

    public string JoystickName => JoystickType switch
    {
        JoystickType.None => "None",
        JoystickType.Kempston => "Kempston",
        JoystickType.Sinclair1 => "Sinclair 1",
        JoystickType.Sinclair2 => "Sinclair 2",
        JoystickType.Cursor => "Cursor",
        JoystickType.Fuller => "Fuller",
        _ => "Unknown"
    };

    public Action AnimateQuickSave { get; set; } = () => {};

    public StatusBarViewModel()
    {
        _timer = new Timer(1000);
        _timer.Elapsed += UpdateRecordingTime;
    }

    partial void OnComputerTypeChanged(ComputerType value) => OnPropertyChanged(nameof(ComputerName));

    partial void OnJoystickTypeChanged(JoystickType value) => OnPropertyChanged(nameof(JoystickName));

    partial void OnRecordingStatusChanged(RecordingStatus value)
    {
        switch (value)
        {
            case RecordingStatus.Recording:
                UpdateProcessingStatus(true, "Recording");
                break;

            case RecordingStatus.Processing:
                UpdateProcessingStatus(true, "Processing");
                break;

            default:
                UpdateProcessingStatus(false);
                break;
        }
    }

    partial void OnStereoModeChanged(StereoMode value)
    {
        ToolTipAy = value switch
        {
            StereoMode.Mono => "AY Mono",
            StereoMode.StereoABC => "AY Stereo ABC",
            StereoMode.StereoACB => "AY Stereo ACB",
            _ => ToolTipAy
        };
    }

    private void UpdateProcessingStatus(bool isActive, string message = "")
    {
        TimeElapsed = string.Empty;
        Message = message;

        _timer.Enabled = isActive;

        if (isActive)
        {
            _stopwatch.Restart();
        }
        else
        {
            _stopwatch.Stop();
        }
    }

    private void UpdateRecordingTime(object? sender, ElapsedEventArgs e) =>
        Dispatcher.UIThread.InvokeAsync(() => TimeElapsed = _stopwatch.Elapsed.ToString(@"hh\:mm\:ss"));
}