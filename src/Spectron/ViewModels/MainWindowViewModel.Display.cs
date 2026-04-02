using System;
using Avalonia.Controls;
using OldBit.Spectron.Controls;
using OldBit.Spectron.Screen;

namespace OldBit.Spectron.ViewModels;

partial class MainWindowViewModel
{
    private NativeMainMenu? _nativeMainMenu;

    private void HandleChangeBorderSize(BorderSize borderSize)
    {
        BorderSize = borderSize;

        _frameBufferConverter.SetBorderSize(borderSize);
        SpectrumScreen = _frameBufferConverter.ScreenBitmap;
    }

    private void HandleToggleFullScreen() =>
        WindowState = WindowState == WindowState.FullScreen ? WindowState.Normal : WindowState.FullScreen;

    partial void OnWindowStateChanged(WindowState value) =>
        IsMenuVisible = value != WindowState.FullScreen && !IsNativeMenuEnabled;

    partial void OnIsNativeMenuEnabledChanged(bool value)
    {
        if (!OperatingSystem.IsMacOS() || MainWindow == null)
        {
            return;
        }

        IsMenuVisible = WindowState != WindowState.FullScreen && !IsNativeMenuEnabled;

        if (IsNativeMenuEnabled)
        {
            _nativeMainMenu ??= new NativeMainMenu(this);
            NativeMenu.SetMenu(MainWindow, _nativeMainMenu.Create());
        }
        else if (_nativeMainMenu != null)
        {
            NativeMenu.SetMenu(MainWindow, _nativeMainMenu.Empty());
        }
    }
}