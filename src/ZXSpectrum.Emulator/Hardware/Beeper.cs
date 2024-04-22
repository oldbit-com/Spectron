using NAudio.Wave;
using ZXSpectrum.Audio;
using ZXSpectrum.Audio.MacOS;

namespace OldBit.ZXSpectrum.Emulator.Hardware;

public class Beeper
{
    private int _lastEarValue;
    private int _lastCycles;
    private readonly AudioQueuePlayer _audioQueuePlayer = new(44100, 1);

    public void UpdateBeeper(byte value, int cycles)
    {
        var earValue= value & 0x10;

        if (earValue == _lastEarValue)
        {
            return;
        }

        _lastEarValue = earValue;
        var duration = cycles - _lastCycles;
        _lastCycles = cycles;

        if (duration > 0)
        {
        }
    }

    public void Start()
    {
        _audioQueuePlayer.Start();
    }


    public void Play()
    {
       // var data = Demo.GenerateSinWave(48000, 2);
       // _audioQueuePlayer.Play(data);
        Console.WriteLine("Done playing!");

        //BufferedWaveProvider bufferedWaveProvider = new(new WaveFormat(44100, 16, 1));


        //player.Play("/Users/voytas/Downloads/StarWars3.wav");
    }
}