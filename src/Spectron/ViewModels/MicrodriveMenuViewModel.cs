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

    public Dictionary<MicrodriveId, Observable<string>> EjectCommandHeadings { get; } = new();

    public MicrodriveMenuViewModel(MicrodriveManager microdriveManager)
    {
        foreach (var drive in Enum.GetValues<MicrodriveId>())
        {
            EjectCommandHeadings.Add(drive, new Observable<string>("Eject"));
        }

        _microdriveManager = microdriveManager;

        NewCommand = new RelayCommand<MicrodriveId>(execute: New);
        InsertCommand = new AsyncRelayCommand<MicrodriveId>(execute: Insert);
        SaveCommand = new AsyncRelayCommand<MicrodriveId>(execute: Save, canExecute: CanExecute);
        EjectCommand = new AsyncRelayCommand<MicrodriveId>(execute: Eject, canExecute: CanExecute);
    }

    private void New(MicrodriveId driveId)
    {
        _microdriveManager.NewCartridge(driveId);

        EjectCommandHeadings[driveId].Value = "Eject 'New Cartridge'";
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
            EjectCommandHeadings[driveId].Value = $"Eject '{fileName}'";
        }
        catch (Exception ex)
        {
            await MessageDialogs.Error(ex.Message);
        }
    }

    private async Task Save(MicrodriveId driveId)
    {
        await Task.CompletedTask;
    }

    private async Task Eject(MicrodriveId driveId)
    {
        _microdriveManager.EjectCartridge(driveId);

        EjectCommandHeadings[driveId].Value = "Eject";

        await Task.CompletedTask;
    }

    private bool CanExecute(MicrodriveId driveId) =>
        _microdriveManager[driveId].IsCartridgeInserted;
}