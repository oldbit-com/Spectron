using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.DependencyInjection;
using OldBit.Spectron.Emulation.Rom;

namespace OldBit.Spectron.Debugger.Tests.Fixtures;

internal static class TestEmulator
{
    internal static Emulator Create()
    {
        var services = new ServiceCollection();

        services.AddEmulation();
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

        using var provider = services.BuildServiceProvider();

        return provider.GetRequiredService<EmulatorFactory>().Create(ComputerType.Spectrum48K, RomType.Original);
    }

    internal static void WriteMemory(this Emulator emulator, Word address, params byte[] values)
    {
        for (var i = 0; i < values.Length; i++)
        {
            emulator.Memory.Write((Word)(address + i), values[i]);
        }
    }
}
