using System.IO.Compression;
using System.Runtime.InteropServices;
using FFMpegCore;
using FFMpegCore.Arguments;
using FFMpegCore.Enums;
using OldBit.Spectron.Emulation.Screen;
using SkiaSharp;

namespace OldBit.Spectron.Recorder;

internal sealed class VideoProcessor : IDisposable
{
    private const string FileNamePrefix = "frame_";
    private const string TempDirPrefix = "spectron-";

    private readonly string _outputFilePath;
    private readonly string _rawRecordingFilePath;
    private readonly string _audioFilePath;
    private readonly int _frameSizeInBytes;
    private readonly byte[] _frameBuffer;
    private readonly SKBitmap _bitmap;

    public VideoProcessor(string outputFilePath, string rawRecordingFilePath, string audioFilePath)
    {
        _outputFilePath = outputFilePath;
        _rawRecordingFilePath = rawRecordingFilePath;
        _audioFilePath = audioFilePath;
        _frameSizeInBytes = Marshal.SizeOf<Color>() * FrameBuffer.Width * FrameBuffer.Height;

        _frameBuffer = new byte[_frameSizeInBytes];

        _bitmap = new SKBitmap(
            FrameBuffer.Width,
            FrameBuffer.Height,
            SKColorType.Rgba8888,
            SKAlphaType.Unpremul);
    }

    internal void Process()
    {
        DirectoryInfo? tempWorkingDir = null;

        try
        {
            tempWorkingDir = Directory.CreateTempSubdirectory(TempDirPrefix);

            RawFileToImages(tempWorkingDir.FullName);
            ConvertImagesToVideo(tempWorkingDir.FullName);
        }
        finally
        {
            FileHelper.TryDeleteFolder(tempWorkingDir);
            FileHelper.TryDeleteFile(_audioFilePath);
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

        FileHelper.TryDeleteFile(_rawRecordingFilePath);
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

    private void ConvertImagesToVideo(string tempWorkingDir)
    {
        FFMpegArguments
            .FromFileInput(Path.Combine(tempWorkingDir, $"{FileNamePrefix}%d.png"), false, options => options
                .WithArgument(new CustomArgument("-framerate 50")))
                .AddFileInput(_audioFilePath)
            .OutputToFile(_outputFilePath, true, options => options
                .WithVideoCodec(VideoCodec.LibX264)
                .WithAudioCodec(AudioCodec.Aac)
                .ForcePixelFormat("yuv420p")
                .WithFramerate(50))
            .ProcessSynchronously();
    }

    public void Dispose() => _bitmap.Dispose();
}