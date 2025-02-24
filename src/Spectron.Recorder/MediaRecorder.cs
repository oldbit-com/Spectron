using FFMpegCore;
using Microsoft.Extensions.Logging;
using OldBit.Spectron.Emulation.Devices.Audio;
using OldBit.Spectron.Emulation.Screen;
using OldBit.Spectron.Recorder.Audio;
using OldBit.Spectron.Recorder.Helpers;
using OldBit.Spectron.Recorder.Video;

namespace OldBit.Spectron.Recorder;

public sealed class MediaRecorder : IDisposable
{
    private readonly RecorderMode _recorderMode;
    private readonly string _filePath;
    private readonly RecorderOptions _options;
    private readonly string _audioRecordedFilePath;
    private readonly string _videoRecordedFilePath;

    private readonly ILogger _logger;
    private readonly AudioRecorder? _audioRecorder;
    private readonly VideoRecorder? _videoRecorder;

    private bool _isRecordingActive;

    public MediaRecorder(
        RecorderMode recorderMode,
        string filePath,
        RecorderOptions options,
        ILogger logger)
    {
        _recorderMode = recorderMode;
        _filePath = filePath;
        _options = options;
        _logger = logger;

        _audioRecordedFilePath = $"{filePath}.audio.rec";
        _videoRecordedFilePath = $"{filePath}.video.rec";

        if (recorderMode is RecorderMode.Audio or RecorderMode.AudioVideo)
        {
            _audioRecorder = new AudioRecorder(_audioRecordedFilePath);
        }

        if (recorderMode is RecorderMode.Video or RecorderMode.AudioVideo)
        {
            _videoRecorder = new VideoRecorder(_videoRecordedFilePath);
        }
    }

    public void AppendFrame(FrameBuffer frameBuffer, AudioBuffer audioBuffer)
    {
        if (!_isRecordingActive)
        {
            return;
        }

        try
        {
            _videoRecorder?.AppendFrame(frameBuffer);
            _audioRecorder?.AppendFrame(audioBuffer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to append frame to recorder");

            _videoRecorder?.Stop();
            _audioRecorder?.Stop();
        }
    }

    public void Start()
    {
        _videoRecorder?.Start();
        _audioRecorder?.Start();

        _isRecordingActive = true;
    }

    public void StartProcess(Action<(bool ISucccess, Exception? Error)> completionCallback)
    {
        _isRecordingActive = false;

        Task.Factory.StartNew(() =>
        {
            Thread.Sleep(100);

            try
            {
                ProcessAudio();

                ProcessVideo();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process audio/video");

                completionCallback((false, ex));
                return;
            }

            completionCallback((true, null));
        });
    }

    public static bool VerifyDependencies(string? ffmpegPath = null)
    {
        try
        {
            var options = new FFOptions();

            if (!string.IsNullOrWhiteSpace(ffmpegPath))
            {
                options.BinaryFolder = ffmpegPath;
            }

            FFMpegCore.Helpers.FFMpegHelper.VerifyFFMpegExists(options);

            return true;
        }
        catch
        {
            return false;
        }
    }

    private void ProcessAudio()
    {
        if (_audioRecorder == null)
        {
            return;
        }

        _audioRecorder.Stop();

        var tempAudioFilePath = $"{_filePath}.temp.wav";

        var audioProcessor = new AudioProcessor(_options, tempAudioFilePath, _audioRecordedFilePath);
        audioProcessor.Process();

        FileHelper.TryDeleteFile(_audioRecordedFilePath);
        if (_recorderMode == RecorderMode.Audio)
        {
            FileHelper.TryMoveFile(tempAudioFilePath, _filePath);
        }
    }

    private void ProcessVideo()
    {
        if (_videoRecorder == null)
        {
            return;
        }

        _videoRecorder.Stop();

        var tempVideoFilePath = $"{_filePath}.temp.mp4";
        var tempAudioFilePath = $"{_filePath}.temp.wav";

        var videoProcessor = new VideoProcessor(_options, tempVideoFilePath, _videoRecordedFilePath, tempAudioFilePath);
        videoProcessor.Process();

        FileHelper.TryDeleteFile(_audioRecordedFilePath);
        FileHelper.TryDeleteFile(_videoRecordedFilePath);
        FileHelper.TryDeleteFile(tempAudioFilePath);
        FileHelper.TryMoveFile(tempVideoFilePath, _filePath);
    }

    public void Dispose()
    {
        _audioRecorder?.Dispose();
        _videoRecorder?.Dispose();
    }
}