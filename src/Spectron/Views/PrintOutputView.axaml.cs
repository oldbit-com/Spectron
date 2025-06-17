using System;
using Avalonia.Controls;
using OldBit.Spectron.ViewModels;

namespace OldBit.Spectron.Views;

public partial class PrintOutputView : Window
{
    public PrintOutputView() => InitializeComponent();

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is PrintOutputViewModel viewModel)
        {
            viewModel.PreviewControl = OutputImage;
        }
    }
}