using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using OldBit.Spectron.Platforms;
using OldBit.Spectron.ViewModels;
using OldBit.Spectron.Views;
using MainWindow = OldBit.Spectron.Views.MainWindow;

namespace OldBit.Spectron;

public class App : Application
{
    internal IServiceProvider? ServiceProvider { get; set; }
    private MainViewModel? MainViewModel { get; set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

#if DEBUG
        this.AttachDeveloperTools();
#endif
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is ClassicDesktopStyleApplicationLifetime desktop)
        {
            MainViewModel = ServiceProvider!.GetRequiredService<MainViewModel>();
            desktop.MainWindow = new MainWindow { DataContext = MainViewModel };

            var lifetime = new ApplicationLifetimeHelper(this, desktop.MainWindow);

            lifetime.AppActivated += (_, e) => MainViewModel.WindowActivated(e);
            lifetime.AppDeactivated += (_, e) => MainViewModel.WindowDeactivated(e);
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

    private void SettingsMenuItem_OnClick(object? sender, EventArgs e) => MainViewModel?.OpenPreferencesWindow();
}