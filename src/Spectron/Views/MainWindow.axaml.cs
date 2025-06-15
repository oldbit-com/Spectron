using System;
using System.Collections.Generic;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using CommunityToolkit.Mvvm.Messaging;
using OldBit.Spectron.Debugger.ViewModels;
using OldBit.Spectron.Debugger.Views;
using OldBit.Spectron.Dialogs;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Files;
using OldBit.Spectron.Messages;
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
            action(ViewModel!.ShowPreferencesView
                .RegisterHandler(ShowDialogAsync<PreferencesViewModel, Preferences, PreferencesView>));

            action(ViewModel!.ShowSelectFileView
                .RegisterHandler(ShowDialogAsync<SelectArchiveFileViewModel, ArchiveEntry?, SelectArchiveFileView>));

            action(ViewModel!.ShowTimeMachineView
                .RegisterHandler(ShowDialogAsync<TimeMachineViewModel, TimeMachineEntry?, TimeMachineView>));
        });

        WeakReferenceMessenger.Default.Register<MainWindow, ShowAboutViewMessage>(this, static (w, _) =>
            ShowDialog<AboutView>(w));

        WeakReferenceMessenger.Default.Register<MainWindow, ShowDebuggerViewMessage>(this, static (w, m) =>
            ShowDialog<DebuggerView>(w, m.ViewModel!));

        WeakReferenceMessenger.Default.Register<MainWindow, ShowKeyboardViewMessage>(this, (w, _) =>
            Show<HelpKeyboardView>(w));

        WeakReferenceMessenger.Default.Register<MainWindow, ShowPrintOutputViewMessage>(this, (w, m) =>
            Show<PrintOutputView>(w, new PrintOutputViewModel(m.Printer)));

        WeakReferenceMessenger.Default.Register<MainWindow, ShowScreenshotViewMessage>(this, (w, m) =>
            Show<ScreenshotView>(w, new ScreenshotViewModel()));

        WeakReferenceMessenger.Default.Register<MainWindow, ShowTapeViewMessage>(this, static (w, m) =>
            ShowDialog<TapeView>(w, new TapeViewModel(m.TapeManager)));

        WeakReferenceMessenger.Default.Register<MainWindow, ShowTrainerViewMessage>(this, static (w, m) =>
            ShowDialog<TrainerView>(w, new TrainerViewModel(m.Emulator, m.PokeFile)));
    }

    private static void ShowDialog<TView>(Window owner, object? viewModel = null) where TView : Window, new()
    {
        var view = new TView { DataContext = viewModel };

        view.ShowDialog(owner).ContinueWith(_ =>
        {
            if (viewModel is IDisposable disposable)
            {
                disposable.Dispose();
            }
        });
    }

    private void Show<TView>(Window owner, object? viewModel = null) where TView : Window, new()
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
                window.Show(owner);
            }

            return;
        }

        var view = new TView { DataContext = viewModel };

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

            _viewModel?.OnViewClosed(viewModel);
        };

        _windows.Add(viewType, view);

        view.Show(this);
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