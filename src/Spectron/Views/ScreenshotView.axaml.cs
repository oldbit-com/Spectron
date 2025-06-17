using System;
using Avalonia.Controls;
using Avalonia.Input;
using OldBit.Spectron.ViewModels;

namespace OldBit.Spectron.Views;

public partial class ScreenshotView : Window
{
    public ScreenshotView() => InitializeComponent();

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key != Key.Escape)
        {
            return;
        }

        e.Handled = true;
        Close();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is ScreenshotViewModel viewModel)
        {
            viewModel.Window = this;
        }
    }
}