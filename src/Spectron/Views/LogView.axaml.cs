using Avalonia.Controls;
using Avalonia.Input;

namespace OldBit.Spectron.Views;

public partial class LogView : Window
{
    public LogView() => InitializeComponent();

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key != Key.Escape)
        {
            return;
        }

        e.Handled = true;
        Close();
    }
}