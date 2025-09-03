using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace OldBit.Spectron.Dialogs;

public static class FileDialogs
{
    public static Window MainWindow { get; set; } = null!;

    public static async Task<IReadOnlyList<IStorageFile>> OpenEmulatorFileAsync() =>
        await OpenFileAsync("Select File",
        [
            FileTypes.All,
            FileTypes.Sna,
            FileTypes.Szx,
            FileTypes.Tap,
            FileTypes.Tzx,
            FileTypes.Z80,
            FileTypes.Zip,
            FileTypes.Pok,
        ]);

    public static async Task<IReadOnlyList<IStorageFile>> OpenTapeFileAsync() =>
        await OpenFileAsync("Select Tape File",
        [
            FileTypes.TapeFiles,
            FileTypes.Tap,
            FileTypes.Tzx
        ]);

    public static async Task<IReadOnlyList<IStorageFile>> OpenCustomRomFileAsync() =>
        await OpenFileAsync("Select Custom ROM File",
        [
            FileTypes.Rom,
            FileTypes.Bin,
            FileTypes.Any,
        ]);

    public static async Task<IReadOnlyList<IStorageFile>> OpenDiskImageFileAsync() =>
        await OpenFileAsync("Select Disk Image",
        [
            FileTypes.Img,
            FileTypes.Any,
        ]);

    public static async Task<IReadOnlyList<IStorageFile>> OpenMicrodriveFileAsync() =>
        await OpenFileAsync("Select Microdrive File",
        [
            FileTypes.Mdr,
            FileTypes.Any,
        ]);

    public static async Task<IStorageFile?> SaveTapeFileAsync(string? suggestedFileName = null)
    {
        var topLevel = TopLevel.GetTopLevel(MainWindow);

        if (topLevel == null)
        {
            return null;
        }

        return await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Tape File",
            DefaultExtension = ".tzx",
            SuggestedFileName = suggestedFileName,
            ShowOverwritePrompt = true,
            FileTypeChoices = [FileTypes.Tzx, FileTypes.Tap]
        });
    }

    public static async Task<IStorageFile?> SaveMicrodriveFileAsync(string? suggestedFileName = null)
    {
        var topLevel = TopLevel.GetTopLevel(MainWindow);

        if (topLevel == null)
        {
            return null;
        }

        return await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Microdrive File",
            DefaultExtension = ".mdr",
            SuggestedFileName = suggestedFileName,
            ShowOverwritePrompt = true,
            FileTypeChoices = [FileTypes.Mdr]
        });
    }

    public static async Task<IStorageFile?> SaveSnapshotFileAsync(string? suggestedFileName = null)
    {
        var topLevel = TopLevel.GetTopLevel(MainWindow);

        if (topLevel == null)
        {
            return null;
        }

        return await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Snapshot File",
            DefaultExtension = ".szx",
            SuggestedFileName = suggestedFileName,
            ShowOverwritePrompt = true,
            FileTypeChoices = [FileTypes.Szx, FileTypes.Z80, FileTypes.Sna],
        });
    }

    public static async Task<IStorageFile?> SaveAudioFileAsync(string? suggestedFileName = null)
    {
        var topLevel = TopLevel.GetTopLevel(MainWindow);

        if (topLevel == null)
        {
            return null;
        }

        return await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Audio File",
            DefaultExtension = ".wav",
            SuggestedFileName = suggestedFileName,
            ShowOverwritePrompt = true,
            FileTypeChoices = [FileTypes.Wav],
        });
    }

    public static async Task<IStorageFile?> SaveVideoFileAsync(string? suggestedFileName = null)
    {
        var topLevel = TopLevel.GetTopLevel(MainWindow);

        if (topLevel == null)
        {
            return null;
        }

        return await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Video File",
            DefaultExtension = ".mp4",
            SuggestedFileName = suggestedFileName,
            ShowOverwritePrompt = true,
            FileTypeChoices = [FileTypes.Mp4],
        });
    }

    public static async Task<IStorageFile?> SaveImageAsync(string title, Control? owner, string? suggestedFileName = null)
    {
        var topLevel = owner != null ? TopLevel.GetTopLevel(owner) : TopLevel.GetTopLevel(MainWindow);

        if (topLevel == null)
        {
            return null;
        }

        return await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = title,
            DefaultExtension = ".png",
            SuggestedFileName = suggestedFileName,
            ShowOverwritePrompt = true,
            FileTypeChoices = [FileTypes.Png],
        });
    }

    private static async Task<IReadOnlyList<IStorageFile>> OpenFileAsync(string title, IReadOnlyList<FilePickerFileType> fileTypes)
    {
        var topLevel = TopLevel.GetTopLevel(MainWindow);

        if (topLevel == null)
        {
            return [];
        }

        return await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
            FileTypeFilter = fileTypes
        });
    }
}