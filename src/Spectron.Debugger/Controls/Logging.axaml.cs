using Avalonia.Controls;
using OldBit.Spectron.Debugger.ViewModels;

namespace OldBit.Spectron.Debugger.Controls;

public partial class Logging : UserControl
{
    public Logging() => InitializeComponent();

    protected override void OnDataContextChanged(EventArgs e)
    {
        if (DataContext is LoggingViewModel viewModel)
        {
            viewModel.Control = this;
        }
    }
}