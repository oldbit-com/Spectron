using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OldBit.Spectron.Dialogs;
using OldBit.Spectron.Emulation.Devices.Beta128.Drive;

namespace OldBit.Spectron.ViewModels;

public class DiskDriveMenuViewModel : ObservableObject
{
    public ICommand InsertCommand { get; }
    public ICommand EjectCommand { get; }
    public ICommand ToggleWriteProtectCommand { get; }

    public Dictionary<DriveId, Observable<string>> EjectCommandHeadings { get; } = new();
    public Dictionary<DriveId, Observable<bool>> IsWriteProtected { get; } = new();

    public DiskDriveMenuViewModel()
    {
        foreach (var drive in Enum.GetValues<DriveId>())
        {
            EjectCommandHeadings.Add(drive, new Observable<string>("Eject"));
            IsWriteProtected.Add(drive, new Observable<bool>(false));
        }

        InsertCommand = new AsyncRelayCommand<DriveId>(execute: Insert);
        EjectCommand = new AsyncRelayCommand<DriveId>(execute: Eject, canExecute: IsDiskInserted);
        ToggleWriteProtectCommand = new RelayCommand<DriveId>(execute: ToggleWriteProtect, canExecute: IsDiskInserted);
    }

    private async Task Insert(DriveId driveId)
    {
        try
        {
            var files = await FileDialogs.OpenDiskFileAsync();

            if (files.Count == 0)
            {
                return;
            }

            // _microdriveManager[driveId].InsertCartridge(files[0].Path.LocalPath);
            // IsWriteProtected[driveId].Value = _microdriveManager[driveId].IsCartridgeWriteProtected;
        }
        catch (Exception ex)
        {
            await MessageDialogs.Error(ex.Message);
        }
    }

    private async Task Eject(DriveId driveId)
    {
        // _microdriveManager[driveId].EjectCartridge();
        // IsWriteProtected[driveId].Value = false;

        await Task.CompletedTask;
    }

    private void ToggleWriteProtect(DriveId driveId)
    {
        IsWriteProtected[driveId].Value = !IsWriteProtected[driveId].Value;
        //_microdriveManager[driveId].IsCartridgeWriteProtected = IsWriteProtected[driveId].Value;
    }

    private bool IsDiskInserted(DriveId driveId) => false;
        //_microdriveManager[driveId].IsCartridgeInserted;
}