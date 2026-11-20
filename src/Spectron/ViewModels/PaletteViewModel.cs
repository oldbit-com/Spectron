using System;
using System.Collections.ObjectModel;
using System.Linq;
using AvaloniaEdit.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using OldBit.Spectron.Emulation.Screen;
using OldBit.Spectron.Emulation.Screen.Palettes;
using OldBit.Spectron.Extensions;

namespace OldBit.Spectron.ViewModels;

public enum PaletteType
{
    Standard,
    BlackAndWhite,
    Green,
    Amber,
    Sepia,
    Commodore,
    Custom
}

public enum ColorGroup
{
    Ink,
    InkBright,
    Paper,
    PaperBright,
    Border
}

public partial class PaletteViewModel : ObservableObject
{
    private static readonly ColorGroup[] ColorGroups = Enum.GetValues<ColorGroup>();

    public static readonly int ColorCount = ColorGroups.Length * PaletteColor.Count;

    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    public PaletteType PaletteType { get; }

    public ObservableCollection<string> Colors { get; } = [];

    public bool IsDefault { get; private init; }

    private PaletteViewModel(PaletteType paletteType)
    {
        PaletteType = paletteType;

        var palette = new Palette();

        Colors.AddRange(ColorGroups
            .SelectMany(group => GetGroup(palette, group).ToArray())
            .Select(color => color.ToHex()));
    }

    private PaletteViewModel(PaletteType paletteType, string[] normal, string[] bright)
    {
        PaletteType = paletteType;

        foreach (var group in ColorGroups)
        {
            Colors.AddRange(group is ColorGroup.InkBright or ColorGroup.PaperBright ? bright : normal);
        }
    }

    public Palette ToPalette()
    {
        var colors = Colors.Select(hex => Color.FromHex(hex)).ToArray();

        return new Palette
        {
            Ink = Get(ColorGroup.Ink),
            Paper = Get(ColorGroup.Paper),
            Bright = new Bright(Get(ColorGroup.InkBright), Get(ColorGroup.PaperBright)),
            Border = Get(ColorGroup.Border),
        };

        PaletteColor Get(ColorGroup group) =>
            PaletteColor.From(colors.AsSpan((int)group * PaletteColor.Count, PaletteColor.Count));
    }

    private static PaletteColor GetGroup(Palette palette, ColorGroup group) => group switch
    {
        ColorGroup.Ink => palette.Ink,
        ColorGroup.InkBright => palette.Bright.Ink,
        ColorGroup.Paper => palette.Paper,
        ColorGroup.PaperBright => palette.Bright.Paper,
        ColorGroup.Border => palette.Border,
        _ => throw new ArgumentOutOfRangeException(nameof(group))
    };

    public static PaletteViewModel CreatePalette(PaletteType paletteType)
    {
        return paletteType switch
        {
            PaletteType.Standard => new PaletteViewModel(paletteType) { Name = "Standard", IsDefault = true },

            PaletteType.BlackAndWhite => new PaletteViewModel(paletteType,
                ["#000000", "#181818", "#404040", "#595959", "#7E7E7E", "#979797", "#BEBEBE", "#D7D7D7"],
                ["#000000", "#1D1D1D", "#4C4C4C", "#696969", "#969696", "#B3B3B3", "#E2E2E2", "#FFFFFF"])
            {
                Name = "Black and White"
            },

            PaletteType.Commodore => new PaletteViewModel(paletteType,
                ["#000000", "#352879", "#68372B", "#6F3D86", "#588D43", "#70A4B2", "#6F4F25", "#959595"],
                ["#000000", "#6C5EB5", "#9A6759", "#8B5FA0", "#9AD284", "#8CC8D6", "#B8C76F", "#FFFFFF"])
            {
                Name = "Commodore"
            },

            PaletteType.Green => new PaletteViewModel(paletteType,
                ["#000000", "#041807", "#0C4013", "#11581A", "#197E25", "#1E962D", "#26BE39", "#2BD740"],
                ["#000000", "#051D08", "#0F4C16", "#15691F", "#1D952C", "#23B235", "#2DE143", "#33FF4C"])
            {
                Name = "Green"
            },

            PaletteType.Amber => new PaletteViewModel(paletteType,
                ["#000000", "#181100", "#402C00", "#583E00", "#7E5800", "#966900", "#BE8500", "#D79600"],
                ["#000000", "#1D1400", "#4C3500", "#694900", "#956800", "#B27D00", "#E19E00", "#FFB200"])
            {
                Name = "Amber"
            },

            PaletteType.Sepia => new PaletteViewModel(PaletteType.Sepia,
                ["#000000", "#18140F", "#403629", "#584B39", "#7E6B52", "#968061", "#BEA17B", "#D7B68B"],
                ["#000000", "#1D1812", "#4C4031", "#695944", "#957F61", "#B29774", "#E1C092", "#FFD8A5"])
            {
                Name = "Sepia"
            },

            PaletteType.Custom => new PaletteViewModel(PaletteType.Custom) { Name = "Custom" },

            _ => throw new ArgumentOutOfRangeException(nameof(paletteType))
        };
    }
}