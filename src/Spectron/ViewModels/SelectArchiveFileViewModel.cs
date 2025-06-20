using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OldBit.Spectron.Emulation.Files;
using OldBit.Spectron.Messages;

namespace OldBit.Spectron.ViewModels;

public partial class SelectArchiveFileViewModel(List<ArchiveEntry> fileNames) : ObservableObject
{
    [ObservableProperty]
    private ArchiveEntry? _selectedFile;

    public List<ArchiveEntry> FileNames { get; } = fileNames;

    [RelayCommand]
    private void SelectFile() =>
        WeakReferenceMessenger.Default.Send(new SelectArchiveFileMessage(SelectedFile));
}