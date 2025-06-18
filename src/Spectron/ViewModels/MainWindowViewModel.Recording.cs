using System;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using OldBit.Spectron.Dialogs;
using OldBit.Spectron.Emulation.Devices.Audio;
using OldBit.Spectron.Models;
using OldBit.Spectron.Recorder;
using OldBit.Spectron.Screen;

namespace OldBit.Spectron.ViewModels;

partial class MainWindowViewModel
{
    private RecorderOptions GetRecorderOptions() => new()
    {
        AudioChannels = Emulator?.AudioManager.StereoMode == StereoMode.Mono ? 1 : 2,
        BorderLeft = BorderSizes.GetBorder(_preferences.Recording.BorderSize).Left,
        BorderRight = BorderSizes.GetBorder(_preferences.Recording.BorderSize).Right,
        BorderTop = BorderSizes.GetBorder(_preferences.Recording.BorderSize).Top,
        BorderBottom = BorderSizes.GetBorder(_preferences.Recording.BorderSize).Bottom,
        ScalingFactor = _preferences.Recording.ScalingFactor,
        ScalingAlgorithm = _preferences.Recording.ScalingAlgorithm,
        FFmpegPath = _preferences.Recording.FFmpegPath,
    };

    private async Task HandleStartAudioRecordingAsync()
    {
        var shouldResume = !IsPaused;

        try
        {
            Pause();

            var file = await FileDialogs.SaveAudioFileAsync();

            if (file != null && Emulator != null)
            {
                _mediaRecorder = new MediaRecorder(
                    RecorderMode.Audio,
                    file.Path.LocalPath,
                    GetRecorderOptions(),
                    _logger);

                _mediaRecorder.Start();

                RecordingStatus = RecordingStatus.Recording;
            }
        }
        catch (Exception ex)
        {
            await MessageDialogs.Error(ex.Message);
        }
        finally
        {
            if (shouldResume)
            {
                Resume();
            }
        }
    }

    private async Task HandleStartVideoRecordingAsync()
    {
        var shouldResume = !IsPaused;

        if (!MediaRecorder.VerifyDependencies())
        {
            await MessageDialogs.Error("Video recording is not available. It requires FFmpeg to be available.\nPlease check the documentation for more information.");

            return;
        }

        try
        {
            Pause();

            var file = await FileDialogs.SaveVideoFileAsync();

            if (file != null && Emulator != null)
            {
                _mediaRecorder = new MediaRecorder(
                    RecorderMode.AudioVideo,
                    file.Path.LocalPath,
                    GetRecorderOptions(),
                    _logger);

                _mediaRecorder.Start();

                RecordingStatus = RecordingStatus.Recording;
            }
        }
        catch (Exception ex)
        {
            await MessageDialogs.Error(ex.Message);
        }
        finally
        {
            if (shouldResume)
            {
                Resume();
            }
        }
    }

    private void HandleStopRecording()
    {
        if (_mediaRecorder == null)
        {
            RecordingStatus = RecordingStatus.None;
            return;
        }

        if (RecordingStatus != RecordingStatus.Recording)
        {
            return;
        }

        RecordingStatus = RecordingStatus.Processing;

        _mediaRecorder.StartProcess(result =>
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                RecordingStatus = RecordingStatus.None;

                NotificationManager.Show(new Notification(
                    result.ISucccess ? "Done!" : "Error!",
                    result.ISucccess ? "Recording has been successfully completed" : $"Recording has failed: {result.Error?.Message}",
                    result.ISucccess ? NotificationType.Information : NotificationType.Error)
                {
                    Expiration = TimeSpan.FromSeconds(10)
                });
            });

            _mediaRecorder.Dispose();
            _mediaRecorder = null;

            if (!result.ISucccess)
            {
                _logger.LogError(result.Error, "Failed to process recording");
            }
        });
    }

    partial void OnRecordingStatusChanged(RecordingStatus value) =>
        StatusBarViewModel.RecordingStatus = RecordingStatus;
}