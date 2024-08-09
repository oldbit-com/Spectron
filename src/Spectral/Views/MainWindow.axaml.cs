using System;
using Avalonia.Controls;
using Avalonia.Input;
using OldBit.Spectral.Dialogs;
using OldBit.Spectral.Helpers;
using OldBit.Spectral.ViewModels;
using ReactiveUI;

namespace OldBit.Spectral.Views;

public partial class MainWindow : Window
{
    private MainWindowViewModel? _viewModel;

    public MainWindow()
    {
        InitializeComponent();
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