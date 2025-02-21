using FFMpegCore;
using Microsoft.Extensions.Logging;
using OldBit.Spectron.Emulation.Devices.Audio;
using OldBit.Spectron.Emulation.Screen;

namespace OldBit.Spectron.Recorder;

public sealed class AudioVideoRecorder : IDisposable
{
    private readonly string _filePath;
    private readonly string _audioFilePath;
    private readonly string _videoFilePath;

    private readonly StereoMode _stereoMode;
    private readonly ILogger _logger;
    private readonly AudioRecorder? _audioRecorder;
    private readonly VideoRecorder? _videoRecorder;

    private bool _isRecordingActive;

    public AudioVideoRecorder(
        RecorderMode recorderMode,
        string filePath,
        StereoMode stereoMode,
        ILogger logger)
    {
        _filePath = filePath;
        _stereoMode = stereoMode;
        _logger = logger;

        _audioFilePath = $"{filePath}.audio.rec";
        _videoFilePath = $"{filePath}.video.rec";

        if (recorderMode is RecorderMode.Audio or RecorderMode.AudioVideo)
        {
            _audioRecorder = new AudioRecorder(_audioFilePath);
        }

        if (recorderMode is RecorderMode.Video or RecorderMode.AudioVideo)
        {
            _videoRecorder = new VideoRecorder(_videoFilePath);
        }
    }

    public void AppendFrame(FrameBuffer frameBuffer, IEnumerable<byte> audioData)
    {
        if (!_isRecordingActive)
        {
            return;
        }

        try
        {
            _videoRecorder?.AppendFrame(frameBuffer);
            _audioRecorder?.AppendFrame(audioData);
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

    public void StartProcess(Action<bool> completionCallback)
    {
        _isRecordingActive = false;

        Task.Factory.StartNew(() =>
        {
            Thread.Sleep(100);

            var isSuccess = true;

            try
            {
                ProcessAudio();

                ProcessVideo();
            }
            catch (Exception ex)
            {
                isSuccess = false;
                _logger.LogError(ex, "Failed to process audio/video");
            }
            finally
            {
                completionCallback(isSuccess);
            }
        });
    }

    public static bool VerifyVideoRecordingRequirements()
    {
        try
        {
            var options = new FFOptions();
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

        var audioProcessor = new AudioProcessor(_stereoMode,  $"{_filePath}.temp.wav", _audioFilePath);
        audioProcessor.Process();
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

        var videoProcessor = new VideoProcessor(tempVideoFilePath, _videoFilePath, tempAudioFilePath);
        videoProcessor.Process();

        FileHelper.TryMoveFile(tempVideoFilePath, _filePath);
    }

    public void Dispose()
    {
        _audioRecorder?.Dispose();
        _videoRecorder?.Dispose();
    }
}