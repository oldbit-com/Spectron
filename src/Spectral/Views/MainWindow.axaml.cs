using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using OldBit.Spectral.Emulator.Computers;
using OldBit.Spectral.Helpers;
using OldBit.Spectral.ViewModels;

namespace OldBit.Spectral.Views;

public partial class MainWindow : Window
{
    private ISpectrum _emulator = default!;
    private bool _isPaused;

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

        viewModel.MainWindow = this;
        viewModel.ScreenControl = ScreenImage;

        viewModel.Initialize();
        _emulator = viewModel.Emulator!;
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        var spectrumKey = KeyMappings.ToSpectrumKey(e);
        if (spectrumKey.Count > 0)
        {
            _emulator.Keyboard.HandleKeyDown(spectrumKey);
        }
    }

    private void OnKeyUp(object? sender, KeyEventArgs e)
    {
        var spectrumKey = KeyMappings.ToSpectrumKey(e);
        if (spectrumKey.Count > 0)
        {
            _emulator.Keyboard.HandleKeyUp(spectrumKey);
        }
    }
}