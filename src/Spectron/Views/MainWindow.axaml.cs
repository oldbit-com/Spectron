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
            action(ViewModel!.ShowAboutView
                .RegisterHandler(ShowViewAsync<Unit, Unit?, AboutView>));

            action(ViewModel!.ShowPreferencesView
                .RegisterHandler(ShowViewAsync<PreferencesViewModel, Preferences, PreferencesView>));

            action(ViewModel!.ShowSelectFileView
                .RegisterHandler(ShowViewAsync<SelectFileViewModel, ArchiveEntry?, SelectFileView>));

            action(ViewModel!.TapeMenuViewModel.ShowTapeView
                .RegisterHandler(ShowViewAsync<TapeViewModel, Unit?, TapeView>));

            action(ViewModel!.ShowTimeMachineView
                .RegisterHandler(ShowViewAsync<TimeMachineViewModel, TimeMachineEntry?, TimeMachineView>));

            action(ViewModel!.ShowDebuggerView
                .RegisterHandler(ShowViewAsync<DebuggerViewModel, Unit?, DebuggerView>));
        });
    }

    private async Task ShowViewAsync<TInput, TOutput, TView>(IInteractionContext<TInput, TOutput?> interaction) where TView : Window, new()
    {
        var dialog = new TView { DataContext = interaction.Input };
        var result = await dialog.ShowDialog<TOutput?>(this);

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