using System.IO.Compression;
using System.Runtime.InteropServices;
using OldBit.Spectron.Emulation.Screen;

namespace OldBit.Spectron.Recorder.Video;

internal class VideoRecorder(string filePath) : IDisposable
{
    private readonly int _frameSizeInBytes = Marshal.SizeOf<Color>() * FrameBuffer.Width * FrameBuffer.Height;

    private Stream? _stream;
    private VideoProcessor? _videoGenerator;

    internal void AppendFrame(FrameBuffer frameBuffer)
    {
        if (_stream == null)
        {
            return;
        }

        unsafe
        {
            fixed (Color* bufferPtr = &frameBuffer.Pixels[0])
            {
                var span = new Span<byte>(bufferPtr, _frameSizeInBytes);

                _stream?.Write(span);
            }
        }
    }

    internal void Start()
    {
        var file = File.OpenWrite(filePath);
        _stream = new BrotliStream(file, CompressionLevel.Fastest);
    }

    internal void Stop()
    {
        _stream?.Flush();
        _stream?.Close();

        _stream = null;
    }

    private void StopGenerator()
    {
        _videoGenerator?.Dispose();
        _videoGenerator = null;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        Stop();
        StopGenerator();
    }
}