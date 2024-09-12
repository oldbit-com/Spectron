using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using OldBit.Spectron.Emulation.DependencyInjection;
using OldBit.Spectron.Services;
using OldBit.Spectron.ViewModels;
using OldBit.Spectron.Views;
using MainWindow = OldBit.Spectron.Views.MainWindow;

namespace OldBit.Spectron;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    public MainWindowViewModel? MainWindowViewModel { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        services.AddEmulation();
        services.AddServices();
        services.AddViewModels();
        services.AddLogging();

        _serviceProvider = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            MainWindowViewModel = _serviceProvider.GetRequiredService<MainWindowViewModel>();

            desktop.MainWindow = new MainWindow
            {
                DataContext = MainWindowViewModel,
            };
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

    private void SettingsMenuItem_OnClick(object? sender, EventArgs e)
    {
        if (MainWindow == null)
        {
            return;
        }

        MainWindowViewModel?.OpenPreferencesWindow();
    }
}