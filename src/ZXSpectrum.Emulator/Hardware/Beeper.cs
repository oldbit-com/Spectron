using OldBit.Beep;

namespace OldBit.ZXSpectrum.Emulator.Hardware;

public class Beeper
{
    private const int CyclesPerSample = 8;

    private int _lastEarValue;
    private long _lastCycles;
    private byte _lastAmplitude;
    private readonly AudioPlayer _audioPlayer;

    public Beeper(float clockMHz)
    {
        var sampleRate = clockMHz * 1000000 / CyclesPerSample;
        _audioPlayer = new AudioPlayer(AudioFormat.Unsigned8Bit, (int)sampleRate, 1);
        _audioPlayer.Start();
    }

    public void UpdateBeeper(byte value, long cycles)
    {
        var earValue = value & 0x10;
        if (earValue == _lastEarValue)
        {
            return;
        }

        _lastEarValue = earValue;
        var duration = cycles - _lastCycles;
        _lastCycles = cycles;

        if (duration == 0)
        {
            return;
        }

        _lastAmplitude = _lastAmplitude == 0 ? (byte)0xFF : (byte)0;
        var length = (int)(duration / CyclesPerSample);

        Task.Run(async() =>
        {
            await _audioPlayer.PlayAsync(Enumerable.Repeat(_lastAmplitude, length));
        });

    }

    public void Start()
    {

    }


    public void Play()
    {
       // var data = Demo.GenerateSinWave(48000, 2);
       // _audioQueuePlayer.Play(data);
      //  Console.WriteLine("Done playing!");

        //BufferedWaveProvider bufferedWaveProvider = new(new WaveFormat(44100, 16, 1));


        //player.Play("/Users/voytas/Downloads/StarWars3.wav");
    }
}