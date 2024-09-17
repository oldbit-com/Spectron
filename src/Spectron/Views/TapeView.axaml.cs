using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace OldBit.Spectron.Views;

public partial class TapeView : Window
{
    public TapeView()
    {
        InitializeComponent();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key != Key.Escape)
        {
            return;
        }

        e.Handled = true;
        Close();
    }

    private void CancelButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}