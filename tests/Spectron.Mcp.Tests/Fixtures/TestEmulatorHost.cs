using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.DependencyInjection;
using OldBit.Spectron.Emulation.Rom;

namespace OldBit.Spectron.Mcp.Tests.Fixtures;

/// <summary>
/// Host with an emulator whose timer thread is never started, so frames only run when tools drive them.
/// </summary>
internal sealed class TestEmulatorHost : IEmulatorHost
{
    public Emulator? Emulator { get; }

    public int PauseCount { get; private set; }
    public int ResumeCount { get; private set; }

    internal TestEmulatorHost(ComputerType computerType = ComputerType.Spectrum48K)
    {
        var services = new ServiceCollection();

        services.AddEmulation();
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

        using var provider = services.BuildServiceProvider();

        Emulator = provider.GetRequiredService<EmulatorFactory>().Create(computerType, RomType.Original);
    }

    public Task PauseAsync()
    {
        PauseCount++;
        return Task.CompletedTask;
    }

    public Task ResumeAsync()
    {
        ResumeCount++;
        return Task.CompletedTask;
    }

    public Task ResetAsync(bool hardReset)
    {
        Emulator?.Reset();
        return Task.CompletedTask;
    }

    public Task LoadFileAsync(string filePath) => Task.CompletedTask;
}
