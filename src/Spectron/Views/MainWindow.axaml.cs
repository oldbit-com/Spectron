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
    private MainWindowViewModel? _viewModel;
    private readonly Dictionary<string, Window> _windows = new();

    public MainWindow()
    {
        InitializeComponent();

        WeakReferenceMessenger.Default.Register<MainWindow, ShowAboutViewMessage>(this, static (window, _) =>
            ShowDialog<AboutView>(window));

        WeakReferenceMessenger.Default.Register<MainWindow, ShowDebuggerViewMessage>(this, static (window, m) =>
            ShowDialog<DebuggerView>(window, m.ViewModel!));

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

        WeakReferenceMessenger.Default.Register<MainWindow, ShowTapeViewMessage>(this, static (window, message) =>
            ShowDialog<TapeView>(window, new TapeViewModel(message.TapeManager)));

        WeakReferenceMessenger.Default.Register<MainWindow, ShowTimeMachineViewMessage>(this, (window, message) =>
        {
            var result = ShowDialog<TimeMachineView, TimeMachineEntry>(window, message.ViewModel);
            message.Reply(result);
        });

        WeakReferenceMessenger.Default.Register<MainWindow, ShowTrainerViewMessage>(this, static (window, message) =>
            ShowDialog<TrainerView>(window, new TrainerViewModel(message.Emulator, message.PokeFile)));
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

    private static async Task<TResponse?> ShowDialog<TView, TResponse>(Window owner, object? viewModel = null) where TView : Window, new()
    {
        var view = new TView { DataContext = viewModel };

        var result = await view.ShowDialog<TResponse?>(owner);

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