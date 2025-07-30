using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using OldBit.Spectron.CommandLine;
using OldBit.Spectron.Extensions;
using OldBit.Spectron.ViewModels;

namespace OldBit.Spectron;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        Console.WriteLine("Starting Spectron...");

        CommandLineParser.Parse(commandLineArgs =>
        {
            try
            {
                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args, builder =>
                {
                    builder.Startup += (sender, e) =>
                    {
                        if (sender is ClassicDesktopStyleApplicationLifetime { MainWindow.DataContext: MainWindowViewModel viewModel })
                        {
                            viewModel.CommandLineArgs = commandLineArgs;
                        }
                    };
                });

                Console.WriteLine("Terminated Spectron...");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }, args);

    }

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToConsole();
}