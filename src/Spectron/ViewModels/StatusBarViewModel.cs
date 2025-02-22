using System;
using System.Diagnostics;
using System.Timers;
using Avalonia.Threading;
using OldBit.Spectron.Models;
using ReactiveUI;

namespace OldBit.Spectron.ViewModels;

public class StatusBarViewModel : ReactiveObject
{
    private RecordingStatus _recordingStatus = RecordingStatus.None;

    private string _framesPerSecond = "FPS: 0";
    private string _message = string.Empty;
    private string _timeElapsed = "";

    private readonly Timer _timer;
    private readonly Stopwatch _stopwatch = new();

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
                        UpdateProcessingStatus(true, "REC");
                        break;

                    case RecordingStatus.Processing:
                        UpdateProcessingStatus(true, "GEN");
                        break;
                }
            });
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
}