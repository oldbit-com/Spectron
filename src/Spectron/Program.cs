using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OldBit.Spectron.CommandLine;
using OldBit.Spectron.Extensions;
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
        using var host = CreateHost(args);
        var logger = host.Services.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("Starting Spectron...");

        return CommandLineParser.Parse(commandLineArgs =>
        {
            try
            {
                BuildAvaloniaApp(logger).StartWithClassicDesktopLifetime(args, builder =>
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
    private static AppBuilder BuildAvaloniaApp(ILogger<Program> logger)
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToConsole(logger);

    private static IHost CreateHost(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.SetMinimumLevel(LogLevel.Trace);

        return builder.Build();
    }
}