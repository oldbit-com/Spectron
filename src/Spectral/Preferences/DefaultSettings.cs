using OldBit.Spectral.Emulation.Computers;
using OldBit.Spectral.Models;

namespace OldBit.Spectral.Preferences;

public class DefaultSettings
{
    public bool IsUlaPlusEnabled { get; set; }

    public BorderSize BorderSize { get; set; } = BorderSize.Medium;

    public ComputerType ComputerType { get; set; } = ComputerType.Spectrum48K;

    public int MaxRecentFiles { get; set; } = 10;
}