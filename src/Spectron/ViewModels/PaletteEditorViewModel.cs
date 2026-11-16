using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OldBit.Spectron.Extensions;

namespace OldBit.Spectron.ViewModels;

public partial class PaletteEditorViewModel : ObservableObject
{
    public ObservableCollection<PaletteViewModel> Palettes { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ClonePaletteCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeletePaletteCommand))]
    [NotifyPropertyChangedFor(nameof(IsEditable))]
    [NotifyPropertyChangedFor(nameof(PaletteName))]
    public partial PaletteViewModel? SelectedPalette { get; set; }

    [ObservableProperty]
    public partial string PaletteName { get; set; } = string.Empty;

    public bool IsEditable => SelectedPalette is not { IsReadOnly: true };

    public PaletteEditorViewModel()
    {
        Palettes.Add(new PaletteViewModel { Name = "Standard", IsReadOnly = true });
        SelectedPalette = Palettes[0];
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        switch (e.PropertyName)
        {
            case nameof(SelectedPalette):
                PaletteName = SelectedPalette?.Name ?? string.Empty;
                break;

            case nameof(PaletteName) when !string.Equals(PaletteName, SelectedPalette?.Name, StringComparison.CurrentCulture):
                SelectedPalette?.Name = PaletteName;
                break;
        }
    }

    [RelayCommand(CanExecute = nameof(CanClonePalette))]
    private void ClonePalette()
    {
        if (SelectedPalette is null)
        {
            return;
        }

        var palette = new PaletteViewModel
        {
            Name = (SelectedPalette.Name + " Copy").Truncate(50),
            IsReadOnly = false
        };

        Palettes.Add(palette);
        SelectedPalette = palette;
    }

    [RelayCommand(CanExecute = nameof(CanDeletePalette))]
    private void DeletePalette()
    {
        if (SelectedPalette is null || SelectedPalette.IsReadOnly)
        {
            return;
        }

        Palettes.Remove(SelectedPalette);
        SelectedPalette = Palettes.FirstOrDefault();
    }

    private bool CanDeletePalette() => SelectedPalette is { IsReadOnly: false };

    private bool CanClonePalette() => SelectedPalette != null;
}