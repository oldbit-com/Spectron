using Avalonia.Controls;
using Avalonia.Threading;
using OldBit.Spectron.Debugger.ViewModels;

namespace OldBit.Spectron.Debugger.Controls;

public partial class Memory : UserControl
{
    public Memory() => InitializeComponent();

    protected override void OnDataContextChanged(EventArgs e)
    {
        if (DataContext is not MemoryViewModel viewModel)
        {
            return;
        }

        viewModel.OnMemoryUpdated = (address, value) =>
        {
            Dispatcher.UIThread.Post(() => MemoryView.UpdateValues(address, value));
        };
    }
}