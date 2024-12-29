using System;
using Avalonia.Controls;
using OldBit.Spectron.ViewModels;

namespace OldBit.Spectron.Controls;

public partial class GamepadControlsMapping : UserControl
{
    public GamepadControlsMapping()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        var viewModel = DataContext as GamepadMappingViewModel;

        if (viewModel != null)
        {
            viewModel.ScrollIntoView = ScrollIntoView;
        }
    }

    private void ScrollIntoView(GamepadButtonMappingViewModel mapping)
    {
        DataGrid.ScrollIntoView(mapping, null);
    }
}