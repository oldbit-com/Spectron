using System.IO.Compression;
using OldBit.Spectron.Emulation.Devices.Audio;

namespace OldBit.Spectron.Recorder.Audio;

internal sealed class AudioRecorder(string filePath) : IDisposable
{
    private Stream? _stream;

    internal void AppendFrame(AudioBuffer audioBuffer)
    {
        for (var i = 0; i < audioBuffer.Count; i++)
        {
            _stream?.WriteByte(audioBuffer.Buffer[i]);
        }
    }

    internal void Start()
    {
        var file = File.OpenWrite(filePath);

        _stream = new GZipStream(file, CompressionLevel.Fastest);
    }

    internal void Stop()
    {
        _stream?.Flush();
        _stream?.Close();

        _stream = null;
    }

    public void Dispose() => Stop();
}