using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using OldBit.Spectron.Emulation.Files;
using OldBit.Spectron.ViewModels;

namespace OldBit.Spectron.Views;

public partial class FavoritesView : Window
{
    private FavoritesViewModel? _viewModel;

    public FavoritesView() => InitializeComponent();

    protected override void OnDataContextChanged(EventArgs e)
    {
        if (DataContext is not FavoritesViewModel viewModel)
        {
            return;
        }

        _viewModel = viewModel;
        _viewModel.SelectedItem = viewModel.Nodes.FirstOrDefault();
    }

    private void OnFileDragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = DragDropEffects.None;

        if (IsValidFileDrop(e))
        {
            e.DragEffects = DragDropEffects.Copy;
        }

        e.Handled = true;
    }

    private void OnFileDrop(object? sender, DragEventArgs e)
    {
        if (!IsValidFileDrop(e))
        {
            return;
        }

        var filePath = GetDroppedFilePath(e);

        if (!IsFileSupported(filePath))
        {
            return;
        }

        _viewModel?.SelectedItem?.Path = filePath!;
    }

    private static bool IsValidFileDrop(DragEventArgs e)
    {
        if (!e.DataTransfer.Contains(DataFormat.File) || e.DataTransfer.Items.Count != 1)
        {
            return false;
        }

        var filePath = GetDroppedFilePath(e);

        return IsFileSupported(filePath);
    }

    private static bool IsFileSupported(string? filePath)
    {
        if (filePath == null)
        {
            return false;
        }

        var fileType = FileTypes.GetFileType(filePath);

        return fileType != FileType.Unsupported;
    }

    private static string? GetDroppedFilePath(DragEventArgs e)
    {
        var items = e.DataTransfer.GetItems(DataFormat.File).FirstOrDefault();
        var file = items?.TryGetFile();

        return file?.Path.LocalPath;
    }
}