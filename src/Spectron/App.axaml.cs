using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OldBit.Spectron.Debugger.Extensions;
using OldBit.Spectron.Emulation.DependencyInjection;
using OldBit.Spectron.Platforms;
using OldBit.Spectron.Services;
using OldBit.Spectron.ViewModels;
using OldBit.Spectron.Views;
using MainWindow = OldBit.Spectron.Views.MainWindow;

namespace OldBit.Spectron;

public class App : Application
{
    private ServiceProvider? _serviceProvider;
    private MainWindowViewModel? MainWindowViewModel { get; set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        services.AddEmulation();
        services.AddServices();
        services.AddDebugging();
        services.AddViewModels();
        services.AddLogging(builder => builder.AddConsole());

        _serviceProvider = services.BuildServiceProvider();

        if (ApplicationLifetime is ClassicDesktopStyleApplicationLifetime desktop)
        {
            MainWindowViewModel = _serviceProvider.GetRequiredService<MainWindowViewModel>();
            desktop.MainWindow = new MainWindow { DataContext = MainWindowViewModel };

            var lifetime = new ApplicationLifetimeHelper(this, desktop.MainWindow);

            lifetime.AppActivated += (_, e) => MainWindowViewModel.WindowActivated(e);
            lifetime.AppDeactivated += (_, e) => MainWindowViewModel.WindowDeactivated(e);
        }

        base.OnFrameworkInitializationCompleted();
    }

    private Window? MainWindow => (ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;

    private void AboutMenuItem_OnClick(object? sender, EventArgs e)
    {
        if (MainWindow == null)
        {
            return;
        }

        var aboutView = new AboutView();
        aboutView.ShowDialog(MainWindow);
    }
}