using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Data.Sqlite;
using OldBit.Spectron.Database;
using OldBit.Spectron.Dialogs;
using OldBit.Spectron.Services;
using OldBit.Spectron.Settings;

namespace OldBit.Spectron.ViewModels;

public partial class GameBrowserViewModel : ObservableObject
{
    private readonly ZxDbClient _zxDbClient;
    private readonly ZxDbSettings _zxDbSettings;
    private readonly IApplicationDataService _applicationDataService;
    private readonly FileDialogs _fileDialogs;

    private bool _isInitialized;

    public ObservableCollection<GameViewModel> Games { get; } = [];

    public IAsyncRelayCommand SearchCommand { get; }

    public IAsyncRelayCommand LocateDatabaseCommand { get; }

    [ObservableProperty]
    public partial GameViewModel? SelectedGame { get; set; }

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsDatabaseAvailable { get; private set; }

    [ObservableProperty]
    public partial string? DatabaseError { get; private set; }

    public GameBrowserViewModel(
        ZxDbClient zxDbClient,
        ZxDbSettings zxDbSettings,
        IApplicationDataService applicationDataService,
        FileDialogs fileDialogs)
    {
        _zxDbClient = zxDbClient;
        _zxDbSettings = zxDbSettings;
        _applicationDataService = applicationDataService;
        _fileDialogs = fileDialogs;

        SearchCommand = new AsyncRelayCommand(Search);
        LocateDatabaseCommand = new AsyncRelayCommand(LocateDatabaseAsync);
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized)
        {
            RefreshAvailability();
            return;
        }

        var settings = await _applicationDataService.LoadAsync<GameBrowserSettings>();

        _zxDbSettings.DatabasePath = settings.ZxDbPath;
        _isInitialized = true;

        RefreshAvailability();
    }

    internal async Task SetDatabasePathAsync(string databasePath)
    {
        if (!await ZxDbClient.IsValidDatabaseAsync(databasePath))
        {
            DatabaseError = "The selected file is not a valid ZXDB database.";
            return;
        }

        _zxDbSettings.DatabasePath = databasePath;
        await _applicationDataService.SaveAsync(new GameBrowserSettings { ZxDbPath = databasePath });

        Games.Clear();
        DatabaseError = null;
        RefreshAvailability();
    }

    private async Task LocateDatabaseAsync()
    {
        var files = await _fileDialogs.OpenZxDbFileAsync();

        if (files.Count == 0)
        {
            return;
        }

        var databasePath = files[0].TryGetLocalPath();

        if (databasePath == null)
        {
            DatabaseError = "The selected file is not a local file.";
            return;
        }

        await SetDatabasePathAsync(databasePath);
    }

    private async Task Search(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            return;
        }

        RefreshAvailability();

        if (!IsDatabaseAvailable)
        {
            return;
        }

        try
        {
            var results = await _zxDbClient.SearchByTitleAsync(SearchText, cancellationToken: cancellationToken);

            Games.Clear();

            foreach (var result in results)
            {
                Games.Add(new GameViewModel
                {
                    EntryId = result.Id,
                    Name = result.Title,
                    Publisher = result.Publisher ?? string.Empty,
                    Year = result.Year?.ToString() ?? string.Empty,
                    Genre = result.Genre ?? string.Empty,
                    Availability = result.Availability ?? string.Empty,
                });
            }
        }
        catch (Exception ex) when (ex is SqliteException or ZxDbNotConfiguredException)
        {
            DatabaseError = $"Unable to read ZXDB database: {ex.Message}";
            RefreshAvailability();
        }
    }

    private void RefreshAvailability() => IsDatabaseAvailable = _zxDbClient.IsAvailable;
}
