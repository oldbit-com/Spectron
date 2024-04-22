using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Channels;

namespace ZXSpectrum.Audio.MacOS;

public sealed class AudioQueuePlayer: IDisposable
{
    private const int FloatSizeInBytes = sizeof(float);
    private const int MaxBuffers = 4;
    private const int DefaultBufferSize = 12288;

    private readonly IntPtr _audioQueue;
    private readonly List<IntPtr> _allocatedAudioBuffers;
    private readonly Channel<IntPtr> _availableAudioBuffers;
    private GCHandle _gch;

    public AudioQueuePlayer(int sampleRate, int channelCount, int bufferSize = DefaultBufferSize)
    {
        _gch = GCHandle.Alloc(this);

        _availableAudioBuffers = Channel.CreateBounded<IntPtr>(new BoundedChannelOptions(MaxBuffers)
        {
            SingleReader = true,
            SingleWriter = true,
            FullMode = BoundedChannelFullMode.Wait
        });

        var audioStreamDescription = GetAudioStreamBasicDescription(sampleRate, channelCount);

        _audioQueue = AudioQueueNewOutput(audioStreamDescription);
        _allocatedAudioBuffers = AudioQueueAllocateBuffers(bufferSize);

        foreach (var buffer in _allocatedAudioBuffers)
        {
            _availableAudioBuffers.Writer.TryWrite(buffer);
        }
    }

    private static AudioStreamBasicDescription GetAudioStreamBasicDescription(int sampleRate, int channelCount) => new()
    {
        SampleRate = sampleRate,
        Format = AudioFormatType.LinearPCM,
        FormatFlags = AudioFormatFlags.AudioFormatFlagIsFloat,
        BytesPerPacket = (uint)(channelCount * FloatSizeInBytes),
        FramesPerPacket = 1,
        BytesPerFrame = (uint)(channelCount * FloatSizeInBytes),
        ChannelsPerFrame = (uint)channelCount,
        BitsPerChannel = FloatSizeInBytes * 8
    };

    public void Start()
    {
        unsafe
        {
            var status = AudioToolbox.AudioQueueStart(_audioQueue, null);
            if (status != 0)
            {
                throw new AudioPlayerException($"Failed to start audio queue: {status}");
            }
        }
    }

    public void Stop()
    {
        AudioToolbox.AudioQueueStop(_audioQueue, true);
    }

    public async Task Enqueue(float[] data, CancellationToken cancellationToken = default)
    {
        var buffer = await _availableAudioBuffers.Reader.ReadAsync(cancellationToken);

        unsafe
        {
            var audioQueueBuffer = (AudioQueueBuffer*)buffer;
            audioQueueBuffer->AudioDataByteSize = (uint)(data.Length * FloatSizeInBytes);

            Marshal.Copy(data, 0, audioQueueBuffer->AudioData, data.Length);

            var status = AudioToolbox.AudioQueueEnqueueBuffer(_audioQueue, audioQueueBuffer, 0, null);
            if (status != 0)
            {
                throw new AudioPlayerException($"Failed to enqueue buffer: {status}");
            }
        }
    }

    private IntPtr AudioQueueNewOutput(AudioStreamBasicDescription description)
    {
        unsafe
        {
            var status = AudioToolbox.AudioQueueNewOutput(ref description, &AudioQueueOutputCallback,
                GCHandle.ToIntPtr(_gch), IntPtr.Zero, IntPtr.Zero, 0, out var audioQueue);

            if (status != 0)
            {
                throw new AudioPlayerException($"Failed to create audio queue: {status}");
            }

            return audioQueue;
        }
    }

    private List<IntPtr> AudioQueueAllocateBuffers(int bufferSize)
    {
        var buffers = new List<IntPtr>();

        for (var i = 0; i < MaxBuffers; i++)
        {
            var status = AudioToolbox.AudioQueueAllocateBuffer(_audioQueue, (uint)bufferSize, out var buffer);

            if (status != 0)
            {
                throw new AudioPlayerException($"Failed to allocate buffer: {status}");
            }

            buffers.Add(buffer);
        }

        return buffers;
    }

    [UnmanagedCallersOnly]
    private static void AudioQueueOutputCallback(IntPtr userData, IntPtr audioQueue, IntPtr buffer)
    {
        var gch = GCHandle.FromIntPtr(userData);
        if (gch.Target is AudioQueuePlayer player)
        {
            player._availableAudioBuffers.Writer.TryWrite(buffer);
        }
    }

    private void ReleaseUnmanagedResources()
    {
        if (_audioQueue == IntPtr.Zero)
        {
            return;
        }

        int status;
        foreach (var buffer in _allocatedAudioBuffers)
        {
            status = AudioToolbox.AudioQueueFreeBuffer(_audioQueue, buffer);
            if (status != 0)
            {
                Trace.WriteLine($"Failed to free audio queue buffer: {status}");
            }
        }

        _allocatedAudioBuffers.Clear();

        status = AudioToolbox.AudioQueueDispose(_audioQueue, true);
        if (status != 0)
        {
            Trace.WriteLine($"Failed to free audio queue: {status}");
        }
    }

    public void Dispose()
    {
        if (_gch.IsAllocated)
        {
            _gch.Free();
        }

        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~AudioQueuePlayer()
    {
        ReleaseUnmanagedResources();
    }
}