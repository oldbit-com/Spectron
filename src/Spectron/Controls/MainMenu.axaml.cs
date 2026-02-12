using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using OldBit.Spectron.ViewModels;

namespace OldBit.Spectron.Controls;

public partial class MainMenu : UserControl
{
    private MainWindowViewModel? _viewModel;

    public MainMenu() => InitializeComponent();

    protected override void OnDataContextChanged(EventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        _viewModel = viewModel;
    }

    private void RecentFilesSubmenuOpened(object? sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem || !ReferenceEquals(sender, e.Source))
        {
            return;
        }

        _viewModel?.RecentFilesViewModel.Opening(menuItem.Items);
    }

    private void FavoritesSubmenuOpened(object? sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem || !ReferenceEquals(sender, e.Source))
        {
            return;
        }

        _viewModel?.FavoritesViewModel.Opening(menuItem.Items);
    }
}