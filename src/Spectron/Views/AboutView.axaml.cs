using Avalonia.Controls;
using Avalonia.Input;

namespace OldBit.Spectron.Views;

public partial class AboutView : Window
{
    public AboutView() => InitializeComponent();

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