using System;
using Avalonia.Controls;
using OldBit.Spectron.ViewModels;

namespace OldBit.Spectron.Controls;

public partial class TimeMachineView : UserControl
{
    public TimeMachineView()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        if (DataContext is not TimeMachineViewModel viewModel)
        {
            return;
        }

        viewModel.PreviewControl = PreviewImage;
    }
}