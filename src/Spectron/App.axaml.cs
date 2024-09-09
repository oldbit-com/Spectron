using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using OldBit.Spectron.Services;
using OldBit.Spectron.ViewModels;
using OldBit.Spectron.Views;
using MainWindow = OldBit.Spectron.Views.MainWindow;

namespace OldBit.Spectron;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        services.AddServices();
        services.AddViewModels();
        services.AddLogging();

        var provider = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindowViewModel = provider.GetRequiredService<MainWindowViewModel>();

            desktop.MainWindow = new MainWindow
            {
                DataContext = mainWindowViewModel,
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

        var settingsView = new SettingsView();
        settingsView.ShowDialog(MainWindow);
    }
}