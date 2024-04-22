using TestConsole;
using ZXSpectrum.Audio.MacOS;

var data = new float[3072];

// float, float, float, float
// 4      4      4      4  bytes

var generator = new SinWaveGenerator(44100, 2);
generator.Generate(data, data.Length);

var task = Task.Run(async () =>
{
    Console.WriteLine("Start playing!");

    using var audioQueue = new AudioQueuePlayer(44100, 2);
    audioQueue.Start();

    for (var i = 1; i <= 20; i++)
    {
        Console.WriteLine($"Playing {i}");
        await audioQueue.Enqueue(data);
    }

    audioQueue.Stop();
}, CancellationToken.None);

await task;

//await Task.Delay(TimeSpan.FromSeconds(30));

Console.WriteLine("Done playing!");
