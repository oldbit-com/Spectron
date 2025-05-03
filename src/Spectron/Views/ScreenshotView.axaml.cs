using System;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using OldBit.Spectron.ViewModels;

namespace OldBit.Spectron.Views;

public partial class ScreenshotView : ReactiveWindow<ScreenshotViewModel>
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
        if (ViewModel != null)
        {
            ViewModel.Window = this;
        }
    }
}