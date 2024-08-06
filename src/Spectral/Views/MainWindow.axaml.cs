using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using OldBit.Spectral.Dialogs;
using OldBit.Spectral.Emulation.Computers;
using OldBit.Spectral.Helpers;
using OldBit.Spectral.ViewModels;

namespace OldBit.Spectral.Views;

public partial class MainWindow : Window
{
    private MainWindowViewModel? _viewModel;

    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        _viewModel = viewModel;

        FileDialogs.MainWindow = this;
        MessageDialogs.MainWindow = this;

        _viewModel.ScreenControl = ScreenImage;

        _viewModel.Initialize(Computer.Spectrum48K);
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        var keys = KeyMappings.ToSpectrumKey(e);
        _viewModel?.KeyDown(keys);
    }

    private void OnKeyUp(object? sender, KeyEventArgs e)
    {
        var keys = KeyMappings.ToSpectrumKey(e);
        _viewModel?.KeyUp(keys);
    }
}