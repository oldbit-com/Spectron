using System;
using Avalonia.ReactiveUI;
using OldBit.Spectron.ViewModels;

namespace OldBit.Spectron.Views;

public partial class PrintOutputView : ReactiveWindow<PrintOutputViewModel>
{
    public PrintOutputView() => InitializeComponent();

    protected override void OnDataContextChanged(EventArgs e)
    {
        if (ViewModel != null)
        {
            ViewModel.PreviewControl = OutputImage;
        }
    }
}