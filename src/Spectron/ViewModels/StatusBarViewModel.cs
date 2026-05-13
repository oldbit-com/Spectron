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
    public partial RecordingStatus RecordingStatus { get; set; } = RecordingStatus.None;

    [ObservableProperty]
    public partial string FramesPerSecond { get; set; } = "0";

    [ObservableProperty]
    public partial string Message { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string TimeElapsed { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ComputerName))]
    public partial ComputerType ComputerType { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(JoystickName))]
    public partial JoystickType JoystickType { get; set; }

    [ObservableProperty]
    public partial string Speed { get; set; } = "100%";

    [ObservableProperty]
    public partial string Clock { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsDivMmcEnabled { get; set; }

    [ObservableProperty]
    public partial bool IsMouseEnabled { get; set; }

    [ObservableProperty]
    public partial bool IsPrinterEnabled { get; set; }

    [ObservableProperty]
    public partial bool IsUlaPlusEnabled { get; set; }

    [ObservableProperty]
    public partial bool IsTapeInserted { get; set; }

    [ObservableProperty]
    public partial bool IsRzxPlaying { get; set; }

    [ObservableProperty]
    public partial bool IsMicroDriveCartridgeInserted { get; set; }

    [ObservableProperty]
    public partial bool IsFloppyDiskInserted { get; set; }

    [ObservableProperty]
    public partial string TapeLoadProgress { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string RzxPlayProgress { get; set; } = "0%";

    [ObservableProperty]
    public partial bool IsAyEnabled { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ToolTipAy))]
    public partial StereoMode StereoMode { get; set; }

    private readonly Timer _timer;
    private readonly Stopwatch _stopwatch = new();

    public string ComputerName => ComputerType switch
    {
        ComputerType.Spectrum16K => "16k",
        ComputerType.Spectrum48K => "48k",
        ComputerType.Spectrum128K => "128k",
        ComputerType.Timex2048 => "TC2048",
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

    public string ToolTipAy => StereoMode switch
    {
        StereoMode.Mono => "AY Mono",
        StereoMode.StereoABC => "AY Stereo ABC",
        StereoMode.StereoACB => "AY Stereo ACB",
        _ => "Unknown"
    };

    public Action AnimateQuickSave { get; set; } = () => {};      // Body defined in code-behind
    public Action AnimateDiskActivity { get; set; } = () => {};   // Body defined in code-behind

    public StatusBarViewModel()
    {
        _timer = new Timer(1000);
        _timer.Elapsed += UpdateRecordingTime;
    }

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