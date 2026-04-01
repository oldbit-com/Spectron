using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OldBit.Spectron.Dialogs;
using OldBit.Spectron.Emulation.Devices.Interface1.Microdrives;
using OldBit.Spectron.Emulation.Devices.Interface1.Microdrives.Events;

namespace OldBit.Spectron.ViewModels;

public partial class MicrodriveMenuViewModel : ObservableObject
{
    private readonly MicrodriveManager _microdriveManager;

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
        _microdriveManager.CartridgeChanged += OnCartridgeChanged;
    }

    private void OnCartridgeChanged(CartridgeChangedEventArgs e)
    {
        var microdrive = _microdriveManager.Microdrives.GetValueOrDefault(e.DriveId);

        if (microdrive == null)
        {
            return;
        }

        if (!microdrive.IsCartridgeInserted)
        {
            EjectCommandHeadings[e.DriveId].Value = "Eject";
        }
        else if (microdrive.Cartridge?.FilePath != null)
        {
            var fileName = Path.GetFileName(microdrive.Cartridge.FilePath);

            EjectCommandHeadings[e.DriveId].Value = $"Eject '{fileName}'";
        }
        else
        {
            EjectCommandHeadings[e.DriveId].Value = "Eject 'New Cartridge'";
        }

        NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void New(MicrodriveId driveId)
    {
        _microdriveManager[driveId].NewCartridge();
        IsWriteProtected[driveId].Value = false;

        NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private async Task Insert(MicrodriveId driveId)
    {
        try
        {
            var files = await FileDialogs.OpenMicrodriveFileAsync();

            if (files.Count == 0)
            {
                return;
            }

            _microdriveManager[driveId].InsertCartridge(files[0].Path.LocalPath);
            IsWriteProtected[driveId].Value = _microdriveManager[driveId].IsCartridgeWriteProtected;

            NotifyCanExecuteChanged();
        }
        catch (Exception ex)
        {
            await MessageDialogs.Error(ex.Message);
        }
    }

    [RelayCommand(CanExecute = nameof(IsCartridgeInserted))]
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

    [RelayCommand(CanExecute = nameof(IsCartridgeInserted))]
    private async Task Eject(MicrodriveId driveId)
    {
        _microdriveManager[driveId].EjectCartridge();
        IsWriteProtected[driveId].Value = false;

        NotifyCanExecuteChanged();

        await Task.CompletedTask;
    }

    [RelayCommand(CanExecute = nameof(IsCartridgeInserted))]
    private void ToggleWriteProtect(MicrodriveId driveId)
    {
        IsWriteProtected[driveId].Value = !IsWriteProtected[driveId].Value;
        _microdriveManager[driveId].IsCartridgeWriteProtected = IsWriteProtected[driveId].Value;
    }

    private void NotifyCanExecuteChanged()
    {
        SaveCommand.NotifyCanExecuteChanged();
        EjectCommand.NotifyCanExecuteChanged();
        ToggleWriteProtectCommand.NotifyCanExecuteChanged();
    }

    private bool IsCartridgeInserted(MicrodriveId driveId) => _microdriveManager[driveId].IsCartridgeInserted;
}