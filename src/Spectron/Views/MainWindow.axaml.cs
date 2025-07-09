using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Messaging;
using OldBit.Spectron.Debugger.Views;
using OldBit.Spectron.Dialogs;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Files;
using OldBit.Spectron.Messages;
using OldBit.Spectron.Settings;
using OldBit.Spectron.ViewModels;

namespace OldBit.Spectron.Views;

public partial class MainWindow : Window
{
    private MainWindowViewModel? _mainViewModel;
    private readonly Dictionary<string, Window> _windows = new();

    public MainWindow()
    {
        InitializeComponent();

        WeakReferenceMessenger.Default.Register<MainWindow, ShowAboutViewMessage>(this, (window, _) =>
            ShowDialog<AboutView>(window));

        WeakReferenceMessenger.Default.Register<MainWindow, ShowDebuggerViewMessage>(this, (window, m) =>
            Show<DebuggerView>(window, m.ViewModel!));

        WeakReferenceMessenger.Default.Register<MainWindow, ShowKeyboardViewMessage>(this, (window, _) =>
            Show<HelpKeyboardView>(window));

        WeakReferenceMessenger.Default.Register<MainWindow, ShowPreferencesViewMessage>(this, (window, message) =>
        {
            var result = ShowDialog<PreferencesView, Preferences?>(window, new PreferencesViewModel(message.Preferences, message.GamepadManager));
            message.Reply(result);
        });

        WeakReferenceMessenger.Default.Register<MainWindow, ShowPrintOutputViewMessage>(this, (window, message) =>
            Show<PrintOutputView>(window, new PrintOutputViewModel(message.Printer)));

        WeakReferenceMessenger.Default.Register<MainWindow, ShowScreenshotViewMessage>(this, (window, message) =>
            Show<ScreenshotView>(window, message.ViewModel));

        WeakReferenceMessenger.Default.Register<MainWindow, ShowSelectArchiveFileViewMessage>(this, (window, message) =>
        {
            var result = ShowDialog<SelectArchiveFileView, ArchiveEntry?>(window, new SelectArchiveFileViewModel(message.FileNames));
            message.Reply(result);
        });

        WeakReferenceMessenger.Default.Register<MainWindow, ShowTapeViewMessage>(this, (window, message) =>
            ShowDialog<TapeView>(window, new TapeViewModel(message.TapeManager)));

        WeakReferenceMessenger.Default.Register<MainWindow, ShowTimeMachineViewMessage>(this, (window, message) =>
        {
            var result = ShowDialog<TimeMachineView, TimeMachineEntry>(window, message.ViewModel);
            message.Reply(result);
        });

        WeakReferenceMessenger.Default.Register<MainWindow, ShowTrainerViewMessage>(this, (window, message) =>
            ShowDialog<TrainerView>(window, new TrainerViewModel(message.Emulator, message.PokeFile)));
    }

    private void ShowDialog<TView>(Window owner, object? viewModel = null) where TView : Window, new()
    {
        var view = new TView { DataContext = viewModel };

        view.Closed += (_, _) =>
        {
            var viewModelType = viewModel?.GetType();

            if (viewModel is IDisposable disposable)
            {
                disposable.Dispose();
            }

            _mainViewModel?.OnViewClosed(viewModelType);
        };

        view.ShowDialog(owner);
    }

    private async Task<TResponse?> ShowDialog<TView, TResponse>(Window owner, object? viewModel = null) where TView : Window, new()
    {
        // Workaround for tooltips not always showing in the opened dialog window
        owner.IsHitTestVisible = false;

        var view = new TView { DataContext = viewModel };

        TResponse? result;

        var viewModelType = viewModel?.GetType();

        try
        {
            result = await view.ShowDialog<TResponse?>(owner);
        }
        finally
        {
            owner.IsHitTestVisible = true;
        }

        _mainViewModel?.OnViewClosed(viewModelType);

        if (viewModel is IDisposable disposable)
        {
            disposable.Dispose();
        }

        return result;

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

            var viewModelType = viewModel?.GetType();

            if (closedWindow.DataContext is IDisposable disposable)
            {
                disposable.Dispose();
            }

            _windows.Remove(viewType);

            _mainViewModel?.OnViewClosed(viewModelType);
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

        _mainViewModel = viewModel;
        _mainViewModel.ScreenControl = ScreenImage;

        FileDialogs.MainWindow = this;
        MessageDialogs.MainWindow = this;
        viewModel.MainWindow = this;
        viewModel.NotificationManager = NotificationManager;
    }

    private void Screen_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        var position = e.GetCurrentPoint(ScreenImage).Position;
        var bounds = ScreenImage.Bounds;

        _mainViewModel?.HandleMouseMoved(position, bounds);
    }

    private void Screen_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(ScreenImage);
        var bounds = ScreenImage.Bounds;

        _mainViewModel?.HandleMouseButtonStateChanged(point, bounds);
    }

    private void Screen_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        var point = e.GetCurrentPoint(ScreenImage);
        var bounds = ScreenImage.Bounds;

        _mainViewModel?.HandleMouseButtonStateChanged(point, bounds);
    }
}