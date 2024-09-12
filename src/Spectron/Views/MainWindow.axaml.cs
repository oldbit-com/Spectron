using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using OldBit.Spectron.Dialogs;
using OldBit.Spectron.Settings;
using OldBit.Spectron.ViewModels;
using ReactiveUI;

namespace OldBit.Spectron.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    private MainWindowViewModel? _viewModel;

    public MainWindow()
    {
        InitializeComponent();

        this.WhenActivated(action =>
            action(ViewModel!.ShowPreferencesView.RegisterHandler(ShowPreferencesViewAsync)));
    }

    private async Task ShowPreferencesViewAsync(InteractionContext<PreferencesViewModel, Preferences?> interaction)
    {
        var dialog = new PreferencesView { DataContext = interaction.Input };
        var result = await dialog.ShowDialog<Preferences?>(this);

        interaction.SetOutput(result);
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        _viewModel = viewModel;
        _viewModel.ScreenControl = ScreenImage;

        FileDialogs.MainWindow = this;
        MessageDialogs.MainWindow = this;
        viewModel.MainWindow = this;
    }

    private void RecentFilesMenu_OnOpening(object? sender, EventArgs e)
    {
        if (sender is not NativeMenu recentFilesMenu)
        {
            return;
        }

        _viewModel?.RecentFilesViewModel.Opening(recentFilesMenu.Items);
    }
}