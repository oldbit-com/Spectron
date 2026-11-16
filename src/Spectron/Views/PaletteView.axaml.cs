using Avalonia.Controls;
using OldBit.Spectron.ViewModels;

namespace OldBit.Spectron.Views;

public partial class PaletteView : Window
{
    public PaletteView()
    {
        InitializeComponent();
        DataContext = new PaletteEditorViewModel();
    }
}