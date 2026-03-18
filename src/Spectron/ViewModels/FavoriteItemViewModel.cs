using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OldBit.Spectron.Dialogs;
using OldBit.Spectron.Emulation.Files;
using OldBit.Spectron.Settings;

namespace OldBit.Spectron.ViewModels;

public partial class FavoriteItemViewModel : ObservableValidator
{
    public ObservableCollection<FavoriteItemViewModel> Nodes { get; } = [];

    public FavoriteSettingsViewModel SettingsViewModel { get; set; } = new();

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    [Required]
    [CustomValidation(typeof(FavoriteItemViewModel), nameof(ValidatePath))]
    [NotifyDataErrorInfo]
    [NotifyPropertyChangedFor(nameof(IsTapeFile))]
    private string _path = string.Empty;

    [ObservableProperty]
    private bool _isFolder;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsTapeFile))]
    private bool _isFile;

    [ObservableProperty]
    private bool _isRoot;

    [ObservableProperty]
    private bool _isCutItem;

    public bool IsTapeFile
    {
        get
        {
            if (!IsFile)
            {
                return false;
            }

            var fileType = FileTypeResolver.FromPath(Path);

            return fileType is FileType.Tap or FileType.Tzx;
        }
    }

    public FavoriteItemViewModel Clone()
    {
        var copy = new FavoriteItemViewModel
        {
            Title = Title,
            Path = Path,
            IsFile = IsFile,
            IsRoot = IsRoot,
            IsFolder = IsFolder
        };

        if (IsFolder)
        {
            foreach (var node in Nodes)
            {
                copy.Nodes.Add(node.Clone());
            }
        }

        return copy;
    }

    [RelayCommand]
    private async Task GetFile()
    {
        var files = await FileDialogs.OpenEmulatorFileAsync();

        if (files.Count <= 0)
        {
            return;
        }

        var filePath = files[0].Path.LocalPath;

        var fileType = FileTypeResolver.FromPath(filePath);

        if (fileType == FileType.Unsupported)
        {
            await MessageDialogs.Warning($"Unsupported file type: {fileType}.");
            return;
        }

        Path = filePath;
    }

    public static ValidationResult? ValidatePath(string s, ValidationContext context)
    {
        var fileType = FileTypeResolver.FromPath(s);

        if (fileType == FileType.Unsupported)
        {
            return new ValidationResult("Unsupported file type.");
        }

        if (!System.IO.File.Exists(s))
        {
            return new ValidationResult("File does not exist.");
        }

        return ValidationResult.Success;;
    }

    public FavoriteProgram ToFavoriteProgram()
    {
        if (IsFolder)
        {
            return new FavoriteProgram
            {
                Title = Title,
                IsFolder = true
            };
        }

        return new FavoriteProgram
        {
            Title = Title,
            Path = Path,
            ComputerType = SettingsViewModel.ComputerType.Value
        };
    }
}