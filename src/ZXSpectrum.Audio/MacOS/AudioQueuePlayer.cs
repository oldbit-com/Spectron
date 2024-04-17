using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ZXSpectrum.Audio.MacOS;

public sealed class AudioQueuePlayer: IDisposable
{
    private const int FloatSizeInBytes = sizeof(float);
    private const int MaxBuffers = 4;

    private IntPtr _audioQueue;
    private List<IntPtr> _allocatedAudioBuffers = [];
    private readonly BlockingCollection<IntPtr> _availableAudioBuffers = new(MaxBuffers);
    private GCHandle _gch;

    public AudioQueuePlayer()
    {
        _gch = GCHandle.Alloc(this);
    }

    // TODO: move to ctor???
    public void Create(int sampleRate, int channelCount, int bufferSize = 12288)
    {
        var audioStreamDescription = new AudioStreamBasicDescription
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

        _audioQueue = AudioQueueNewOutput(audioStreamDescription);
        _allocatedAudioBuffers = AudioQueueAllocateBuffers(bufferSize);

        foreach (var buffer in _allocatedAudioBuffers)
        {
            _availableAudioBuffers.Add(buffer);
        }
    }

    public void Start()
    {
        int status;
        unsafe
        {
            status = AudioToolbox.AudioQueueStart(_audioQueue, null);
        }

        if (status != 0)
        {
            throw new Exception($"Failed to start audio queue: {status}");
        }
    }

    public void Play(byte[] data)
    {
        var buffer = IntPtr.Zero;
        while (!_availableAudioBuffers.IsCompleted)
        {
            try
            {
                buffer = _availableAudioBuffers.Take();
                break;
            }
            catch (InvalidOperationException e) { }
        }

        var bufferFloat = new float[3072];
        for (var i = 0; i < bufferFloat.Length; i++)
        {
            bufferFloat[i] = data[i] / 255f;
        }

        unsafe
        {
            var audioQueueBuffer = (AudioQueueBuffer*)buffer;
            audioQueueBuffer->AudioDataByteSize = audioQueueBuffer->AudioDataBytesCapacity;

            Marshal.Copy(bufferFloat, 0, audioQueueBuffer->AudioData, bufferFloat.Length);

            {
                var status = AudioToolbox.AudioQueueEnqueueBuffer(_audioQueue, audioQueueBuffer, 0, null);

                if (status != 0)
                {
                    throw new AudioPlayerException($"Failed to enqueue buffer: {status}");
                }
            }
        }
    }

    public void Stop()
    {
        AudioToolbox.AudioQueueStop(_audioQueue, true);
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
            player._availableAudioBuffers.Add(buffer);
        }

        Console.WriteLine("AudioQueueOutputCallback");
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
                Debug.WriteLine($"Failed to free audio queue buffer: {status}");
            }
        }

        _allocatedAudioBuffers.Clear();

        status = AudioToolbox.AudioQueueDispose(_audioQueue, true);
        if (status != 0)
        {
            Debug.WriteLine($"Failed to free audio queue: {status}");
        }

        _audioQueue = IntPtr.Zero;

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