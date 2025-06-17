using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;
using OldBit.Spectron.Messages;

namespace OldBit.Spectron.Views;

public partial class SelectArchiveFileView : Window
{
    public SelectArchiveFileView()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
        {
            return;
        }

        WeakReferenceMessenger.Default.Register<SelectArchiveFileView, SelectArchiveFileMessage>(this,
            static (window, message) => window.Close(message.SelectedFile));
    }
}