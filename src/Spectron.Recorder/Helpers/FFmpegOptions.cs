using FFMpegCore;

namespace OldBit.Spectron.Recorder.Helpers;

internal static class FFmpegOptions
{
    internal static FFOptions Create(string? ffmpegPath)
    {
        var options = new FFOptions();

        if (!string.IsNullOrWhiteSpace(ffmpegPath))
        {
            options.BinaryFolder = ffmpegPath;
        }

        return options;
    }
}
