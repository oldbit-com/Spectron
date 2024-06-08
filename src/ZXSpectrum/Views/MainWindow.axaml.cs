using System;
using System.Linq;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using OldBit.ZXSpectrum.Emulator;
using OldBit.ZXSpectrum.Emulator.Screen;
using OldBit.ZXSpectrum.Helpers;
using OldBit.ZXSpectrum.ViewModels;

namespace OldBit.ZXSpectrum.Views;

public partial class MainWindow : Window
{
    private ISpectrum _spectrum = default!;

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
        _spectrum = new Spectrum48
        {
            OnScreenUpdate = (pixels) =>
            {
                var image = new WriteableBitmap(
                    new PixelSize(ScreenRenderer.TotalWidth, ScreenRenderer.TotalHeight),
                    new Vector(96, 96),
                    PixelFormat.Rgba8888);
                using var frameBuffer = image.Lock();

                Marshal.Copy(pixels, 0, frameBuffer.Address, pixels.Length);

                Dispatcher.UIThread.Post(() => ScreenImage.Source = image);
            }
        };

        _spectrum.Start();
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        var spectrumKey = KeyMappings.ToSpectrumKey(e);
        if (spectrumKey.Count > 0)
        {
            _spectrum.Keyboard.KeyDown(spectrumKey);
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