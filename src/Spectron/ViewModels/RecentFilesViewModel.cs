using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using OldBit.Spectron.Services;
using OldBit.Spectron.Settings;
using ReactiveUI;

namespace OldBit.Spectron.ViewModels;

public class RecentFilesViewModel : ViewModelBase
{
    private readonly RecentFilesService _recentFilesService;
    private readonly ReactiveCommand<string, Unit> _openRecentFileCommand;
    private RecentFilesSettings _recentFilesSettings = new();

    public Func<string, Task>? OpenRecentFileAsync;

    public RecentFilesViewModel(RecentFilesService recentFilesService)
    {
        _recentFilesService = recentFilesService;

        _openRecentFileCommand = ReactiveCommand.CreateFromTask<string, Unit>(async fileName =>
        {
            if (OpenRecentFileAsync != null)
            {
                await OpenRecentFileAsync(fileName);
            }

            return Unit.Default;
        });
    }

    public async Task LoadAsync() => _recentFilesSettings = await _recentFilesService.LoadAsync();

    public async Task SaveAsync() => await _recentFilesService.SaveAsync(_recentFilesSettings);

    public void Opening(IList<NativeMenuItemBase> items)
    {
        items.Clear();

        foreach (var file in _recentFilesSettings.Files)
        {
            items.Add(new NativeMenuItem
            {
                Header = file,
                Command = _openRecentFileCommand,
                CommandParameter = file
            });
        }
    }

    public void Opening(ItemCollection items)
    {
        items.Clear();

        foreach (var file in _recentFilesSettings.Files)
        {
            var fileName = Path.GetFileName(file);

            items.Add(new MenuItem
            {
                Header = fileName,
                Command = _openRecentFileCommand,
                CommandParameter = file
            });
        }
    }

    public void Add(string filePath)
    {
        _recentFilesSettings.Files.Remove(filePath);
        _recentFilesSettings.Files.Insert(0, filePath);

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