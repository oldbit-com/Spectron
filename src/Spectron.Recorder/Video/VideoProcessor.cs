using System.IO.Compression;
using FFMpegCore;
using FFMpegCore.Arguments;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;
using OldBit.Spectron.Emulation.Screen;
using OldBit.Spectron.Recorder.Helpers;

namespace OldBit.Spectron.Recorder.Video;

/// <summary>
/// Converts raw recorded frames into a video file. The raw recording is a Brotli compressed
/// sequence of RGBA frames, which is decompressed and piped directly into FFmpeg as raw video.
/// </summary>
internal sealed class VideoProcessor(
    FrameBuffer frameBuffer,
    RecorderOptions options,
    string outputFilePath,
    string rawRecordingFilePath,
    string audioFilePath)
{
    private const int FrameRate = 50;

    internal void Process()
    {
        var height = frameBuffer.Height -
                     (ScreenSize.BorderTop - options.BorderTop) -
                     (ScreenSize.BorderBottom - options.BorderBottom);

        var width = frameBuffer.Width -
                    (ScreenSize.BorderLeft - options.BorderLeft) -
                    (ScreenSize.BorderRight - options.BorderRight);

        var x = (ScreenSize.BorderLeft + ScreenSize.BorderRight - options.BorderLeft - options.BorderRight) / 2;
        var y = ScreenSize.BorderTop - options.BorderTop;

        using var rawRecordingFile = File.OpenRead(rawRecordingFilePath);
        using var rawFramesStream = new BrotliStream(rawRecordingFile, CompressionMode.Decompress);

        FFMpegArguments
            .FromPipeInput(new StreamPipeSource(rawFramesStream), args => args
                .WithArgument(new CustomArgument("-f rawvideo"))
                .WithArgument(new CustomArgument("-pix_fmt rgba"))
                .WithArgument(new CustomArgument($"-video_size {frameBuffer.Width}x{frameBuffer.Height}"))
                .WithArgument(new CustomArgument($"-framerate {FrameRate}")))
            .AddFileInput(audioFilePath, verifyExists: false, args => args
                .WithArgument(new CustomArgument("-c:a pcm_s16le"))
                .WithArgument(new CustomArgument($"-ac {options.AudioChannels}")))
            .OutputToFile(outputFilePath, true, args => args
                .WithArgument(new CustomArgument($"-vf crop={width}:{height}:{x}:{y},scale=iw*{options.ScalingFactor}:-1"))
                .WithArgument(new CustomArgument($"-sws_flags {options.ScalingAlgorithm}"))
                .WithVideoCodec(VideoCodec.LibX264)
                .WithAudioCodec(AudioCodec.Aac)
                .ForcePixelFormat("yuv420p")
                .WithFramerate(FrameRate))
            .ProcessSynchronously(ffMpegOptions: FFmpegOptions.Create(options.FFmpegPath));
    }
}
