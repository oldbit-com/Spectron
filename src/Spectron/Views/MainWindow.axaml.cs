using System;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using OldBit.Spectron.Dialogs;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Storage;
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
        {
            action(ViewModel!.ShowPreferencesView.RegisterHandler(ShowPreferencesViewAsync));
            action(ViewModel!.TapeMenuViewModel.ShowTapeView.RegisterHandler(ShowTapeViewAsync));
            action(ViewModel!.ShowAboutView.RegisterHandler(ShowAboutViewAsync));
            action(ViewModel!.ShowSelectFileView.RegisterHandler(ShowSelectFileViewAsync));
            action(ViewModel!.ShowTimeMachineView.RegisterHandler(ShowTimeMachineViewAsync));
        });
    }

    private async Task ShowPreferencesViewAsync(IInteractionContext<PreferencesViewModel, Preferences?> interaction)
    {
        var dialog = new PreferencesView { DataContext = interaction.Input };
        var result = await dialog.ShowDialog<Preferences?>(this);

        interaction.SetOutput(result);
    }

    private async Task ShowTapeViewAsync(IInteractionContext<TapeViewModel, Unit?> interaction)
    {
        var dialog = new TapeView { DataContext = interaction.Input };
        var result = await dialog.ShowDialog<Unit?>(this);

        interaction.SetOutput(result);
    }

    private async Task ShowAboutViewAsync(IInteractionContext<Unit, Unit?> interaction)
    {
        var dialog = new AboutView();
        await dialog.ShowDialog<Unit?>(this);

        interaction.SetOutput(Unit.Default);
    }

    private async Task ShowSelectFileViewAsync(IInteractionContext<SelectFileViewModel, ArchiveEntry?> interaction)
    {
        var dialog = new SelectFileView { DataContext = interaction.Input };
        var result = await dialog.ShowDialog<ArchiveEntry?>(this);

        interaction.SetOutput(result);
    }

    private async Task ShowTimeMachineViewAsync(IInteractionContext<TimeMachineViewModel, TimeMachineEntry?> interaction)
    {
        var dialog = new TimeMachineView { DataContext = interaction.Input };
        var result = await dialog.ShowDialog<TimeMachineEntry?>(this);

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

    private void RecentFilesSubmenuOpened(object? sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem)
        {
            return;
        }

        _viewModel?.RecentFilesViewModel.Opening(menuItem.Items);
    }
}