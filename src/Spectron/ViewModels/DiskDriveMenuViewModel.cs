using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OldBit.Spectron.Dialogs;
using OldBit.Spectron.Emulation.Devices.Beta128;
using OldBit.Spectron.Emulation.Devices.Beta128.Drive;
using OldBit.Spectron.Emulation.Devices.Beta128.Events;
using OldBit.Spectron.Messages;

namespace OldBit.Spectron.ViewModels;

public class DiskDriveMenuViewModel : ObservableObject
{
    private readonly DiskDriveManager _diskDriveManager;
    public ICommand NewCommand { get; }
    public ICommand InsertCommand { get; }
    public ICommand InsertDefaultDriveCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand EjectCommand { get; }
    public ICommand ViewCommand { get; }
    public ICommand ToggleWriteProtectCommand { get; }

    public Dictionary<DriveId, Observable<string>> EjectCommandHeadings { get; } = new();
    public Dictionary<DriveId, Observable<bool>> IsWriteProtected { get; } = new();

    public DiskDriveMenuViewModel(DiskDriveManager diskDriveManager)
    {
        foreach (var drive in Enum.GetValues<DriveId>())
        {
            EjectCommandHeadings.Add(drive, new Observable<string>("Eject"));
            IsWriteProtected.Add(drive, new Observable<bool>(false));
        }

        _diskDriveManager = diskDriveManager;
        _diskDriveManager.DiskChanged += OnDiskChanged;

        NewCommand = new RelayCommand<DriveId>(execute: New);
        InsertCommand = new AsyncRelayCommand<DriveId>(execute: Insert);
        InsertDefaultDriveCommand = new AsyncRelayCommand(execute: async() => await Insert(DriveId.DriveA));
        SaveCommand = new AsyncRelayCommand<DriveId>(execute: Save, canExecute: IsDiskInserted);
        EjectCommand = new AsyncRelayCommand<DriveId>(execute: Eject, canExecute: IsDiskInserted);
        ViewCommand = new RelayCommand<DriveId>(execute: View, canExecute: IsDiskInserted);
        ToggleWriteProtectCommand = new RelayCommand<DriveId>(execute: ToggleWriteProtect, canExecute: IsDiskInserted);
    }

    private void OnDiskChanged(DiskChangedEventArgs e)
    {
        var diskDrive = _diskDriveManager.Drives.GetValueOrDefault(e.DriveId);

        if (diskDrive == null)
        {
            return;
        }

        if (!diskDrive.IsDiskInserted)
        {
            EjectCommandHeadings[e.DriveId].Value = "Eject";
        }
        else if (diskDrive.DiskFile?.FilePath != null)
        {
            var fileName = Path.GetFileName(diskDrive.DiskFile.FilePath);

            EjectCommandHeadings[e.DriveId].Value = $"Eject '{fileName}'";
        }
        else
        {
            EjectCommandHeadings[e.DriveId].Value = "Eject 'New Disk'";
        }

        if (diskDrive.IsWriteProtected != IsWriteProtected[e.DriveId].Value)
        {
            ToggleWriteProtect(e.DriveId);
        }
    }

    private void New(DriveId driveId)
    {
        _diskDriveManager[driveId].NewDisk();
        IsWriteProtected[driveId].Value = false;
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

            _diskDriveManager[driveId].InsertDisk(files[0].Path.LocalPath);
        }
        catch (Exception ex)
        {
            await MessageDialogs.Error(ex.Message);
        }
    }

    private async Task Save(DriveId driveId)
    {
        var diskImage = _diskDriveManager[driveId].DiskFile;

        if (diskImage == null)
        {
            return;
        }

        try
        {
            var file = await FileDialogs.SaveDiskFileAsync(Path.GetFileNameWithoutExtension(diskImage.FilePath));

            if (file == null)
            {
                return;
            }

            await diskImage.SaveAsync(file.Path.LocalPath);

            var fileName = Path.GetFileName(file.Path.LocalPath);
            EjectCommandHeadings[driveId].Value = $"Eject '{fileName}'";
        }
        catch (Exception ex)
        {
            await MessageDialogs.Error(ex.Message);
        }
    }

    private async Task Eject(DriveId driveId)
    {
        _diskDriveManager[driveId].EjectDisk();
        IsWriteProtected[driveId].Value = false;

        await Task.CompletedTask;
    }

    private void View(DriveId driveId) =>
        WeakReferenceMessenger.Default.Send(new ShowDiskViewMessage(_diskDriveManager, driveId));

    private void ToggleWriteProtect(DriveId driveId)
    {
        IsWriteProtected[driveId].Value = !IsWriteProtected[driveId].Value;
        _diskDriveManager[driveId].IsWriteProtected = IsWriteProtected[driveId].Value;
    }

    private bool IsDiskInserted(DriveId driveId) =>
        _diskDriveManager[driveId].IsDiskInserted;
}