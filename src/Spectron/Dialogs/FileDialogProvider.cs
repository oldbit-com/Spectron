using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;

namespace OldBit.Spectron.Dialogs;

public interface IFileDialogProvider
{
    Task<IReadOnlyList<IStorageFile>> OpenFilePickerAsync(FilePickerOpenOptions options);
    Task<IStorageFile?> SaveFilePickerAsync(FilePickerSaveOptions options);
}

public class FileDialogProvider : IFileDialogProvider
{
    public async Task<IReadOnlyList<IStorageFile>> OpenFilePickerAsync(FilePickerOpenOptions options)
    {
        var topLevel = GetTopLevel();

        if (topLevel == null)
        {
            return [];
        }

        return await topLevel.StorageProvider.OpenFilePickerAsync(options);
    }

    public async Task<IStorageFile?> SaveFilePickerAsync(FilePickerSaveOptions options)
    {
        var topLevel = GetTopLevel();

        if (topLevel == null)
        {
            return null;
        }

        return await topLevel.StorageProvider.SaveFilePickerAsync(options);
    }

    private static TopLevel? GetTopLevel()
    {
        var lifetime = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;

        var window = lifetime?.Windows.FirstOrDefault(x => x.IsActive);

        return TopLevel.GetTopLevel(window);
    }
}