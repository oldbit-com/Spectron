using System.Collections.Generic;

namespace OldBit.Spectron.Settings;

public class RecentFilesSettings
{
    public List<string> Files { get; set; } = [];

    public string CurrentFileName { get; set; } = string.Empty;

    public int MaxRecentFiles { get; set; } = 10;
}