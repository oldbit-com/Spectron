using Avalonia.Controls;
using OldBit.Spectron.Debugger.ViewModels;

namespace OldBit.Spectron.Debugger.Controls;

public partial class BreakpointList : UserControl
{
    public BreakpointList() => InitializeComponent();

    private void Breakpoints_OnCellEditEnded(object? sender, DataGridCellEditEndedEventArgs e)
    {
        if (DataContext is BreakpointListViewModel viewModel && e.Row.DataContext is BreakpointViewModel item)
        {
            viewModel.UpdateBreakpoint(item);
        }
    }
}