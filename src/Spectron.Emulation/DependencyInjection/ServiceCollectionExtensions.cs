using Microsoft.Extensions.DependencyInjection;
using OldBit.Spectron.Emulation.Snapshot;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Spectron.Emulation.Tape.Loader;

namespace OldBit.Spectron.Emulation.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static void AddEmulation(this IServiceCollection services)
    {
        services.AddSingleton<TimeMachine>();
        services.AddSingleton<EmulatorFactory>();
        services.AddSingleton<SnaSnapshot>();
        services.AddSingleton<SzxSnapshot>();
        services.AddSingleton<Z80Snapshot>();
        services.AddSingleton<SnapshotLoader>();
        services.AddSingleton<TapeLoader>();
        services.AddSingleton<TapeManager>();
    }
}