using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OldBit.Spectron.Services;
using OldBit.Spectron.Settings;

namespace OldBit.Spectron.ViewModels;

public partial class RecentFilesViewModel : ObservableObject
{
    private readonly RecentFilesService _recentFilesService;
    private RecentFilesSettings _recentFilesSettings = new();

    public string CurrentFileName
    {
        get => _recentFilesSettings.CurrentFileName;
        set => _recentFilesSettings.CurrentFileName = value;
    }

    public Func<string, Task>? OpenRecentFileAsync;

    public RecentFilesViewModel(RecentFilesService recentFilesService)
    {
        _recentFilesService = recentFilesService;
    }

    [RelayCommand]
    private async Task OpenRecentFile(string fileName)
    {
        if (OpenRecentFileAsync != null)
        {
            await OpenRecentFileAsync(fileName);
        }
    }

    public async Task LoadAsync() => _recentFilesSettings = await _recentFilesService.LoadAsync();

    public async Task SaveAsync() => await _recentFilesService.SaveAsync(_recentFilesSettings);

    public void Opening(ItemCollection items)
    {
        items.Clear();

        foreach (var file in _recentFilesSettings.Files)
        {
            var fileName = Path.GetFileName(file);

            items.Add(new MenuItem
            {
                Header = fileName,
                Command = OpenRecentFileCommand,
                CommandParameter = file
            });
        }
    }

    public void Add(string filePath)
    {
        _recentFilesSettings.Files.Remove(filePath);
        _recentFilesSettings.Files.Insert(0, filePath);
        _recentFilesSettings.CurrentFileName = Path.GetFileName(filePath);

        if (_recentFilesSettings.Files.Count > _recentFilesSettings.MaxRecentFiles)
        {
            _recentFilesSettings.Files.RemoveRange(
                _recentFilesSettings.MaxRecentFiles,
                _recentFilesSettings.Files.Count - _recentFilesSettings.MaxRecentFiles);
        }
    }

    public void Remove(string? filePath)
    {
        if (filePath is not null)
        {
            _recentFilesSettings.Files.Remove(filePath);
        }
    }
}