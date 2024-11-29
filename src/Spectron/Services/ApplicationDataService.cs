using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OldBit.Spectron.Extensions;

namespace OldBit.Spectron.Services;

public class ApplicationDataService(ILogger<ApplicationDataService> logger)
{
    private readonly ILogger _logger = logger;

    public async Task SaveAsync(object settings)
    {
        try
        {
            var filePath = GetSettingsFilePath(settings);

            if (!TryCreateDirectory(filePath))
            {
                return;
            }

            var json = JsonSerializer.Serialize(settings);

            await File.WriteAllTextAsync(filePath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save settings");
        }
    }

    public async Task<T> LoadAsync<T>() where T : new()
    {
        try
        {
            var filePath = GetSettingsFilePath(new T());

            if (!File.Exists(filePath))
            {
                return new T();
            }

            var json = await File.ReadAllTextAsync(filePath);

            return JsonSerializer.Deserialize<T>(json) ?? new T();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load settings");
        }

        return new T();
    }

    private static string GetSettingsFilePath(object settings)
    {
        var fileName = GetFileName(settings.GetType());

        return GetFilePath(fileName);
    }

    private static bool TryCreateDirectory(string filePath)
    {
        try
        {
            var directory = Path.GetDirectoryName(filePath);

            if (directory == null)
            {
                return false;
            }

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string GetFilePath(string fileName)
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        return Path.Join(appData, "OldBit", "Spectron", fileName);
    }

    private static string GetFileName(Type settingsType) => $"{settingsType.Name.ToKebabCase()}.json";
}