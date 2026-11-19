using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using AvaloniaEdit.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OldBit.Spectron.Emulation.Screen;
using OldBit.Spectron.Emulation.Screen.Palettes;
using OldBit.Spectron.Extensions;

namespace OldBit.Spectron.ViewModels;

public partial class PaletteEditorViewModel : ObservableObject
{
    public ObservableCollection<PaletteViewModel> Palettes { get; } = [];

    public Action<Palette> SetEmulatorPalette { get; set; } = _ => {};

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ResetPaletteCommand))]
    [NotifyPropertyChangedFor(nameof(IsEditable))]
    public partial PaletteViewModel? SelectedPalette { get; set; }

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

    public Dictionary<PaletteType, IList<string>> ToDictionary() => Palettes
        .Where(p => p.PaletteType != PaletteType.Standard)
        .ToDictionary<PaletteViewModel, PaletteType, IList<string>>(p => p.PaletteType, p => p.Colors);

    public void Update(Dictionary<PaletteType, IList<string>> palettes)
    {
        foreach (var palette in palettes)
        {
            var existingPalette = Palettes.FirstOrDefault(p => p.PaletteType == palette.Key);

            if (existingPalette is null ||
                palette.Value.Count != PaletteViewModel.ColorCount ||
                !palette.Value.All(hex => ColorExtensions.TryParseHex(hex, out _)))
            {
                continue;
            }

            existingPalette.Colors.Clear();
            existingPalette.Colors.AddRange(palette.Value.Select(hex => Color.FromHex(hex).ToHex()));
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