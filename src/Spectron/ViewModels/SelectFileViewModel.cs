using System.Collections.Generic;
using System.Reactive;
using OldBit.Spectron.Emulation.Files;
using ReactiveUI;

namespace OldBit.Spectron.ViewModels;

public class SelectFileViewModel : ReactiveObject
{
    public ReactiveCommand<Unit, ArchiveEntry?> SelectFileCommand { get; }

    public SelectFileViewModel()
    {
        SelectFileCommand = ReactiveCommand.Create(() => SelectedFile);
    }

    public List<ArchiveEntry> FileNames { get; set; } = [];

    private ArchiveEntry? _selectedFile;
    public ArchiveEntry? SelectedFile
    {
        get => _selectedFile;
        set => this.RaiseAndSetIfChanged(ref _selectedFile, value);
    }
}