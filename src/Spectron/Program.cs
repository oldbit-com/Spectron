using System;
using Avalonia;
using Avalonia.ReactiveUI;
using OldBit.Spectron.Extensions;
using ReactiveUI;

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

        RxApp.DefaultExceptionHandler = new ObservableExceptionHandler();

        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToConsole()
            .UseReactiveUI();
}