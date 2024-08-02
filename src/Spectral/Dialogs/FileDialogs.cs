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
                TapeFileTypes.All,
                TapeFileTypes.Sna,
                TapeFileTypes.Szx,
                TapeFileTypes.Tap,
                TapeFileTypes.Tzx,
                TapeFileTypes.Z80
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
                TapeFileTypes.TapTzx,
                TapeFileTypes.Tap,
                TapeFileTypes.Tzx
            }
        });
    }
}