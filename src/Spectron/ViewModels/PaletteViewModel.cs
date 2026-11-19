using System;
using System.Collections.ObjectModel;
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

public partial class PaletteViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    public PaletteType PaletteType { get; }

    public ObservableCollection<string> Colors { get; } = [];

    public bool IsDefault { get; private init; }

    private PaletteViewModel(PaletteType paletteType)
    {
        PaletteType = paletteType;

        var palette = new Palette();

        Colors.Add(palette.Ink.Black.ToHex());
        Colors.Add(palette.Ink.Blue.ToHex());
        Colors.Add(palette.Ink.Red.ToHex());
        Colors.Add(palette.Ink.Magenta.ToHex());
        Colors.Add(palette.Ink.Green.ToHex());
        Colors.Add(palette.Ink.Cyan.ToHex());
        Colors.Add(palette.Ink.Yellow.ToHex());
        Colors.Add(palette.Ink.White.ToHex());

        Colors.Add(palette.Bright.Ink.Black.ToHex());
        Colors.Add(palette.Bright.Ink.Blue.ToHex());
        Colors.Add(palette.Bright.Ink.Red.ToHex());
        Colors.Add(palette.Bright.Ink.Magenta.ToHex());
        Colors.Add(palette.Bright.Ink.Green.ToHex());
        Colors.Add(palette.Bright.Ink.Cyan.ToHex());
        Colors.Add(palette.Bright.Ink.Yellow.ToHex());
        Colors.Add(palette.Bright.Ink.White.ToHex());

        Colors.Add(palette.Paper.Black.ToHex());
        Colors.Add(palette.Paper.Blue.ToHex());
        Colors.Add(palette.Paper.Red.ToHex());
        Colors.Add(palette.Paper.Magenta.ToHex());
        Colors.Add(palette.Paper.Green.ToHex());
        Colors.Add(palette.Paper.Cyan.ToHex());
        Colors.Add(palette.Paper.Yellow.ToHex());
        Colors.Add(palette.Paper.White.ToHex());

        Colors.Add(palette.Bright.Paper.Black.ToHex());
        Colors.Add(palette.Bright.Paper.Blue.ToHex());
        Colors.Add(palette.Bright.Paper.Red.ToHex());
        Colors.Add(palette.Bright.Paper.Magenta.ToHex());
        Colors.Add(palette.Bright.Paper.Green.ToHex());
        Colors.Add(palette.Bright.Paper.Cyan.ToHex());
        Colors.Add(palette.Bright.Paper.Yellow.ToHex());
        Colors.Add(palette.Bright.Paper.White.ToHex());

        Colors.Add(palette.Border.Black.ToHex());
        Colors.Add(palette.Border.Blue.ToHex());
        Colors.Add(palette.Border.Red.ToHex());
        Colors.Add(palette.Border.Magenta.ToHex());
        Colors.Add(palette.Border.Green.ToHex());
        Colors.Add(palette.Border.Cyan.ToHex());
        Colors.Add(palette.Border.Yellow.ToHex());
        Colors.Add(palette.Border.White.ToHex());
    }

    private PaletteViewModel(PaletteType paletteType, string[] normal, string[] bright)
    {
        PaletteType = paletteType;

        Colors.AddRange(normal); // Ink
        Colors.AddRange(bright); // Ink bright
        Colors.AddRange(normal); // Paper
        Colors.AddRange(bright); // Paper bright
        Colors.AddRange(normal); // Border
    }

    public Palette ToPalette() => new()
    {
        Ink = new PaletteColor(
            Color.FromHex(Colors[0]),
            Color.FromHex(Colors[1]),
            Color.FromHex(Colors[2]),
            Color.FromHex(Colors[3]),
            Color.FromHex(Colors[4]),
            Color.FromHex(Colors[5]),
            Color.FromHex(Colors[6]),
            Color.FromHex(Colors[7])),

        Bright = new Bright(
            new PaletteColor(
                Color.FromHex(Colors[8]),
                Color.FromHex(Colors[9]),
                Color.FromHex(Colors[10]),
                Color.FromHex(Colors[11]),
                Color.FromHex(Colors[12]),
                Color.FromHex(Colors[13]),
                Color.FromHex(Colors[14]),
                Color.FromHex(Colors[15])
            ),

            new PaletteColor(
                Color.FromHex(Colors[24]),
                Color.FromHex(Colors[25]),
                Color.FromHex(Colors[26]),
                Color.FromHex(Colors[27]),
                Color.FromHex(Colors[28]),
                Color.FromHex(Colors[29]),
                Color.FromHex(Colors[30]),
                Color.FromHex(Colors[31])
            )),

        Paper = new PaletteColor(
            Color.FromHex(Colors[16]),
            Color.FromHex(Colors[17]),
            Color.FromHex(Colors[18]),
            Color.FromHex(Colors[19]),
            Color.FromHex(Colors[20]),
            Color.FromHex(Colors[21]),
            Color.FromHex(Colors[22]),
            Color.FromHex(Colors[23])),

        Border = new PaletteColor(
            Color.FromHex(Colors[32]),
            Color.FromHex(Colors[33]),
            Color.FromHex(Colors[34]),
            Color.FromHex(Colors[35]),
            Color.FromHex(Colors[36]),
            Color.FromHex(Colors[37]),
            Color.FromHex(Colors[38]),
            Color.FromHex(Colors[39])),
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