using Avalonia.Controls;
using OldBit.Spectron.Screen;

namespace OldBit.Spectron.ViewModels;

partial class MainWindowViewModel
{
    private void HandleChangeBorderSize(BorderSize borderSize)
    {
        BorderSize = borderSize;

        _frameBufferConverter.SetBorderSize(borderSize);
        SpectrumScreen = _frameBufferConverter.ScreenBitmap;
    }

    private void HandleToggleFullScreen()
    {
        WindowState = WindowState == WindowState.FullScreen ? WindowState.Normal : WindowState.FullScreen;
        IsFullScreen = WindowState == WindowState.FullScreen;
    }

    partial void OnWindowStateChanged(WindowState value) =>
        IsMenuVisible = value != WindowState.FullScreen;
}