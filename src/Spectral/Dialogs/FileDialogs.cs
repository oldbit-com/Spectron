using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace OldBit.Spectral.Dialogs;

public static class FileDialogs
{
    public static Window MainWindow { get; set; } = null!;

    public static async Task<IReadOnlyList<IStorageFile>> OpenAnyFileAsync()
    {
        var topLevel = TopLevel.GetTopLevel(MainWindow);
        if (topLevel == null)
        {
            return Array.Empty<IStorageFile>();
        }

        return await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open File",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                FileTypes.All,
                FileTypes.Sna,
                FileTypes.Szx,
                FileTypes.Tap,
                FileTypes.Tzx,
                FileTypes.Z80
            }
        });
    }

    public static async Task<IReadOnlyList<IStorageFile>> OpenTapeFileAsync()
    {
        var topLevel = TopLevel.GetTopLevel(MainWindow);
        if (topLevel == null)
        {
            return Array.Empty<IStorageFile>();
        }

        return await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Tape File",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                FileTypes.TapeFiles,
                FileTypes.Tap,
                FileTypes.Tzx
            }
        });
    }

    public static async Task<IStorageFile?> SaveSnapshotFileAsync(string? suggestedFileName = null)
    {
        var topLevel = TopLevel.GetTopLevel(MainWindow);
        if (topLevel == null)
        {
            return null;
        }

        var documentsFolder = await topLevel.StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents);

        return await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Snapshot File",
            DefaultExtension = ".z80",
            SuggestedFileName = suggestedFileName,
            SuggestedStartLocation = documentsFolder,
            ShowOverwritePrompt = true,
            FileTypeChoices = new[]
            {
                FileTypes.Szx,
                FileTypes.Z80,
                FileTypes.Sna
            },
        });
    }
}