using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;

namespace OldBit.Spectron.ViewModels;

public partial class GameViewModel : ObservableObject
{
    public int EntryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public ObservableCollection<string> Authors { get; set; } = [];
    public string Availability { get; set; } = string.Empty;
    public string? AvailabilityType { get; set; }
    public string Machine { get; set; } = string.Empty;
    public MaterialIconKind Icon => ResolveIcon(Genre);
    public ObservableCollection<NameValuePair<string>> Files { get; set; } = [];

    [ObservableProperty]
    public partial byte[]? ScreenData { get; set; }

    [ObservableProperty]
    public partial string? SelectedFile { get; set; }

    private static MaterialIconKind ResolveIcon(string genre)
    {
        if (genre.Contains("Game: ", StringComparison.OrdinalIgnoreCase))
        {
            return MaterialIconKind.GamepadVariantOutline;
        }

        if (genre.Contains("Programming: ", StringComparison.OrdinalIgnoreCase))
        {
            return MaterialIconKind.Variable;
        }

        if (genre.Contains("Utility: ", StringComparison.OrdinalIgnoreCase))
        {
            return MaterialIconKind.Tools;
        }

        if (genre.Contains("Compilation: ", StringComparison.OrdinalIgnoreCase) ||
            genre.Equals("Compilation", StringComparison.OrdinalIgnoreCase))
        {
            return MaterialIconKind.Cassette;
        }

        if (genre.Contains("General: Business", StringComparison.OrdinalIgnoreCase))
        {
            return MaterialIconKind.HandshakeOutline;
        }

        if (genre.Contains("General: Domestic", StringComparison.OrdinalIgnoreCase))
        {
            return MaterialIconKind.HomeOutline;
        }

        if (genre.Contains("General: Education", StringComparison.OrdinalIgnoreCase))
        {
            return MaterialIconKind.SchoolOutline;
        }

        if (genre.Contains("General: Industrial", StringComparison.OrdinalIgnoreCase))
        {
            return MaterialIconKind.Factory;
        }

        if (genre.Contains("General: Simulation", StringComparison.OrdinalIgnoreCase))
        {
            return MaterialIconKind.HeadCogOutline;
        }

        if (genre.Contains("Demoscene: ", StringComparison.OrdinalIgnoreCase))
        {
            return MaterialIconKind.StarShootingOutline;
        }

        return MaterialIconKind.HelpRhombusOutline;
    }
}
