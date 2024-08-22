using System.Collections.Generic;
using System.Threading.Tasks;

namespace OldBit.Spectron.Preferences;

public class RecentFilesManager
{
    private readonly int _maxRecentFiles;
    private RecentFilesSettings _recentFilesSettings = new();

    public RecentFilesManager(int maxRecentFiles)
    {
        _maxRecentFiles = maxRecentFiles;
    }

    public async Task LoadAsync()
    {
        _recentFilesSettings = await SettingsManager.LoadAsync<RecentFilesSettings>();
    }

    public void AppendAsync(string path)
    {
        _recentFilesSettings.RecentFiles.Remove(path);
        _recentFilesSettings.RecentFiles.Insert(0, path);

        if (_recentFilesSettings.RecentFiles.Count > _maxRecentFiles)
        {
            _recentFilesSettings.RecentFiles.RemoveRange(_maxRecentFiles,
                _recentFilesSettings.RecentFiles.Count - _maxRecentFiles);
        }
    }

    public IReadOnlyList<string> RecentFiles => _recentFilesSettings.RecentFiles;
}