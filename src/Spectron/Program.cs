using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OldBit.Spectron.CommandLine;
using OldBit.Spectron.Debugger.Extensions;
using OldBit.Spectron.Emulation.DependencyInjection;
using OldBit.Spectron.Extensions;
using OldBit.Spectron.Logging;
using OldBit.Spectron.Services;
using OldBit.Spectron.ViewModels;

namespace OldBit.Spectron;

public class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static int Main(string[] args)
    {
        var host = CreateHost(args);
        var logger = host.Services.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("Starting Spectron...");

        return CommandLineParser.Parse(commandLineArgs =>
        {
            try
            {
                BuildAvaloniaApp(host.Services).StartWithClassicDesktopLifetime(args, builder =>
                {
                    builder.Startup += (sender, e) =>
                    {
                        if (sender is ClassicDesktopStyleApplicationLifetime { MainWindow.DataContext: MainWindowViewModel viewModel })
                        {
                            viewModel.CommandLineArgs = commandLineArgs;
                        }
                    };
                });

                logger.LogInformation("Terminated Spectron...");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception occurred during startup");
            }
        }, args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp(IServiceProvider services) => AppBuilder
        .Configure<App>()
        .UsePlatformDetect()
        .AfterSetup(app => ((App?)app.Instance)?.ServiceProvider = services)
        .LogToConsole(services.GetRequiredService<ILogger<Program>>());

    private static IHost CreateHost(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddEmulation();
        builder.Services.AddServices();
        builder.Services.AddDebugging();
        builder.Services.AddViewModels();
        builder.Logging.ClearProviders();
        builder.Logging.SetMinimumLevel(LogLevel.Trace);
        builder.Logging.AddInMemory();

        if (builder.Environment.IsDevelopment())
        {
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();
        }

        return builder.Build();
    }
}