using System.IO.Compression;
using System.Runtime.InteropServices;
using FFMpegCore;
using Microsoft.Extensions.Logging;
using OldBit.Spectron.Emulation.Screen;

namespace OldBit.Spectron.Recorder;

public class VideoRecorder : IDisposable
{
    private readonly string _filePath;
    private readonly ILogger _logger;
    private readonly int _frameSizeInBytes;
    private readonly string _rawRecordingFilePath;

    private Stream? _stream;
    private VideoGenerator? _videoGenerator;

    public VideoRecorder(string filePath, ILogger logger)
    {
        _filePath = filePath;
        _logger = logger;
        _frameSizeInBytes = Marshal.SizeOf<Color>() * FrameBuffer.Width * FrameBuffer.Height;

        _rawRecordingFilePath = $"{filePath}.raw";

        var file = File.OpenWrite(_rawRecordingFilePath);
        _stream = new BrotliStream(file, CompressionLevel.Fastest);
    }

    public void AppendFrame(FrameBuffer frameBuffer)
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

    public static bool VerifyRequiredDependencies()
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

    private void StopRecorder()
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

    public void StartProcessing(Action completionCallback)
    {
        StopRecorder();

        _videoGenerator = new VideoGenerator(_filePath, _rawRecordingFilePath, _logger);
        _videoGenerator.Generate(() =>
        {
            _videoGenerator.Dispose();

            completionCallback();
        });
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        StopRecorder();
        StopGenerator();
    }
}