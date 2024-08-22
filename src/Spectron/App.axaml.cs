using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
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
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void AboutMenuItem_OnClick(object? sender, EventArgs e)
    {
        var mainWindow = (Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        if (mainWindow == null)
        {
            return;
        }

        var aboutView = new AboutView();
        aboutView.ShowDialog(mainWindow);
    }
}