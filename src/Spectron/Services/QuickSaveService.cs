using System;
using System.IO;
using Microsoft.Extensions.Logging;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.State;

namespace OldBit.Spectron.Services;

public class QuickSaveService(
    IEnvironmentService environmentService,
    IStateSnapshotStore snapshotStore,
    ILogger<QuickSaveService> logger)
{
    private const string QuickSaveFileName = "quick-save.spectron";

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
            var quickSaveFilePath = environmentService.GetAppDataPath(QuickSaveFileName);
            var snapshot = StateSnapshotManager.CreateSnapshot(emulator);
            snapshotStore.Save(quickSaveFilePath, snapshot);

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
            var quickSaveFilePath = environmentService.GetAppDataPath(QuickSaveFileName);

            return snapshotStore.Load(quickSaveFilePath);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while quick load file");
        }

        return null;
    }
}