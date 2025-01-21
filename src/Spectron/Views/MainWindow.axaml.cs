using System;
using System.Collections.Generic;
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
    private readonly Dictionary<string, Window> _windows = new();

    public MainWindow()
    {
        InitializeComponent();

        this.WhenActivated(action =>
        {
            action(ViewModel!.ShowAboutView
                .RegisterHandler(ShowDialogAsync<Unit, Unit?, AboutView>));

            action(ViewModel!.ShowDebuggerView
                .RegisterHandler(Show<DebuggerViewModel, Unit?, DebuggerView>));

            action(ViewModel!.ShowKeyboardHelpView
                .RegisterHandler(Show<Unit, Unit?, HelpKeyboardView>));

            action(ViewModel!.ShowPreferencesView
                .RegisterHandler(ShowDialogAsync<PreferencesViewModel, Preferences, PreferencesView>));

            action(ViewModel!.ShowSelectFileView
                .RegisterHandler(ShowDialogAsync<SelectFileViewModel, ArchiveEntry?, SelectFileView>));

            action(ViewModel!.TapeMenuViewModel.ShowTapeView
                .RegisterHandler(ShowDialogAsync<TapeViewModel, Unit?, TapeView>));

            action(ViewModel!.ShowTimeMachineView
                .RegisterHandler(ShowDialogAsync<TimeMachineViewModel, TimeMachineEntry?, TimeMachineView>));
        });
    }

    private async Task ShowDialogAsync<TInput, TOutput, TView>(IInteractionContext<TInput, TOutput?> context) where TView : Window, new()
    {
        var view = new TView { DataContext = context.Input };
        var result = await view.ShowDialog<TOutput?>(this);

        context.SetOutput(result);
    }

    private void Show<TInput, TOutput, TView>(IInteractionContext<TInput, TOutput?> context) where TView : Window, new()
    {
        var viewType = typeof(TView).Name;

        if (_windows.TryGetValue(viewType, out var window))
        {
            if (viewType == nameof(HelpKeyboardView))
            {
                window.Close();
            }
            else
            {
                window.Show(this);
            }

            return;
        }

        var view = new TView { DataContext = context.Input };
        view.Closed += (_, _) => _windows.Remove(viewType);

        _windows.Add(viewType, view);

        view.Show(this);
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