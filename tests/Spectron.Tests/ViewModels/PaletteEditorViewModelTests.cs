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
}
