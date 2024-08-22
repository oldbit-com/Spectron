using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace OldBit.Spectron.Preferences;

public static class SettingsManager
{
    public static async Task SaveAsync(object settings)
    {
        var fileName = GetSettingsFileName(settings);
        var filePath = GetSettingsFilePath(fileName);

        var directory = Path.GetDirectoryName(filePath);
        if (directory == null)
        {
            return;
        }

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(settings);
        await File.WriteAllTextAsync(filePath, json);
    }

    public static async Task<T> LoadAsync<T>() where T : new()
    {
        var settings = new T();
        var fileName = GetSettingsFileName(settings);
        var filePath = GetSettingsFilePath(fileName);

        if (!File.Exists(filePath))
        {
            return settings;
        }

        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<T>(json) ?? settings;
    }

    private static string GetSettingsFilePath(string fileName)
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Join(appData, "OldBit", "Spectron", fileName);
    }

    private static string GetSettingsFileName(object settings)
    {
        return settings switch
        {
            RecentFilesSettings _ => "recent_files.json",
            DefaultSettings _ => "defaults.json",
            _ => throw new ArgumentException("Unknown settings type")
        };
    }
}