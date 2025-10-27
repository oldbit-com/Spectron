using System;
using System.Collections.ObjectModel;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using OldBit.Spectron.Emulation.Devices.Beta128;
using OldBit.Spectron.Emulation.Devices.Beta128.Drive;
using OldBit.Spectron.Emulation.Devices.Beta128.Floppy;

namespace OldBit.Spectron.ViewModels;

public partial class DiskViewModel : ObservableObject
{
    public record DiskFile(string FileName, string FileType, string Length, string Address);

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _numberOfTracks = string.Empty;

    [ObservableProperty]
    private string _numberOfSides = string.Empty;

    [ObservableProperty]
    private string _numberOfFiles = string.Empty;

    [ObservableProperty]
    private string _numberOfDeletedFiles = string.Empty;

    public ObservableCollection<DiskFile> Files { get; } = [];

    public DiskViewModel(DiskDriveManager diskDriveManager, DriveId driveId)
    {
        var drive = diskDriveManager.Drives[driveId];

        if (drive.DiskFile?.Floppy == null)
        {
            return;
        }

        PopulateLabels(drive.DiskFile.Floppy);
        PopulateDirectory(drive.DiskFile.Floppy);
    }

    private void PopulateLabels(FloppyDisk floppyDisk)
    {
        Name = floppyDisk.Label;
        NumberOfTracks = floppyDisk.TotalCylinders.ToString();
        NumberOfSides= floppyDisk.TotalSides.ToString();
    }

    private void PopulateDirectory(FloppyDisk floppyDisk)
    {
        var isLastFile = false;
        var numberOfDeletedFiles = 0;
        var numberOfFiles = 0;

        for (var sectorNo = 1; sectorNo <= 8; sectorNo++)
        {
            var sector = floppyDisk.GetSector(0, 0, sectorNo);
            var data = sector.GetData();

            for (var i = 0; i < 16; i++)
            {
                var dir = data[(i * 16)..((i + 1) * 16)];

                if (dir[0] == 0x00)
                {
                    isLastFile = true;
                    break;
                }

                if (dir[0] == 0x01)
                {
                    numberOfDeletedFiles += 1;
                }
                else
                {
                    numberOfFiles += 1;
                    Files.Add(ParseDirectoryEntry(dir));
                }
            }

            if (isLastFile)
            {
                break;
            }
        }

        NumberOfFiles = numberOfFiles.ToString();
        NumberOfDeletedFiles = numberOfDeletedFiles.ToString();
    }

    private static DiskFile ParseDirectoryEntry(ReadOnlySpan<byte> entry)
    {
        var fileName = Encoding.ASCII.GetString(entry[..0x08]);
        var fileTypeCode = entry[0x08];
        var fileType = "?";
        var length = entry[0x0D];

        var sectorNo = entry[0x0E];
        var trackNo = entry[0x0F] >> 1;
        var sideNo = entry[0x0F] & 0x01;

        var address = $"T{trackNo}H{sideNo}S{sectorNo}";

        if (fileTypeCode is > 0x20 and < 0x7F)
        {
            fileType = Encoding.ASCII.GetString([fileTypeCode]);
        }

        return new DiskFile(fileName, fileType, length.ToString(), address);
    }
}