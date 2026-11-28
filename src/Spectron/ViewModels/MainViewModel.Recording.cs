using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using OldBit.Spectron.Emulation.Devices.Audio;
using OldBit.Spectron.Emulation.Rzx;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Spectron.Models;
using OldBit.Spectron.Recorder;
using OldBit.Spectron.Screen;

namespace OldBit.Spectron.ViewModels;

partial class MainViewModel
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

            var file = await _fileDialogs.SaveAudioFileAsync();

            if (file != null && Emulator != null)
            {
                _mediaRecorder = new MediaRecorder(
                    Emulator.ScreenBuffer.FrameBuffer,
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
            await _messageDialogs.Error(ex.Message);
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

        if (!MediaRecorder.VerifyDependencies(_preferences.Recording.FFmpegPath))
        {
            await _messageDialogs.Error("Video recording is not available. It requires FFmpeg to be available.\nPlease check the documentation for more information.");

            return;
        }

        try
        {
            Pause();

            var file = await _fileDialogs.SaveVideoFileAsync();

            if (file != null && Emulator != null)
            {
                _mediaRecorder = new MediaRecorder(
                    Emulator.ScreenBuffer.FrameBuffer,
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
            await _messageDialogs.Error(ex.Message);
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

    private async Task HandleStartRzxRecording()
    {
        var shouldResume = !IsPaused;

        try
        {
            Pause();

            var file = await _fileDialogs.SaveRzxFileAsync();

            if (file != null && Emulator != null)
            {
                RzxRecordingStatus = RecordingStatus.Recording;
                TapeLoadSpeed = TapeSpeed.Normal;
                ClockMultiplier = 1;

                var recorder = new RzxRecorder(Emulator, _snapshotManager, file.Path.LocalPath);
                recorder.Start();
            }
        }
        catch (Exception ex)
        {
            await _messageDialogs.Error(ex.Message);
        }
        finally
        {
            if (shouldResume)
            {
                Resume();
            }
        }
    }

    private void HandleStopRzxRecording()
    {
        RzxRecordingStatus = RecordingStatus.None;

        if (Emulator?.RzxRecorder == null)
        {
            return;
        }

        var fileName = Emulator.RzxRecorder.FileName;
        var rzxFile = Emulator.RzxRecorder.Stop();

        using var stream = File.Create(fileName);
        rzxFile.Save(stream);
    }

    partial void OnRecordingStatusChanged(RecordingStatus value) =>
        StatusBarViewModel.RecordingStatus = value;

    partial void OnRzxRecordingStatusChanged(RecordingStatus value) =>
        StatusBarViewModel.RzxRecordingStatus = value;
}