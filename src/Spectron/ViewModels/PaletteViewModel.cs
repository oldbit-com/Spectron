using CommunityToolkit.Mvvm.ComponentModel;

namespace OldBit.Spectron.ViewModels;

public class PaletteViewModel : ObservableObject
{
    public string[] Colors { get; } =
    [
        // Ink
        "#FF000000", "#FF0000D8", "#FFD80000", "#FFD800D8", "#FF00D800", "#FF00D8D8", "#FFD8D800", "#FFD8D8D8",
        // Ink Bright
        "#FF000000", "#FF0000FF", "#FFD80000", "#FFD800FF", "#FF00FF00", "#FF00FFFF", "#FFFFFF00", "#FFFFFFFF",
        // Paper
        "#FF000000", "#FF0000D8", "#FFD80000", "#FFD800D8", "#FF00D800", "#FF00D8D8", "#FFD8D800", "#FFD8D8D8",
        // Paper Bright
        "#FF000000", "#FF0000FF", "#FFD80000", "#FFD800FF", "#FF00FF00", "#FF00FFFF", "#FFFFFF00", "#FFFFFFFF",
        // Border
        "#FF000000", "#FF0000D8", "#FFD80000", "#FFD800D8", "#FF00D800", "#FF00D8D8", "#FFD8D800", "#FFD8D8D8",
    ];
}