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
                ["#FF000000", "#FF181818", "#FF404040", "#FF595959", "#FF7E7E7E", "#FF979797", "#FFBEBEBE", "#FFD7D7D7"],
                ["#FF000000", "#FF1D1D1D", "#FF4C4C4C", "#FF696969", "#FF969696", "#FFB3B3B3", "#FFE2E2E2", "#FFFFFFFF"])
            {
                Name = "Black and White"
            },

            PaletteType.Commodore => new PaletteViewModel(paletteType,
                ["#FF000000", "#FF352879", "#FF68372B", "#FF6F3D86", "#FF588D43", "#FF70A4B2", "#FF6F4F25", "#FF959595"],
                ["#FF000000", "#FF6C5EB5", "#FF9A6759", "#FF8B5FA0", "#FF9AD284", "#FF8CC8D6", "#FFB8C76F", "#FFFFFFFF"])
            {
                Name = "Commodore"
            },

            PaletteType.Green => new PaletteViewModel(paletteType,
                ["#FF000000", "#FF041807", "#FF0C4013", "#FF11581A", "#FF197E25", "#FF1E962D", "#FF26BE39", "#FF2BD740"],
                ["#FF000000", "#FF051D08", "#FF0F4C16", "#FF15691F", "#FF1D952C", "#FF23B235", "#FF2DE143", "#FF33FF4C"])
            {
                Name = "Green"
            },

            PaletteType.Amber => new PaletteViewModel(paletteType,
                ["#FF000000", "#FF181100", "#FF402C00", "#FF583E00", "#FF7E5800", "#FF966900", "#FFBE8500", "#FFD79600"],
                ["#FF000000", "#FF1D1400", "#FF4C3500", "#FF694900", "#FF956800", "#FFB27D00", "#FFE19E00", "#FFFFB200"])
            {
                Name = "Amber"
            },

            PaletteType.Sepia => new PaletteViewModel(PaletteType.Sepia,
                ["#FF000000", "#FF18140F", "#FF403629", "#FF584B39", "#FF7E6B52", "#FF968061", "#FFBEA17B", "#FFD7B68B"],
                ["#FF000000", "#FF1D1812", "#FF4C4031", "#FF695944", "#FF957F61", "#FFB29774", "#FFE1C092", "#FFFFD8A5"])
            {
                Name = "Sepia"
            },

            PaletteType.Custom => new PaletteViewModel(PaletteType.Custom) { Name = "Custom" },

            _ => throw new ArgumentOutOfRangeException(nameof(paletteType))
        };
    }
}