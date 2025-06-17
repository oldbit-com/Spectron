using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using OldBit.Spectron.ViewModels;
using VisualExtensions = Avalonia.VisualTree.VisualExtensions;

namespace OldBit.Spectron.Views;

public partial class TapeView : Window
{
    public TapeView() => InitializeComponent();

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key != Key.Escape)
        {
            return;
        }

        e.Handled = true;
        Close();
    }

    private void CancelButton_OnClick(object? sender, RoutedEventArgs e) => Close();

    private void InputElement_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (e.Source is not Visual visual)
        {
            return;
        }

        var row = VisualExtensions.FindAncestorOfType<DataGridRow>(visual);

        if (row?.DataContext is not TapeBlockViewModel blockViewModel)
        {
            return;
        }

        if (DataContext is not TapeViewModel tapeViewModel)
        {
            return;
        }

        if (blockViewModel.Index != null)
        {
            tapeViewModel.SetActiveBlock(blockViewModel.Index.Value - 1);
        }
    }
}