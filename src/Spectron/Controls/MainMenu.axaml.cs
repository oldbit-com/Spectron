using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using OldBit.Spectron.ViewModels;

namespace OldBit.Spectron.Controls;

public partial class MainMenu : UserControl
{
    private MainViewModel? _viewModel;

    public MainMenu()
    {
        InitializeComponent();
        LostFocus += OnLostFocus;
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        if (DataContext is not MainViewModel viewModel)
        {
            return;
        }

        _viewModel = viewModel;

        _viewModel.FavoritesViewModel.WindowFavoritesMenu = FavoritesMenu;
    }

    private void OnLostFocus(object? sender, RoutedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_viewModel?.WindowState != WindowState.FullScreen)
            {
                return;
            }

            var focused = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement() as Control;
            var current = focused;

            while (current != null && current != this)
            {
                current = current.Parent as Control;
            }

            if (current != this)
            {
                _viewModel.IsMenuVisible = false;
            }
        });
    }

    private void RecentFilesSubmenuOpened(object? sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem || !ReferenceEquals(sender, e.Source))
        {
            return;
        }

        _viewModel?.RecentFilesViewModel.Opening(menuItem.Items);
    }
}