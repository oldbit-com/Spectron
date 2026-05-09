using System;
using System.IO;

namespace OldBit.Spectron.Services;

public class EnvironmentService : IEnvironmentService
{
    public string GetAppDataPath(string fileName)
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        return Path.Join(appData, "OldBit", "Spectron", fileName);
    }
}