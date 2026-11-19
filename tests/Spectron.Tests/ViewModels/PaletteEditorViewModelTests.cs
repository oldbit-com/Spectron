using OldBit.Spectron.Emulation.Screen.Palettes;
using OldBit.Spectron.ViewModels;

namespace OldBit.Spectron.Tests.ViewModels;

public class PaletteEditorViewModelTests
{
    [Fact]
    public void Update_WithArgbHex_NormalizesToRgbHex()
    {
        var viewModel = new PaletteEditorViewModel();
        var colors = Enumerable.Repeat("#ff123456", 40).ToList();

        viewModel.Update(new Dictionary<PaletteType, IList<string>> { [PaletteType.Custom] = colors });

        var custom = viewModel.Palettes.Single(p => p.PaletteType == PaletteType.Custom);
        custom.Colors.ShouldAllBe(hex => hex == "#123456");
    }

    [Fact]
    public void Update_WithInvalidHex_KeepsExistingColors()
    {
        var viewModel = new PaletteEditorViewModel();
        var custom = viewModel.Palettes.Single(p => p.PaletteType == PaletteType.Custom);
        var original = custom.Colors.ToList();
        var colors = Enumerable.Repeat("#123456", 40).ToList();
        colors[5] = "garbage";

        viewModel.Update(new Dictionary<PaletteType, IList<string>> { [PaletteType.Custom] = colors });

        custom.Colors.ShouldBe(original);
    }

    [Fact]
    public void PresetPalettes_UseRgbHex()
    {
        var viewModel = new PaletteEditorViewModel();

        viewModel.Palettes
            .SelectMany(p => p.Colors)
            .ShouldAllBe(hex => hex.Length == 7 && hex[0] == '#');
    }

    [Fact]
    public void ToPalette_StandardPalette_MatchesDefaultPalette()
    {
        var expected = new Palette();

        var palette = PaletteViewModel.CreatePalette(PaletteType.Standard).ToPalette();

        palette.Ink.ShouldBe(expected.Ink);
        palette.Paper.ShouldBe(expected.Paper);
        palette.Bright.ShouldBe(expected.Bright);
        palette.Border.ShouldBe(expected.Border);
    }

    [Fact]
    public void ToPalette_MapsColorGroupsInStoredOrder()
    {
        var viewModel = PaletteViewModel.CreatePalette(PaletteType.Custom);

        for (var i = 0; i < viewModel.Colors.Count; i++)
        {
            viewModel.Colors[i] = $"#0000{i:X2}";
        }

        var palette = viewModel.ToPalette();

        palette.Ink.Black.Blue.ShouldBe((byte)0);
        palette.Bright.Ink.Black.Blue.ShouldBe((byte)8);
        palette.Paper.Black.Blue.ShouldBe((byte)16);
        palette.Bright.Paper.Black.Blue.ShouldBe((byte)24);
        palette.Border.Black.Blue.ShouldBe((byte)32);
        palette.Border.White.Blue.ShouldBe((byte)39);
    }

    [Fact]
    public void PresetPalette_UsesNormalColorsForInkPaperBorderAndBrightForBrightGroups()
    {
        var palette = PaletteViewModel.CreatePalette(PaletteType.Amber).ToPalette();

        palette.Paper.ShouldBe(palette.Ink);
        palette.Border.ShouldBe(palette.Ink);
        palette.Bright.Paper.ShouldBe(palette.Bright.Ink);
        palette.Bright.Ink.ShouldNotBe(palette.Ink);
    }
}
