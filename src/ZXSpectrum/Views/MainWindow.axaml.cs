using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using OldBit.ZXSpectrum.Emulator.Computers;
using OldBit.ZXSpectrum.Emulator.Screen;
using OldBit.ZXSpectrum.Helpers;
using OldBit.ZXSpectrum.ViewModels;

namespace OldBit.ZXSpectrum.Views;

public partial class MainWindow : Window
{
    private ISpectrum _spectrum = default!;
    private bool _isPaused;

    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        InitializeEmulator();

        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.MainWindow = this;
            viewModel.Spectrum = _spectrum;
        }
    }

    private void InitializeEmulator()
    {
        var spectrum = new Spectrum48K();

        spectrum.RenderScreen += buffer =>
        {
            var bitmap = new WriteableBitmap(
                new PixelSize(FrameBuffer.Width, FrameBuffer.Height),
                new Vector(96, 96),
                PixelFormat.Rgba8888);

            using (var frameBuffer = bitmap.Lock())
            {
                var data = buffer.ToBytes();
                Marshal.Copy(data, 0, frameBuffer.Address, data.Length);
            }

            Dispatcher.UIThread.Post(() => ScreenImage.Source = bitmap);
        };

        _spectrum = spectrum;

        _spectrum.Start();
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        var spectrumKey = KeyMappings.ToSpectrumKey(e);
        if (spectrumKey.Count > 0)
        {
            _spectrum.Keyboard.KeyDown(spectrumKey);
        }

        if (e.Key == Key.F11)
        {
            if (_isPaused)
            {
                _spectrum.Resume();
            }
            else
            {
                _spectrum.Pause();
            }

            _isPaused = !_isPaused;
        }
    }

    private void OnKeyUp(object? sender, KeyEventArgs e)
    {
        var spectrumKey = KeyMappings.ToSpectrumKey(e);
        if (spectrumKey.Count > 0)
        {
            _spectrum.Keyboard.KeyUp(spectrumKey);
        }
    }
}