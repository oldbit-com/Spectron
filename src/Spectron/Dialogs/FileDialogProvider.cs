using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace OldBit.Spectron.Dialogs;

public interface IFileDialogProvider
{
    Task<IReadOnlyList<IStorageFile>> OpenFilePickerAsync(Visual? owner, FilePickerOpenOptions options);
    Task<IStorageFile?> SaveFilePickerAsync(Visual? owner, FilePickerSaveOptions options);
}

public class FileDialogProvider : IFileDialogProvider
{
    public async Task<IReadOnlyList<IStorageFile>> OpenFilePickerAsync(Visual? owner, FilePickerOpenOptions options)
    {
        var topLevel = TopLevel.GetTopLevel(owner);

        if (topLevel == null)
        {
            return [];
        }

        return await topLevel.StorageProvider.OpenFilePickerAsync(options);
    }

    public async Task<IStorageFile?> SaveFilePickerAsync(Visual? owner, FilePickerSaveOptions options)
    {
        var topLevel = TopLevel.GetTopLevel(owner);

        if (topLevel == null)
        {
            return null;
        }

        return await topLevel.StorageProvider.SaveFilePickerAsync(options);
    }
}