using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OldBit.Spectron.Emulation.Screen;
using OldBit.Spectron.Emulation.Screen.Palettes;
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

    public NativeMenu? NativeFavoriteMenu { get; set; }
    public MenuItem? WindowFavoritesMenu { get; set; }

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

    public void UpdatePalettes()
    {

    }

    public void RefreshMenu()
    {
        RefreshWindowMenu();
        RefreshNativeMenu();
    }

    private void RefreshWindowMenu()
    {
        if (WindowFavoritesMenu is null)
        {
            return;
        }
    }

    private void RefreshNativeMenu()
    {
        if (NativeFavoriteMenu is null)
        {
            return;
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

    [RelayCommand]
    private void UsePalette()
    {
        if (SelectedPalette is null)
        {
            return;
        }

        var palette = new Palette
        {
            Ink = new PaletteColor(
                Color.FromHex(SelectedPalette.Colors[0]),
                Color.FromHex(SelectedPalette.Colors[1]),
                Color.FromHex(SelectedPalette.Colors[2]),
                Color.FromHex(SelectedPalette.Colors[3]),
                Color.FromHex(SelectedPalette.Colors[4]),
                Color.FromHex(SelectedPalette.Colors[5]),
                Color.FromHex(SelectedPalette.Colors[6]),
                Color.FromHex(SelectedPalette.Colors[7])),

            Bright = new Bright(
                new PaletteColor(
                    Color.FromHex(SelectedPalette.Colors[8]),
                    Color.FromHex(SelectedPalette.Colors[9]),
                    Color.FromHex(SelectedPalette.Colors[10]),
                    Color.FromHex(SelectedPalette.Colors[11]),
                    Color.FromHex(SelectedPalette.Colors[12]),
                    Color.FromHex(SelectedPalette.Colors[13]),
                    Color.FromHex(SelectedPalette.Colors[14]),
                    Color.FromHex(SelectedPalette.Colors[15])
                    ),

                new PaletteColor(
                    Color.FromHex(SelectedPalette.Colors[24]),
                    Color.FromHex(SelectedPalette.Colors[25]),
                    Color.FromHex(SelectedPalette.Colors[26]),
                    Color.FromHex(SelectedPalette.Colors[27]),
                    Color.FromHex(SelectedPalette.Colors[28]),
                    Color.FromHex(SelectedPalette.Colors[29]),
                    Color.FromHex(SelectedPalette.Colors[30]),
                    Color.FromHex(SelectedPalette.Colors[31])
                )),

            Paper = new PaletteColor(
                Color.FromHex(SelectedPalette.Colors[16]),
                Color.FromHex(SelectedPalette.Colors[17]),
                Color.FromHex(SelectedPalette.Colors[18]),
                Color.FromHex(SelectedPalette.Colors[19]),
                Color.FromHex(SelectedPalette.Colors[20]),
                Color.FromHex(SelectedPalette.Colors[21]),
                Color.FromHex(SelectedPalette.Colors[22]),
                Color.FromHex(SelectedPalette.Colors[23])),

            Border = new PaletteColor(
                Color.FromHex(SelectedPalette.Colors[32]),
                Color.FromHex(SelectedPalette.Colors[33]),
                Color.FromHex(SelectedPalette.Colors[34]),
                Color.FromHex(SelectedPalette.Colors[35]),
                Color.FromHex(SelectedPalette.Colors[36]),
                Color.FromHex(SelectedPalette.Colors[37]),
                Color.FromHex(SelectedPalette.Colors[38]),
                Color.FromHex(SelectedPalette.Colors[39])),
        };

        SpectrumPalette.SetPalette(palette);
    }

    private bool CanDeletePalette() => SelectedPalette is { IsReadOnly: false };

    private bool CanClonePalette() => SelectedPalette != null;
}