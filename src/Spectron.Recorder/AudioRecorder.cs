using System.IO.Compression;

namespace OldBit.Spectron.Recorder;

internal sealed class AudioRecorder(string filePath) : IDisposable
{
    private Stream? _stream;

    internal void AppendFrame(IEnumerable<byte> audioData)
    {
        foreach (var data in audioData)
        {
            _stream?.WriteByte(data);
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