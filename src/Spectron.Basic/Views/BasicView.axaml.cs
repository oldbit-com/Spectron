using Avalonia.Controls;
using Avalonia.Interactivity;
using OldBit.Spectron.Basic.ViewModels;

namespace OldBit.Spectron.Basic.Views;

public partial class BasicView : Window
{
    private BasicViewModel? _viewModel;

    public BasicView() => InitializeComponent();

    protected override void OnDataContextChanged(EventArgs e)
    {
        if (DataContext is not BasicViewModel viewModel)
        {
            return;
        }

        _viewModel = viewModel;
        _viewModel.BasicViewer = BasicViewer;
    }

    private void Control_OnLoaded(object? sender, RoutedEventArgs e) => _viewModel?.Loaded();
}