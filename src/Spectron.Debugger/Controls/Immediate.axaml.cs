using Avalonia.Controls;
using OldBit.Spectron.Debugger.ViewModels;

namespace OldBit.Spectron.Debugger.Controls;

public partial class Immediate : UserControl
{
    public Immediate() => InitializeComponent();

    protected override void OnDataContextChanged(EventArgs e)
    {
        if (DataContext is ImmediateViewModel viewModel)
        {
            viewModel.ScrollToEnd = () => OutputTextBox.CaretIndex = OutputTextBox.Text?.Length ?? 0;
        }
    }
}