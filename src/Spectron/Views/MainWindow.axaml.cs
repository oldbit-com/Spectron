using System;
using Avalonia.Controls;
using OldBit.Spectron.Dialogs;
using OldBit.Spectron.ViewModels;

namespace OldBit.Spectron.Views;

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
        viewModel.MainWindow = this;
    }
}