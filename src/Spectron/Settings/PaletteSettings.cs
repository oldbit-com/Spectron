using System.Collections.Generic;
using OldBit.Spectron.ViewModels;

namespace OldBit.Spectron.Settings;

public record PaletteSettings
{
    public Dictionary<PaletteType, IList<string>> PaletteColors { get; init; } = new();
}