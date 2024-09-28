using OpenTK.Audio.OpenAL;

namespace OldBit.Spectron.Emulation.Devices.Audio;

public class AudioPlayer2 : IDisposable
{
    private readonly ALFormat _audioFormat;
    private readonly int _sampleRate;
    private readonly Thread _thread;

    private readonly ALDevice _device;
    private readonly ALContext _context;
    private readonly int[] _buffers;
    private readonly int _source;

    private bool _isRunning;

    public AudioPlayer2(int sampleRate)
    {
        _audioFormat = ALFormat.Mono8;
        _sampleRate = sampleRate;

        _device = ALC.OpenDevice(null);

        _context = ALC.CreateContext(_device, new ALContextAttributes());
        ALC.MakeContextCurrent(_context);

        _buffers = AL.GenBuffers(4);
        _source = AL.GenSource();

        _thread = new Thread(Worker)
        {
            IsBackground = true,
            Priority = ThreadPriority.AboveNormal
        };
    }

    public void Start()
    {
        _isRunning = true;
        _thread.Start();
    }

    public void Stop()
    {
        _isRunning = false;
        _thread.Join();
    }

    private int _bufferIndex = 0;

    public void EnqueueAudio(byte[] audioData)
    {
        var buffersProcessed = GetProcessedBuffersCount();

        if (buffersProcessed > 0)
        {
            var bufferId = AL.SourceUnqueueBuffer(_source);
            AL.BufferData(bufferId, _audioFormat, audioData, _sampleRate);
            AL.SourceQueueBuffer(_source, bufferId);
        }
        else
        {
            if (_bufferIndex < _buffers.Length)
            {
                AL.BufferData(_buffers[_bufferIndex], _audioFormat, audioData, _sampleRate);
                AL.SourceQueueBuffer(_source, _buffers[_bufferIndex]);
                _bufferIndex++;
            }
            else
            {
                _bufferIndex = 0;
            }
        }
    }

    private void Worker()
    {
        while (_isRunning)
        {
            if (IsPlaying() || !HasBuffersQueued())
            {
                Thread.SpinWait(10);
                continue;
            }

            AL.SourcePlay(_source);
        }

        AL.SourceStop(_source);
    }

    private bool HasBuffersQueued()
    {
        AL.GetSource(_source, ALGetSourcei.BuffersQueued, out var buffersQueued);
        return buffersQueued > 0;
    }

    private bool IsPlaying()
    {
        AL.GetSource(_source, ALGetSourcei.SourceState, out var state);
        return state == (int)ALSourceState.Playing;
    }

    private int GetProcessedBuffersCount()
    {
        AL.GetSource(_source, ALGetSourcei.BuffersProcessed, out var buffersProcessed);
        return buffersProcessed;
    }

    private void ReleaseUnmanagedResources()
    {
        AL.DeleteSource(_source);
        AL.DeleteBuffers(_buffers);
        ALC.DestroyContext(_context);
        ALC.CloseDevice(_device);
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~AudioPlayer2()
    {
        ReleaseUnmanagedResources();
    }
}