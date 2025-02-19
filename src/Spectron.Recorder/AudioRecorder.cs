using Microsoft.Extensions.Logging;
using OldBit.Spectron.Emulation.Devices.Audio;

namespace OldBit.Spectron.Recorder;

public sealed class AudioRecorder(AudioManager audioManager, string filePath, ILogger logger) : IDisposable
{
    private WaveFileWriter? _writer;

    public void Start()
    {
        audioManager.BeforeEnqueue += AudioManagerOnBeforeEnqueue;

        _writer = new WaveFileWriter(
            filePath,
            AudioManager.PlayerSampleRate,
            audioManager.StereoMode == StereoMode.None ? 1 : 2);
    }

    private void AudioManagerOnBeforeEnqueue(IEnumerable<byte> audioData)
    {
        try
        {
            _writer?.Write(audioData);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to write audio data to file");

            Stop();
        }
    }

    public void Stop()
    {
        audioManager.BeforeEnqueue -= AudioManagerOnBeforeEnqueue;

        _writer?.Close();
        _writer = null;
    }

    public void Dispose() => Stop();
}