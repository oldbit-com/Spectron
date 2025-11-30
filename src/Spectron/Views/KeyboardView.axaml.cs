using System;
using Avalonia.Controls;
using Avalonia.Input;
using OldBit.Spectron.ViewModels;

namespace OldBit.Spectron.Views;

public partial class KeyboardView : Window
{
    private KeyboardViewModel? _viewModel;

    public KeyboardView() => InitializeComponent();

    protected override void OnDataContextChanged(EventArgs e)
    {
        if (DataContext is not KeyboardViewModel viewModel)
        {
            return;
        }

        _viewModel = viewModel;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (e.ClickCount > 1)
        {
            var point = e.GetPosition(this);
            _viewModel?.DoubleClick(point);

            return;
        }

        BeginMoveDrag(e);
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key != Key.Escape && e.Key != Key.F1)
        {
            return;
        }

        Close();
        e.Handled = true;
    }
}