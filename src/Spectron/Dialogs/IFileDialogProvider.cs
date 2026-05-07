using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace OldBit.Spectron.Dialogs;

public interface IFileDialogProvider
{
    Task<IReadOnlyList<IStorageFile>> OpenFilePickerAsync(FilePickerOpenOptions options);
    Task<IStorageFile?> SaveFilePickerAsync(FilePickerSaveOptions options);
}