using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OldBit.Spectron.Debugger.Logging;
using OldBit.Spectron.Emulation;

namespace OldBit.Spectron.Debugger.ViewModels;

public partial class LoggingViewModel : ObservableObject, IDisposable
{
    private Emulator _emulator = null!;
    private InstructionLogger? _instructionLogger;

    public UserControl? Control { get; set; }

    private bool CanStartLogging => !IsLoggingRunning;
    private bool CanStopLogging => IsLoggingRunning;
    private bool CanClearLogFile => !string.IsNullOrEmpty(LogFilePath);

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ClearLogFileCommand))]
    private string _logFilePath = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartLoggingCommand))]
    [NotifyCanExecuteChangedFor(nameof(StopLoggingCommand))]
    private bool _isLoggingRunning;

    [ObservableProperty]
    private bool _shouldLogTicks = true;

    partial void OnShouldLogTicksChanged(bool value)
    {
        if (_instructionLogger != null)
        {
            _instructionLogger.ShouldLogTicks = value;
        }
    }

    partial void OnLogFilePathChanged(string value) => OnPropertyChanged(nameof(CanClearLogFile));

    partial void OnIsLoggingRunningChanged(bool value) => OnPropertyChanged(nameof(IsLoggingRunning));

    public void Configure(Emulator emulator)
    {
        _emulator = emulator;

        if (string.IsNullOrEmpty(LogFilePath))
        {
            return;
        }

        var instructionLogger = new InstructionLogger(LogFilePath, _emulator);
        instructionLogger.ShouldLogTicks = ShouldLogTicks;

        if (_instructionLogger?.IsEnabled == true)
        {
            instructionLogger.Enable();
        }

        _instructionLogger = instructionLogger;
    }

    [RelayCommand(CanExecute = nameof(CanSelectLogFile))]
    private async Task SelectLogFileFile()
    {
        var file = await SelectLogFileFileAsync();

        if (file != null)
        {
            LogFilePath = file.Path.LocalPath;

            _instructionLogger = new InstructionLogger(LogFilePath, _emulator);
        }
    }

    private bool CanSelectLogFile() => !IsLoggingRunning;

    private async Task<IStorageFile?> SelectLogFileFileAsync()
    {
        var topLevel = TopLevel.GetTopLevel(Control);

        if (topLevel == null)
        {
            return null;
        }

        return await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Logging FileName",
            DefaultExtension = ".log",
            SuggestedFileName = "spectron_debug.log",
            ShowOverwritePrompt = false,
            FileTypeChoices = [new FilePickerFileType("Log File") { Patterns = ["*.log"] }],
        });
    }

    [RelayCommand(CanExecute = nameof(CanStartLogging))]
    private void StartLogging()
    {
        IsLoggingRunning = true;
        _instructionLogger?.Enable();
    }

    [RelayCommand(CanExecute = nameof(CanStopLogging))]
    private void StopLogging()
    {
        IsLoggingRunning = false;
        _instructionLogger?.Disable();
    }

    [RelayCommand(CanExecute = nameof(CanClearLogFile))]
    private void ClearLogFile() => _instructionLogger?.ClearLogFile();

    public void Dispose()
    {
        _instructionLogger?.Dispose();
        GC.SuppressFinalize(this);
    }
}