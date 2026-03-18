using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using OldBit.Spectron.Emulation;

namespace OldBit.Spectron.ViewModels;

public partial class FavoriteSettingsViewModel : ObservableObject
{
    [ObservableProperty]
    private NameValuePair<ComputerType?> _computerType;

    public FavoriteSettingsViewModel(ComputerType? computerType = null)
    {
        ComputerType = ComputerTypes.FirstOrDefault(x => x.Value == computerType, ComputerTypes[0]);
    }

    public List<NameValuePair<ComputerType?>> ComputerTypes { get; } =
    [
        new("Default", null),
        new("ZX Spectrum 16k", OldBit.Spectron.Emulation.ComputerType.Spectrum16K),
        new("ZX Spectrum 48k", OldBit.Spectron.Emulation.ComputerType.Spectrum48K),
        new("ZX Spectrum 128k", OldBit.Spectron.Emulation.ComputerType.Spectrum128K),
    ];
}