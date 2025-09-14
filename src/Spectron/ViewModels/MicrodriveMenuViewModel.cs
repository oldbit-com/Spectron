using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OldBit.Spectron.Dialogs;
using OldBit.Spectron.Emulation.Devices.Interface1;
using OldBit.Spectron.Emulation.Devices.Interface1.Microdrive;

namespace OldBit.Spectron.ViewModels;

public class MicrodriveMenuViewModel : ObservableObject
{
    private readonly MicrodriveManager _microdriveManager;

    public ICommand NewCommand { get; }
    public ICommand InsertCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand EjectCommand { get; }
    public ICommand ToggleWriteProtectCommand { get; }

    public Dictionary<MicrodriveId, Observable<string>> EjectCommandHeadings { get; } = new();
    public Dictionary<MicrodriveId, Observable<bool>> IsWriteProtected { get; } = new();

    public MicrodriveMenuViewModel(MicrodriveManager microdriveManager)
    {
        foreach (var drive in Enum.GetValues<MicrodriveId>())
        {
            EjectCommandHeadings.Add(drive, new Observable<string>("Eject"));
            IsWriteProtected.Add(drive, new Observable<bool>(false));
        }

        _microdriveManager = microdriveManager;

        _microdriveManager.StateChanged += OnDriveStateChanged;

        NewCommand = new RelayCommand<MicrodriveId>(execute: New);
        InsertCommand = new AsyncRelayCommand<MicrodriveId>(execute: Insert);
        SaveCommand = new AsyncRelayCommand<MicrodriveId>(execute: Save, canExecute: IsCartridgeInserted);
        EjectCommand = new AsyncRelayCommand<MicrodriveId>(execute: Eject, canExecute: IsCartridgeInserted);
        ToggleWriteProtectCommand = new RelayCommand<MicrodriveId>(execute: ToggleWriteProtect, canExecute: IsCartridgeInserted);
    }

    private void OnDriveStateChanged(MicrodriveStateChangedEventArgs e)
    {
        var microdrive = _microdriveManager.Microdrives.GetValueOrDefault(e.MicrodriveId);

        if (microdrive == null)
        {
            return;
        }

        if (microdrive.Cartridge?.FilePath != null)
        {
            var fileName = Path.GetFileName(microdrive.Cartridge.FilePath);

            EjectCommandHeadings[e.MicrodriveId].Value = $"Eject '{fileName}'";
        }
        else
        {
            EjectCommandHeadings[e.MicrodriveId].Value = "Eject 'New Cartridge'";
        }
    }

    private void New(MicrodriveId driveId)
    {
        _microdriveManager.NewCartridge(driveId);

       // EjectCommandHeadings[driveId].Value = "Eject 'New Cartridge'";
        IsWriteProtected[driveId].Value = false;
    }

    private async Task Insert(MicrodriveId driveId)
    {
        try
        {
            var files = await FileDialogs.OpenMicrodriveFileAsync();

            if (files.Count == 0)
            {
                return;
            }

            _microdriveManager.InsertCartridge(driveId, files[0].Path.LocalPath);

            var fileName = Path.GetFileName(files[0].Path.LocalPath);

            //EjectCommandHeadings[driveId].Value = $"Eject '{fileName}'";
            IsWriteProtected[driveId].Value = _microdriveManager[driveId].IsCartridgeWriteProtected;
        }
        catch (Exception ex)
        {
            await MessageDialogs.Error(ex.Message);
        }
    }

    private async Task Save(MicrodriveId driveId)
    {
        var cartridge = _microdriveManager[driveId].Cartridge;

        if (cartridge == null)
        {
            return;
        }

        try
        {
            var file = await FileDialogs.SaveMicrodriveFileAsync(Path.GetFileNameWithoutExtension(cartridge.FilePath));

            if (file == null)
            {
                return;
            }

            await cartridge.SaveAsync(file.Path.LocalPath);

            var fileName = Path.GetFileName(file.Path.LocalPath);
            EjectCommandHeadings[driveId].Value = $"Eject '{fileName}'";
        }
        catch (Exception ex)
        {
            await MessageDialogs.Error(ex.Message);
        }
    }

    private async Task Eject(MicrodriveId driveId)
    {
        _microdriveManager.EjectCartridge(driveId);

        EjectCommandHeadings[driveId].Value = "Eject";

        await Task.CompletedTask;
    }

    private void ToggleWriteProtect(MicrodriveId driveId)
    {
        IsWriteProtected[driveId].Value = !IsWriteProtected[driveId].Value;
        _microdriveManager[driveId].IsCartridgeWriteProtected = IsWriteProtected[driveId].Value;
    }

    private bool IsCartridgeInserted(MicrodriveId driveId) =>
        _microdriveManager[driveId].IsCartridgeInserted;
}