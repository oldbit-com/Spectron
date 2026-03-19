using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using OldBit.Spectron.Dialogs;
using OldBit.Spectron.Emulation.Devices.Beta128.Drive;
using OldBit.Spectron.Emulation.Devices.Interface1.Microdrives;
using OldBit.Spectron.Emulation.Extensions;
using OldBit.Spectron.Emulation.Files;
using OldBit.Spectron.Emulation.Snapshot;
using OldBit.Spectron.Files.Pok;
using OldBit.Spectron.Messages;
using OldBit.Spectron.Settings;

namespace OldBit.Spectron.ViewModels;

partial class MainWindowViewModel
{
    private async Task HandleLoadFileAsync() => await HandleLoadFileAsync(null);

    private async Task HandleLoadFileAsync(string? filePath, FavoriteProgram? favorite = null)
    {
        Stream? stream = null;
        var shouldResume = !IsPaused;

        try
        {
            Pause();

            if (filePath == null)
            {
                var files = await FileDialogs.OpenEmulatorFileAsync();
                if (files.Count <= 0)
                {
                    return;
                }

                filePath = files[0].Path.LocalPath;
            }

            var fileType = FileTypeResolver.FromPath(filePath);
            if (fileType == FileType.Unsupported)
            {
                await MessageDialogs.Warning($"Unsupported file type: {fileType}.");
                return;
            }

            (stream, fileType) = await LoadFileAsync(filePath, fileType);
            if (stream == null)
            {
                return;
            }

            switch (fileType)
            {
                case FileType.Pok:
                    LoadPokeFile(stream);
                    return;

                case FileType.Mdr:
                    LoadMicrodriveFile(filePath, stream);
                    RecentFilesViewModel.Add(filePath);
                    return;

                case FileType.Trd:
                case FileType.Scl:
                    LoadDiskFile(filePath, stream);
                    RecentFilesViewModel.Add(filePath);
                    return;

                default:
                    if (_preferences.IsAutoLoadPokeFilesEnabled)
                    {
                        TryAutoLoadPokeFile(filePath, fileType);
                    }

                    break;
            }

            if (CreateEmulator(stream, fileType, favorite))
            {
                RecentFilesViewModel.Add(filePath);
                Title = $"{DefaultTitle} [{RecentFilesViewModel.CurrentFileName}]";
            }
        }
        catch (Exception ex)
        {
            await MessageDialogs.Error(ex.Message);
            RecentFilesViewModel.Remove(filePath);
        }
        finally
        {
            stream?.Close();

            if (shouldResume)
            {
                Resume();
            }
        }
    }

    private async Task OpenFavorite(FavoriteProgram favorite)
    {
        if (!string.IsNullOrWhiteSpace(favorite.Path))
        {
            await HandleLoadFileAsync(favorite.Path, favorite);
        }
    }

    private static async Task<(Stream? Stream, FileType FileType)> LoadFileAsync(string filePath, FileType fileType)
    {
        Stream? stream = null;

        if (fileType.IsArchive())
        {
            var archive = new ZipArchiveReader(filePath);
            var files = archive.GetSupportedFiles();

            switch (files.Count)
            {
                case 0:
                    await MessageDialogs.Warning("No matching files found in the archive.");
                    return (null, fileType);

                case 1:
                    fileType = files[0].FileType;
                    stream = archive.GetFile(files[0].Name);
                    break;

                default:
                {
                    var selectedFile = await WeakReferenceMessenger.Default.Send(new ShowSelectArchiveFileViewMessage(files));

                    if (selectedFile != null)
                    {
                        fileType = selectedFile.FileType;
                        stream = archive.GetFile(selectedFile.Name);
                    }
                    break;
                }
            }
        }
        else
        {
            stream = File.OpenRead(filePath);
        }

        return (stream, fileType);
    }

    private void LoadPokeFile(Stream stream)
    {
        _pokeFile = PokeFile.Load(stream);
        OpenTrainersWindow();
    }

    private void LoadMicrodriveFile(string filePath, Stream stream)
    {
        Emulator?.Interface1.Enable();
        Emulator?.Beta128.Disable();
        Emulator?.MicrodriveManager.Microdrives[MicrodriveId.Drive1].InsertCartridge(filePath, stream);
    }

    private void LoadDiskFile(string filePath, Stream stream)
    {
        Emulator?.Beta128.Enable();
        Emulator?.Interface1.Disable();
        Emulator?.DiskDriveManager.Drives[DriveId.DriveA].InsertDisk(filePath, stream);
    }

    private void TryAutoLoadPokeFile(string filePath, FileType fileType)
    {
        try
        {
            var pokeFilePath = Path.ChangeExtension(filePath, ".pok");

            if (!File.Exists(pokeFilePath))
            {
                return;
            }

            _pokeFile = PokeFile.Load(pokeFilePath);
        }
        catch
        {
            // Ignore errors
        }
    }

    private async Task HandleSaveFileAsync()
    {
        var shouldResume = !IsPaused;

        try
        {
            Pause();

            var suggestedFileName = GetSuggestedFileName();
            var file = await FileDialogs.SaveSnapshotFileAsync(suggestedFileName);

            if (file != null && Emulator != null)
            {
                SnapshotManager.Save(file.Path.LocalPath, Emulator);
            }
        }
        catch (Exception ex)
        {
            await MessageDialogs.Error(ex.Message);
        }
        finally
        {
            if (shouldResume)
            {
                Resume();
            }
        }
    }

    private string? GetSuggestedFileName() => string.IsNullOrWhiteSpace(RecentFilesViewModel.CurrentFileName)
        ? null
        : Path.GetFileNameWithoutExtension(RecentFilesViewModel.CurrentFileName);

    private void HandleQuickSave()
    {
        if (Emulator == null)
        {
            return;
        }

        _quickSaveService.RequestQuickSave();
    }

    private void HandleQuickLoad()
    {
        var snapshot = _quickSaveService.QuickLoad();

        if (snapshot != null)
        {
            CreateEmulator(snapshot);
        }
    }
}