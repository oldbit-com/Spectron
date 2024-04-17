using ZXSpectrum.Audio;
using ZXSpectrum.Audio.MacOS;

var data = Demo.GenerateSinWave(48000, 2);

var task = Task.Run(async () =>
{
    Console.WriteLine("Start playing!");

    using var audioQueue = new AudioQueuePlayer(44100, 1);
    audioQueue.Start();

    for (var i = 1; i <= 10; i++)
    {
        Console.WriteLine($"Playing {i}");
        await audioQueue.Enqueue(data);

        Thread.Sleep(500);
    }

    audioQueue.Stop();
}, CancellationToken.None);

await task;

//await Task.Delay(TimeSpan.FromSeconds(30));

Console.WriteLine("Done playing!");
