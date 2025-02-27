using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Timers;
using Avalonia.Threading;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices.Joystick;
using OldBit.Spectron.Models;
using ReactiveUI;

namespace OldBit.Spectron.ViewModels;

public class StatusBarViewModel : ReactiveObject
{
    private RecordingStatus _recordingStatus = RecordingStatus.None;

    private string _framesPerSecond = "0";
    private string _message = string.Empty;
    private string _timeElapsed = "";
    private ComputerType _computerType;
    private JoystickType _joystickType;
    private string _speed = "100";

    private readonly ObservableAsPropertyHelper<string> _computerName;
    private readonly ObservableAsPropertyHelper<string> _joystickName;

    private readonly Timer _timer;
    private readonly Stopwatch _stopwatch = new();

    public Action AnimateQuickSave { get; set; } = () => { };

    public StatusBarViewModel()
    {
        _timer = new Timer(1000);
        _timer.Elapsed += UpdateRecordingTime;

        this.WhenAny(x => x.RecordingStatus, x => x.Value)
            .Subscribe(status =>
            {
                switch (status)
                {
                    case RecordingStatus.None:
                        UpdateProcessingStatus(false);
                        break;

                    case RecordingStatus.Recording:
                        UpdateProcessingStatus(true, "Recording");
                        break;

                    case RecordingStatus.Processing:
                        UpdateProcessingStatus(true, "Processing");
                        break;
                }
            });

        this.WhenAnyValue(x => x.ComputerType).Select(computerType => computerType switch
        {
            ComputerType.Spectrum16K => "16k",
            ComputerType.Spectrum48K => "48k",
            ComputerType.Spectrum128K => "128k",
            ComputerType.Timex2048 => "Timex",
            _ => "Unknown"
        }).ToProperty(this, x => x.ComputerName, out _computerName);

        this.WhenAnyValue(x => x.JoystickType).Select(joystickType => joystickType switch
        {
            JoystickType.None => "None",
            JoystickType.Kempston => "Kempston",
            JoystickType.Sinclair1 => "Sinclair 1",
            JoystickType.Sinclair2 => "Sinclair 2",
            JoystickType.Cursor => "Cursor",
            JoystickType.Fuller => "Fuller",
            _ => "Unknown"
        }).ToProperty(this, x => x.JoystickName, out _joystickName);
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

    private void UpdateRecordingTime(object? sender, ElapsedEventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
            TimeElapsed = _stopwatch.Elapsed.ToString(@"hh\:mm\:ss"));
    }

    public ComputerType ComputerType
    {
        get => _computerType;
        set => this.RaiseAndSetIfChanged(ref _computerType, value);
    }

    public string ComputerName => _computerName.Value;

    public JoystickType JoystickType
    {
        get => _joystickType;
        set => this.RaiseAndSetIfChanged(ref _joystickType, value);
    }

    public string JoystickName => _joystickName.Value;

    public string FramesPerSecond
    {
        get => _framesPerSecond;
        set => this.RaiseAndSetIfChanged(ref _framesPerSecond, value);
    }

    public RecordingStatus RecordingStatus
    {
        get => _recordingStatus;
        set => this.RaiseAndSetIfChanged(ref _recordingStatus, value);
    }

    public string Message
    {
        get => _message;
        set => this.RaiseAndSetIfChanged(ref _message, value);
    }

    public string TimeElapsed
    {
        get => _timeElapsed;
        set => this.RaiseAndSetIfChanged(ref _timeElapsed, value);
    }

    public string Speed
    {
        get => _speed;
        set => this.RaiseAndSetIfChanged(ref _speed, value);
    }
}