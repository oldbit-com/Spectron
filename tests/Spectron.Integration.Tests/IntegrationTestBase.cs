using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.DependencyInjection;
using OldBit.Spectron.Emulation.Screen;
using OldBit.Spectron.Emulation.Tape.Loader;
using OldBit.Spectron.Integration.Tests.Fixtures;

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
        using var converter = new ScreenConverter(frameBuffer);
        converter.UpdateBitmap();

        var path = Path.Combine(Directory.GetCurrentDirectory(), "TestResults");
        Directory.CreateDirectory(path);

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), $"{path}/{fileName}");

        converter.SaveBitmap(filePath);
    }
}