using System;
using System.IO;
using Microsoft.Extensions.Logging;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.State;

namespace OldBit.Spectron.Services;

public class QuickSaveService(ILogger<QuickSaveService> logger)
{
    private bool _isQuickSaveRequested;

    public void RequestQuickSave() => _isQuickSaveRequested = true;

    public bool QuickSaveIfRequested(Emulator? emulator)
    {
        if (!_isQuickSaveRequested)
        {
            return false;
        }

        _isQuickSaveRequested = false;

        if (emulator == null)
        {
            return false;
        }

        try
        {
            var quickSaveFilePath = GetQuickSaveFilePath();
            var snapshot = StateManager.CreateSnapshot(emulator);

            snapshot.Save(quickSaveFilePath);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while quick save file");
        }

        return false;
    }

    public StateSnapshot? QuickLoad()
    {
        try
        {
            var quickSaveFilePath = GetQuickSaveFilePath();

            return StateSnapshot.Load(quickSaveFilePath);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while quick load file");
        }

        return null;
    }

    private static string GetQuickSaveFilePath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return  Path.Join(appData, "OldBit", "Spectron", "quick-save.spectron");
    }
}