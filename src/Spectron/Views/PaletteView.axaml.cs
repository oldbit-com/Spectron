using System;
using Avalonia.Controls;
using OldBit.Spectron.ViewModels;

namespace OldBit.Spectron.Views;

public partial class PaletteView : Window
{
    private PaletteEditorViewModel? _viewModel;

    public PaletteView() => InitializeComponent();

    protected override void OnDataContextChanged(EventArgs e)
    {
        if (DataContext is not PaletteEditorViewModel viewModel)
        {
            return;
        }

        _viewModel = viewModel;
    }

    protected override void OnClosed(EventArgs e)
    {
        _viewModel?.UpdatePalettes();
        base.OnClosed(e);
    }
}