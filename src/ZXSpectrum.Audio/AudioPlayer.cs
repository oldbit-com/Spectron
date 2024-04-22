using System.Runtime.InteropServices;
using ZXSpectrum.Audio.MacOS;

namespace ZXSpectrum.Audio;

public class AudioPlayer
{
    private readonly int _sampleRate;
    private readonly int _channelCount;
    private readonly AudioFormat _format;

    private readonly AudioQueuePlayer _audioQueuePlayer;

    public AudioPlayer(int sampleRate, int channelCount, AudioFormat format)
    {
        _sampleRate = sampleRate;
        _channelCount = channelCount;
        _format = format;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            _audioQueuePlayer = new AudioQueuePlayer(sampleRate, channelCount);
        }
    }

    public async Task Play(Stream stream, CancellationToken cancellationToken)
    {
        var buffer = new byte[12288];
        var count = await stream.ReadAsync(buffer, cancellationToken);
        // 8, 26, 32 bit
    }

    // private static OSPlatform GetPlatform()
    // {
    //     OperatingSystem.IsMacOS()
    // }
}