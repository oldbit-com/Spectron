using Microsoft.Extensions.Logging;
using OldBit.Spectron.Emulation.Devices.Audio;

namespace OldBit.Spectron.Recorder;

public sealed class AudioRecorder(StereoMode stereoMode, string filePath, ILogger logger) : IDisposable
{
    private WaveFileWriter? _writer;

    public void AppendFrame(IEnumerable<byte> audioData)
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

    public void Start()
    {
        _writer = new WaveFileWriter(
            filePath,
            AudioManager.PlayerSampleRate,
            stereoMode == StereoMode.None ? 1 : 2);
    }

    public void Stop()
    {
        _writer?.Close();
        _writer = null;
    }

    public void Dispose() => Stop();
}