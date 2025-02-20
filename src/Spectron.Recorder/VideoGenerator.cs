using System.IO.Compression;
using System.Runtime.InteropServices;
using FFMpegCore;
using FFMpegCore.Arguments;
using FFMpegCore.Enums;
using Microsoft.Extensions.Logging;
using OldBit.Spectron.Emulation.Screen;
using SkiaSharp;

namespace OldBit.Spectron.Recorder;

public sealed class VideoGenerator : IDisposable
{
    private const string FileNamePrefix = "frame_";
    private const string TempDirPrefix = "spectron-";

    private readonly string _outputFilePath;
    private readonly string _rawRecordingFilePath;
    private readonly ILogger _logger;
    private readonly int _frameSizeInBytes;
    private readonly byte[] _frameBuffer;
    private readonly SKBitmap _bitmap;

    public VideoGenerator(string outputFilePath, string rawRecordingFilePath, ILogger logger)
    {
        _outputFilePath = outputFilePath;
        _rawRecordingFilePath = rawRecordingFilePath;
        _logger = logger;
        _frameSizeInBytes = Marshal.SizeOf<Color>() * FrameBuffer.Width * FrameBuffer.Height;

        _frameBuffer = new byte[_frameSizeInBytes];

        _bitmap = new SKBitmap(
            FrameBuffer.Width,
            FrameBuffer.Height,
            SKColorType.Rgba8888,
            SKAlphaType.Unpremul);
    }

    public void Generate(Action completionCallback) => Task.Factory.StartNew(
        () => GenerateThread(completionCallback));

    private void GenerateThread(Action completionCallback)
    {
        DirectoryInfo? tempWorkingDir = null;

        try
        {
            tempWorkingDir = Directory.CreateTempSubdirectory(TempDirPrefix);

            RawFileToImages(tempWorkingDir.FullName);
            ConvertImagesToVideo(tempWorkingDir.FullName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating video");
        }
        finally
        {
            TryDeleteTempFolder(tempWorkingDir);
            completionCallback();
        }
    }

    private void RawFileToImages(string tempWorkingDir)
    {
        var index = 1;

        var rawRecodingFile = File.OpenRead(_rawRecordingFilePath);
        using var rawFileStream = new BrotliStream(rawRecodingFile, CompressionMode.Decompress);

        while (true)
        {
            var readSize = rawFileStream.ReadAtLeast(_frameBuffer, _frameSizeInBytes, throwOnEndOfStream: false);

            if (readSize != _frameSizeInBytes)
            {
                break;
            }

            unsafe
            {
                fixed (byte* bufferPtr = &_frameBuffer[0])
                {
                    _bitmap.SetPixels((IntPtr)bufferPtr);
                }
            }

            SaveImage(tempWorkingDir, index);

            index += 1;
        }

        rawFileStream.Close();

        TryDeleteRawRecordingFile();
    }

    private void SaveImage(string tempWorkingDir, int index)
    {
        // TODO: Cropping and scaling
        using var image = SKImage.FromBitmap(_bitmap);
        using var png = image.Encode(SKEncodedImageFormat.Png, 100);

        var fileName = Path.Combine(tempWorkingDir, $"{FileNamePrefix}{index}.png");
        using var imageStream = File.OpenWrite(fileName);

        png.SaveTo(imageStream);
        imageStream.Close();
    }

    private void TryDeleteRawRecordingFile()
    {
        try
        {
            File.Delete(_rawRecordingFilePath);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting raw recording file: {File}", _rawRecordingFilePath);
        }
    }

    private void TryDeleteTempFolder(DirectoryInfo? directory)
    {
        if (directory == null)
        {
            return;
        }

        try
        {
            directory.Delete(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting temp folder: {Folder}", directory.FullName);;
        }
    }

    private void ConvertImagesToVideo(string tempWorkingDir)
    {
        FFMpegArguments
            .FromFileInput(Path.Combine(tempWorkingDir, $"{FileNamePrefix}%d.png"), false, options => options
                .WithArgument(new CustomArgument("-framerate 50")))
            .OutputToFile(_outputFilePath, true, options => options
                .WithVideoCodec(VideoCodec.LibX264)
                .ForcePixelFormat("yuv420p")
                .WithFramerate(50))
            .ProcessSynchronously();
    }

    public void Dispose() => _bitmap.Dispose();
}