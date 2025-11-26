using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace OldBit.Spectron.Views;

public partial class LogView : Window
{
    public LogView()
    {
        InitializeComponent();

        Editor.AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape)
        {
            return;
        }

        e.Handled = true;
        Close();
    }
}