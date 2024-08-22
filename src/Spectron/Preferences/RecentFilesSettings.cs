using System.Collections.Generic;

namespace OldBit.Spectron.Preferences;

public class RecentFilesSettings
{
    public string FileName => "recentFiles.json";

    public List<string> RecentFiles { get; set; } = [];
}