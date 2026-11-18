using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OldBit.Spectron.Emulation.Screen.Palettes;

namespace OldBit.Spectron.ViewModels;

public partial class PaletteEditorViewModel : ObservableObject
{
    public ObservableCollection<PaletteViewModel> Palettes { get; } = [];

    public Action<Palette> SetEmulatorPalette { get; set; } = _ => {};

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ResetPaletteCommand))]
    [NotifyPropertyChangedFor(nameof(IsEditable))]
    [NotifyPropertyChangedFor(nameof(PaletteName))]
    public partial PaletteViewModel? SelectedPalette { get; set; }

    [ObservableProperty]
    public partial string PaletteName { get; set; } = string.Empty;

    public bool IsEditable => SelectedPalette is not { IsDefault: true };

    public PaletteEditorViewModel()
    {
        Palettes.Add(PaletteViewModel.CreatePalette(PaletteType.Standard));
        Palettes.Add(PaletteViewModel.CreatePalette(PaletteType.BlackAndWhite));
        Palettes.Add(PaletteViewModel.CreatePalette(PaletteType.Commodore));
        Palettes.Add(PaletteViewModel.CreatePalette(PaletteType.Green));
        Palettes.Add(PaletteViewModel.CreatePalette(PaletteType.Amber));
        Palettes.Add(PaletteViewModel.CreatePalette(PaletteType.Sepia));
        Palettes.Add(PaletteViewModel.CreatePalette(PaletteType.Custom));

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

    [RelayCommand(CanExecute = nameof(CanResetPalette))]
    private void ResetPalette()
    {
        if (SelectedPalette is null)
        {
            return;
        }

        var index = Palettes.IndexOf(SelectedPalette);
        if (index < 0)
        {
            return;
        }

        Palettes[index] = PaletteViewModel.CreatePalette(SelectedPalette.PaletteType);
        SelectedPalette = Palettes[index];
    }

    [RelayCommand]
    private void PreviewPalette()
    {
        if (SelectedPalette is null)
        {
            return;
        }

        SetEmulatorPalette(SelectedPalette.ToPalette());
    }

    private bool CanResetPalette() => SelectedPalette is { IsDefault: false };
}