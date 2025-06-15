using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Messaging;
using OldBit.Spectron.Messages;

namespace OldBit.Spectron.Views;

public partial class PreferencesView : Window
{
    public PreferencesView()
    {
        InitializeComponent();

        WeakReferenceMessenger.Default.Register<PreferencesView, PreferencesViewClosedMessage>(this, static (window, message) =>
            window.Close(message.Preference));
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
}