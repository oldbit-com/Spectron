using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using OldBit.Spectron.Emulation.Devices.Beta128.Drive;
using OldBit.Spectron.Emulation.Devices.Interface1.Microdrives;
using OldBit.Spectron.Emulation.Extensions;
using OldBit.Spectron.Emulation.Files;
using OldBit.Spectron.Files.Pok;
using OldBit.Spectron.Messages;
using OldBit.Spectron.Settings;

namespace OldBit.Spectron.ViewModels;

partial class MainViewModel
{
    private async Task HandleLoadFileAsync() => await HandleLoadFileAsync(null);

    private async Task HandleLoadFileAsync(string? filePath, FavoriteProgram? favorite = null)
    {
        var shouldResume = !IsPaused;

        try
        {
            Pause();

            if (filePath == null)
            {
                var files = await _fileDialogs.OpenEmulatorFileAsync();
                if (files.Count <= 0)
                {
                    return;
                }

                filePath = files[0].Path.LocalPath;
            }

            var fileType = FileTypeResolver.FromPath(filePath);
            if (fileType == FileType.Unsupported)
            {
                await _messageDialogs.Warning($"Unsupported file type: {Path.GetExtension(filePath)}.");
                return;
            }

            await LoadFileCoreAsync(filePath, fileType, favorite, SelectArchiveEntryAsync);
        }
        catch (Exception ex)
        {
            await _messageDialogs.Error(ex.Message);
            RecentFilesViewModel.Remove(filePath);
        }
        finally
        {
            if (shouldResume)
            {
                Resume();
            }
        }
    }

    /// <summary>
    /// Loads a file without showing any dialogs. Errors are thrown to the caller.
    /// </summary>
    /// <param name="filePath">The path to the file.</param>
    /// <param name="fileType">The type of the file.</param>
    /// <param name="favorite">The favorite program settings to apply, if any.</param>
    /// <param name="selectArchiveEntry">Chooses which supported file to load from a zip archive.
    /// The list can be empty. Returns null to cancel the load.</param>
    /// <returns>The type of the loaded file (for an archive, the type of the selected entry) or null if nothing was loaded.</returns>
    private async Task<FileType?> LoadFileCoreAsync(
        string filePath,
        FileType fileType,
        FavoriteProgram? favorite,
        Func<List<ArchiveEntry>, Task<ArchiveEntry?>> selectArchiveEntry)
    {
        if (fileType == FileType.Unsupported)
        {
            throw new NotSupportedException($"Unsupported file type: {Path.GetExtension(filePath)}.");
        }

        ZipArchiveReader? archive = null;
        Stream? stream = null;

        try
        {
            if (fileType.IsArchive())
            {
                archive = new ZipArchiveReader(filePath);

                var entry = await selectArchiveEntry(archive.GetSupportedFiles());
                if (entry == null)
                {
                    return null;
                }

                fileType = entry.FileType;
                stream = archive.GetFile(entry.Name) ??
                         throw new FileNotFoundException($"File {entry.Name} not found in the archive.");
            }
            else
            {
                stream = File.OpenRead(filePath);
            }

            switch (fileType)
            {
                case FileType.Pok:
                    LoadPokeFile(stream);
                    return fileType;

                case FileType.Mdr:
                    LoadMicrodriveFile(filePath, stream);
                    RecentFilesViewModel.Add(filePath);
                    return fileType;

                case FileType.Trd:
                case FileType.Scl:
                    LoadDiskFile(filePath, stream);
                    RecentFilesViewModel.Add(filePath);
                    return fileType;

                default:
                    if (_preferences.IsAutoLoadPokeFilesEnabled)
                    {
                        TryAutoLoadPokeFile(filePath);
                    }

                    break;
            }

            if (!CreateEmulator(stream, fileType, favorite))
            {
                return null;
            }

            RecentFilesViewModel.Add(filePath);
            Title = $"{DefaultTitle} [{RecentFilesViewModel.CurrentFileName}]";

            return fileType;
        }
        finally
        {
            stream?.Dispose();
            archive?.Dispose();
        }
    }

    private async Task<ArchiveEntry?> SelectArchiveEntryAsync(List<ArchiveEntry> entries)
    {
        switch (entries.Count)
        {
            case 0:
                await _messageDialogs.Warning("No matching files found in the archive.");
                return null;

            case 1:
                return entries[0];

            default:
                return await WeakReferenceMessenger.Default.Send(new ShowSelectArchiveFileViewMessage(entries));
        }
    }

    private async Task HandleBrowseGamesAsync()
    {
        await _gameBrowserViewModel.InitializeAsync();

        WeakReferenceMessenger.Default.Send(new ShowGameBrowserViewMessage(_gameBrowserViewModel));
    }

    private async Task OpenFavorite(FavoriteProgram favorite)
    {
        if (!string.IsNullOrWhiteSpace(favorite.Path))
        {
            await HandleLoadFileAsync(favorite.Path, favorite);
        }
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

    private void TryAutoLoadPokeFile(string filePath)
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
            var file = await _fileDialogs.SaveSnapshotFileAsync(suggestedFileName);

            if (file != null && Emulator != null)
            {
                _snapshotManager.Save(file.Path.LocalPath, Emulator);
            }
        }
        catch (Exception ex)
        {
            await _messageDialogs.Error(ex.Message);
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