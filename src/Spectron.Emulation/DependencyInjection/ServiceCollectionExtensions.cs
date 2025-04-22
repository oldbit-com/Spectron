using Microsoft.Extensions.DependencyInjection;
using OldBit.Spectron.Emulation.Commands;
using OldBit.Spectron.Emulation.Devices.Joystick.Gamepad;
using OldBit.Spectron.Emulation.Devices.Keyboard;
using OldBit.Spectron.Emulation.Devices.Mouse;
using OldBit.Spectron.Emulation.Snapshot;
using OldBit.Spectron.Emulation.State;
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
        services.AddSingleton<SnapshotManager>();
        services.AddSingleton<StateManager>();
        services.AddSingleton<Loader>();
        services.AddSingleton<TapeManager>();
        services.AddSingleton<GamepadManager>();
        services.AddSingleton<MouseManager>();
        services.AddSingleton<KeyboardState>();
        services.AddSingleton<CommandManager>();
    }
}