using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using OldBit.Spectron.Debugger.ViewModels;

namespace OldBit.Spectron.Debugger.Views;

public partial class MemoryView : Window
{
    public MemoryView() => InitializeComponent();

    protected override void OnDataContextChanged(EventArgs e)
    {
        if (DataContext is not MemoryViewModel viewModel)
        {
            return;
        }

        viewModel.GoTo = address => Dispatcher.UIThread.Post(() => MemoryViewer.Select(address));

        viewModel.OnMemoryUpdated = (address, value) =>
        {
            Dispatcher.UIThread.Post(() => MemoryViewer.UpdateValues(address, value));
        };
    }
}