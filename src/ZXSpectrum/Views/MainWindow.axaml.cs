using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using OldBit.ZXSpectrum.Emulator.Computers;
using OldBit.ZXSpectrum.Helpers;
using OldBit.ZXSpectrum.ViewModels;

namespace OldBit.ZXSpectrum.Views;

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
        //InitializeEmulator();

        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.MainWindow = this;
            viewModel.ScreenControl = ScreenImage;

            viewModel.Initialize();
            _emulator = viewModel.Emulator!;
        }
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        var spectrumKey = KeyMappings.ToSpectrumKey(e);
        if (spectrumKey.Count > 0)
        {
            _emulator.Keyboard.KeyDown(spectrumKey);
        }

        if (e.Key == Key.F11)
        {
            if (_isPaused)
            {
                _emulator.Resume();
            }
            else
            {
                _emulator.Pause();
            }

            _isPaused = !_isPaused;
        }
    }

    private void OnKeyUp(object? sender, KeyEventArgs e)
    {
        var spectrumKey = KeyMappings.ToSpectrumKey(e);
        if (spectrumKey.Count > 0)
        {
            _emulator.Keyboard.KeyUp(spectrumKey);
        }
    }
}