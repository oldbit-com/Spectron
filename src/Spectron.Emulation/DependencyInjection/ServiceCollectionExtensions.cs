using Microsoft.Extensions.DependencyInjection;
using OldBit.Spectron.Emulation.Commands;
using OldBit.Spectron.Emulation.Devices.Beta128;
using OldBit.Spectron.Emulation.Devices.Gamepad;
using OldBit.Spectron.Emulation.Devices.Interface1.Microdrives;
using OldBit.Spectron.Emulation.Devices.Keyboard;
using OldBit.Spectron.Emulation.Snapshot;
using OldBit.Spectron.Emulation.State;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Spectron.Emulation.Tape.Loader;
using OldBit.Spectron.Emulation.TimeTravel;

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
        services.AddSingleton<SnapshotManager>();
        services.AddSingleton<StateManager>();
        services.AddSingleton<Loader>();
        services.AddSingleton<TapeManager>();
        services.AddSingleton<MicrodriveManager>();
        services.AddSingleton<DiskDriveManager>();
        services.AddSingleton<GamepadManager>();
        services.AddSingleton<KeyboardState>();
        services.AddSingleton<CommandManager>();
    }
}