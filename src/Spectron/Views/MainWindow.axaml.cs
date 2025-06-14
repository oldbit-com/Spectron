using System;
using System.Collections.Generic;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using OldBit.Spectron.Debugger.ViewModels;
using OldBit.Spectron.Debugger.Views;
using OldBit.Spectron.Dialogs;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Files;
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

            action(ViewModel!.ShowPrintOutputView
                .RegisterHandler(Show<PrintOutputViewModel, Unit?, PrintOutputView>));

            action(ViewModel!.ShowScreenshotView
                .RegisterHandler(Show<ScreenshotViewModel, Unit?, ScreenshotView>));

            action(ViewModel!.ShowSelectFileView
                .RegisterHandler(ShowDialogAsync<SelectArchiveFileViewModel, ArchiveEntry?, SelectArchiveFileView>));

            action(ViewModel!.TapeMenuViewModel.ShowTapeView
                .RegisterHandler(ShowDialogAsync<TapeViewModel, Unit?, TapeView>));

            action(ViewModel!.ShowTimeMachineView
                .RegisterHandler(ShowDialogAsync<TimeMachineViewModel, TimeMachineEntry?, TimeMachineView>));

            action(ViewModel!.ShowTrainersView
                .RegisterHandler(ShowDialogAsync<TrainerViewModel, Unit?, TrainerView>));
        });
    }

    private async Task ShowDialogAsync<TInput, TOutput, TView>(IInteractionContext<TInput, TOutput?> context) where TView : Window, new()
    {
        var view = new TView { DataContext = context.Input };
        var result = await view.ShowDialog<TOutput?>(this);

        context.SetOutput(result);

        if (context.Input is IDisposable disposable)
        {
            disposable.Dispose();
        }
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

        view.Closed += (_, _) =>
        {
            if (!_windows.TryGetValue(viewType, out var closedWindow))
            {
                return;
            }

            if (closedWindow.DataContext is IDisposable disposable)
            {
                disposable.Dispose();
            }

            _windows.Remove(viewType);

            _viewModel?.OnViewClosed(context.Input);
        };

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
        viewModel.NotificationManager = NotificationManager;
    }

    private void InputElement_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        var position = e.GetCurrentPoint(ScreenImage).Position;
        var bounds = ScreenImage.Bounds;

        _viewModel?.HandleMouseMoved(position, bounds);
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(ScreenImage);
        var bounds = ScreenImage.Bounds;

        _viewModel?.HandleMouseButtonStateChanged(point, bounds);
    }

    private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        var point = e.GetCurrentPoint(ScreenImage);
        var bounds = ScreenImage.Bounds;

        _viewModel?.HandleMouseButtonStateChanged(point, bounds);
    }
}