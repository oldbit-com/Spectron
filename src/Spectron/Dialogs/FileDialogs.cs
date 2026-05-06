using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace OldBit.Spectron.Dialogs;

public sealed class FileDialogs(IFileDialogProvider fileDialogProvider)
{
    public async Task<IReadOnlyList<IStorageFile>> OpenEmulatorFileAsync() =>
        await OpenFileAsync("Select File", [
            FilePickerType.All,
            FilePickerType.TapeFiles,
            FilePickerType.SnapshotFiles,
            FilePickerType.DiskFiles,
            FilePickerType.Spectron,
            FilePickerType.Sna,
            FilePickerType.Szx,
            FilePickerType.Tap,
            FilePickerType.Tzx,
            FilePickerType.Z80,
            FilePickerType.Rzx,
            FilePickerType.Zip,
            FilePickerType.Pok,
            FilePickerType.Mdr,
            FilePickerType.Trd,
            FilePickerType.Scl,
        ]);

    public async Task<IReadOnlyList<IStorageFile>> OpenTapeFileAsync() =>
        await OpenFileAsync("Select Tape File", [
            FilePickerType.TapeFiles,
            FilePickerType.Tap,
            FilePickerType.Tzx
        ]);

    public async Task<IReadOnlyList<IStorageFile>> OpenCustomRomFileAsync() =>
        await OpenFileAsync("Select Custom ROM File", [
            FilePickerType.Rom,
            FilePickerType.Bin,
            FilePickerType.Any,
        ]);

    public async Task<IReadOnlyList<IStorageFile>> OpenDiskImageFileAsync() =>
        await OpenFileAsync("Select Disk Image", [
            FilePickerType.Img,
            FilePickerType.Any,
        ]);

    public async Task<IReadOnlyList<IStorageFile>> OpenMicrodriveFileAsync() =>
        await OpenFileAsync("Select Microdrive File", [
            FilePickerType.Mdr,
            FilePickerType.Any,
        ]);

    public async Task<IReadOnlyList<IStorageFile>> OpenDiskFileAsync() =>
        await OpenFileAsync("Select Disk File", [
            FilePickerType.DiskFiles,
            FilePickerType.Trd,
            FilePickerType.Scl,
            FilePickerType.Any,
        ]);

    public async Task<IStorageFile?> SaveTapeFileAsync(string? suggestedFileName = null)
    {
        var options = new FilePickerSaveOptions
        {
            Title = "Save Tape File",
            DefaultExtension = ".tzx",
            SuggestedFileName = suggestedFileName,
            ShowOverwritePrompt = true,
            FileTypeChoices = [FilePickerType.Tzx, FilePickerType.Tap]
        };

        return await fileDialogProvider.SaveFilePickerAsync(options);
    }

    public async Task<IStorageFile?> SaveMicrodriveFileAsync(string? suggestedFileName = null)
    {
        var options = new FilePickerSaveOptions
        {
            Title = "Save Microdrive File",
            DefaultExtension = ".mdr",
            SuggestedFileName = suggestedFileName,
            ShowOverwritePrompt = true,
            FileTypeChoices = [FilePickerType.Mdr]
        };

        return await fileDialogProvider.SaveFilePickerAsync(options);
    }

    public async Task<IStorageFile?> SaveDiskFileAsync(string? suggestedFileName = null)
    {
        var options = new FilePickerSaveOptions
        {
            Title = "Save Disk File",
            DefaultExtension = ".trd",
            SuggestedFileName = suggestedFileName,
            ShowOverwritePrompt = true,
            FileTypeChoices = [FilePickerType.Trd, FilePickerType.Scl]
        };

        return await fileDialogProvider.SaveFilePickerAsync(options);
    }

    public async Task<IStorageFile?> SaveSnapshotFileAsync(string? suggestedFileName = null)
    {
        var options = new FilePickerSaveOptions
        {
            Title = "Save Snapshot File",
            DefaultExtension = ".spectron",
            SuggestedFileName = suggestedFileName,
            ShowOverwritePrompt = true,
            FileTypeChoices = [FilePickerType.Spectron, FilePickerType.Szx, FilePickerType.Z80, FilePickerType.Sna],
        };

        return await fileDialogProvider.SaveFilePickerAsync(options);
    }

    public async Task<IStorageFile?> SaveAudioFileAsync(string? suggestedFileName = null)
    {
        var options = new FilePickerSaveOptions
        {
            Title = "Save Audio File",
            DefaultExtension = ".wav",
            SuggestedFileName = suggestedFileName,
            ShowOverwritePrompt = true,
            FileTypeChoices = [FilePickerType.Wav],
        };

        return await fileDialogProvider.SaveFilePickerAsync(options);
    }

    public async Task<IStorageFile?> SaveVideoFileAsync(string? suggestedFileName = null)
    {
        var options = new FilePickerSaveOptions
        {
            Title = "Save Video File",
            DefaultExtension = ".mp4",
            SuggestedFileName = suggestedFileName,
            ShowOverwritePrompt = true,
            FileTypeChoices = [FilePickerType.Mp4],
        };

        return await fileDialogProvider.SaveFilePickerAsync(options);
    }

    public async Task<IStorageFile?> SaveImageAsync(string title, string? suggestedFileName = null)
    {
        var options = new FilePickerSaveOptions
        {
            Title = title,
            DefaultExtension = ".png",
            SuggestedFileName = suggestedFileName,
            ShowOverwritePrompt = true,
            FileTypeChoices = [FilePickerType.Png],
        };

        return await fileDialogProvider.SaveFilePickerAsync(options);
    }

    private async Task<IReadOnlyList<IStorageFile>> OpenFileAsync(string title, IReadOnlyList<FilePickerFileType> fileTypes)
    {
        var options = new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
            FileTypeFilter = fileTypes
        };

        return await fileDialogProvider.OpenFilePickerAsync(options);
    }
}