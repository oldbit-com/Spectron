using System.Collections.Generic;
using System.Reactive;
using OldBit.Spectron.Emulation.Storage;
using ReactiveUI;

namespace OldBit.Spectron.ViewModels;

public class SelectFileViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, ArchiveEntry?> FileSelectedCommand { get; }

    public SelectFileViewModel()
    {
        FileSelectedCommand = ReactiveCommand.Create(() => SelectedFile);
    }

    public List<ArchiveEntry> FileNames { get; set; } = [];

    private ArchiveEntry? _selectedFile;
    public ArchiveEntry? SelectedFile
    {
        get => _selectedFile;
        set => this.RaiseAndSetIfChanged(ref _selectedFile, value);
    }
}