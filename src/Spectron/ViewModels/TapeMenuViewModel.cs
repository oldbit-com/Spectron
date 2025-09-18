using System.IO;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OldBit.Spectron.Dialogs;
using OldBit.Spectron.Emulation.Files;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Spectron.Files.Extensions;
using OldBit.Spectron.Messages;
using FileTypes = OldBit.Spectron.Emulation.Files.FileTypes;

namespace OldBit.Spectron.ViewModels;

public partial class TapeMenuViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _canStop;

    [ObservableProperty]
    private bool _canPlay;

    [ObservableProperty]
    private bool _canRewind;

    [ObservableProperty]
    private bool _canEject;

    private readonly TapeManager _tapeManager;
    private readonly RecentFilesViewModel _recentFilesViewModel;

    public TapeMenuViewModel(TapeManager tapeManager, RecentFilesViewModel recentFilesViewModel)
    {
        _tapeManager = tapeManager;
        _recentFilesViewModel = recentFilesViewModel;

        _tapeManager.TapeChanged += args => Dispatcher.UIThread.Post(() => { UpdateTapeState(args); });
    }

    [RelayCommand]
    private void New() => _tapeManager.NewTape();

    [RelayCommand]
    private async Task Insert()
    {
        var files = await FileDialogs.OpenTapeFileAsync();

        if (files.Count == 0)
        {
            return;
        }

        _tapeManager.InsertTape(files[0].Path.LocalPath);
    }

    [RelayCommand(CanExecute = nameof(CanEject))]
    private async Task Save()
    {
        var fileName = string.Empty;

        var fileType = FileTypes.GetFileType(_recentFilesViewModel.CurrentFileName);

        if (fileType.IsTape())
        {
            fileName = _recentFilesViewModel.CurrentFileName;
        }

        var file = await FileDialogs.SaveTapeFileAsync(Path.GetFileNameWithoutExtension(fileName));

        if (file == null)
        {
            return;
        }

        fileType = FileTypes.GetFileType(file.Path.LocalPath);

        if (fileType == FileType.Tap)
        {
            var tap = _tapeManager.Cassette.Content.ToTap();
            tap.Save(file.Path.LocalPath);
        }
        else
        {
            _tapeManager.Cassette.Content.Save(file.Path.LocalPath);
        }
    }

    [RelayCommand(CanExecute = nameof(CanPlay))]
    private void Play()
    {
        _tapeManager?.PlayTape();
    }

    [RelayCommand(CanExecute = nameof(CanStop))]
    private void Stop()
    {
        _tapeManager?.StopTape();
    }

    [RelayCommand(CanExecute = nameof(CanRewind))]
    private void Rewind()
    {
        _tapeManager?.RewindTape();
    }

    [RelayCommand(CanExecute = nameof(CanEject))]
    private void Eject()
    {
        _tapeManager?.EjectTape();
    }

    [RelayCommand(CanExecute = nameof(CanEject))]
    private void View() => WeakReferenceMessenger.Default.Send(new ShowTapeViewMessage(_tapeManager));

    private void UpdateTapeState(TapeChangedEventArgs args)
    {
        Dispatcher.UIThread.Post(() =>
        {
            switch (args.Action)
            {
                case TapeAction.Inserted:
                    CanRewind = true;
                    CanPlay = true;
                    CanStop = false;
                    CanEject = true;
                    break;

                case TapeAction.Stopped:
                    CanRewind = true;
                    CanPlay = true;
                    CanStop = false;
                    CanEject = true;
                    break;

                case TapeAction.Started:
                    CanRewind = false;
                    CanPlay = false;
                    CanStop = true;
                    CanEject = true;
                    break;

                case TapeAction.Ejected:
                    CanRewind = false;
                    CanPlay = false;
                    CanStop = false;
                    CanEject = false;
                    break;
            }
        });
    }
}