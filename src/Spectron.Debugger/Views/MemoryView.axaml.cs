using Avalonia.Controls;
using Avalonia.Threading;
using OldBit.Spectron.Debugger.Converters;
using OldBit.Spectron.Debugger.ViewModels;

namespace OldBit.Spectron.Debugger.Views;

public partial class MemoryView : Window
{
    public MemoryView()
    {
        InitializeComponent();
        MemoryViewer.AsciiFormatter = new ZxAsciiConverter();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        if (DataContext is not MemoryViewModel viewModel)
        {
            return;
        }

        viewModel.Clipboard = Clipboard;
        viewModel.Viewer = MemoryViewer;

        viewModel.GoTo = address =>
            Dispatcher.UIThread.Post(() => MemoryViewer.Select(address));

        viewModel.Select = (address, length) =>
            Dispatcher.UIThread.Post(() => MemoryViewer.Select(address, length));

        viewModel.OnMemoryUpdated = (address, value) =>
            Dispatcher.UIThread.Post(() => MemoryViewer.UpdateValues(address, value));
    }
}