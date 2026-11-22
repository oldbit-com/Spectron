using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.DependencyInjection;
using OldBit.Spectron.Emulation.Screen;
using OldBit.Spectron.Emulation.Tape.Loader;

namespace OldBit.Spectron.Integration.Tests;

public class IntegrationTestBase
{
    protected EmulatorFactory EmulatorFactory { get; }
    protected Loader Loader { get; }

    protected IntegrationTestBase()
    {
        var services = new ServiceCollection();

        services.AddEmulation();
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

        using var provider = services.BuildServiceProvider();

        EmulatorFactory = provider.GetRequiredService<EmulatorFactory>();
        Loader = provider.GetRequiredService<Loader>();
    }

    protected static void SaveOutput(FrameBuffer frameBuffer, string fileName)
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "TestResults");
        Directory.CreateDirectory(path);

        // Skip the first and last rows of the frame buffer.
        var png = PngEncoder.Encode(frameBuffer, 0, 1, frameBuffer.Width, frameBuffer.Height - 2);
        File.WriteAllBytes(Path.Join(path, fileName), png);
    }
}
