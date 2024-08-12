using Avalonia.Controls;
using Avalonia.Input;

namespace OldBit.Spectral.Views;

public partial class HelpKeyboardView : Window
{
    public HelpKeyboardView()
    {
        InitializeComponent();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (!e.Pointer.IsPrimary)
        {
            return;
        }

        BeginMoveDrag(e);
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key != Key.Escape)
        {
            return;
        }

        Close();
        e.Handled = true;
    }
}