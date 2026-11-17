using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using OldBit.Spectron.Emulation.Screen.Palettes;
using OldBit.Spectron.Extensions;

namespace OldBit.Spectron.ViewModels;

public partial class PaletteViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    public ObservableCollection<string> Colors { get; } = [];

    public bool IsReadOnly { get; init; }

    public PaletteViewModel()
    {
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
}