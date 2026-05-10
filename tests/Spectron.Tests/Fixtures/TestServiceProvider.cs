using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OldBit.Spectron.Debugger.Extensions;
using OldBit.Spectron.Emulation.DependencyInjection;
using OldBit.Spectron.Logging;
using OldBit.Spectron.Services;
using OldBit.Spectron.ViewModels;

namespace OldBit.Spectron.Tests.Fixtures;

internal sealed class TestServiceProvider
{
    private ServiceProvider? _provider;

    internal ServiceCollection Services { get; } = [];

    internal TestServiceProvider(ITestOutputHelper output)
    {
        Services.AddServices();
        Services.AddViewModels();
        Services.AddEmulation();
        Services.AddDebugging();
        Services.AddLogging();
        Services.AddSingleton(output);

        Services.AddSingleton<ILogStore, TestLogStore>();
        Services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddProvider(new XUnitLoggerProvider(output));
            builder.SetMinimumLevel(LogLevel.Trace);
        });
    }

    internal ServiceProvider Build()
    {
        _provider ??= Services.BuildServiceProvider();

        return _provider;
    }
}