using System.Reactive;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using OldBit.Spectron.Debugger.Logging;
using OldBit.Spectron.Emulation;
using ReactiveUI;

namespace OldBit.Spectron.Debugger.ViewModels;

public class LoggingViewModel : ReactiveObject, IDisposable
{
    private readonly Emulator _emulator;
    private InstructionLogger? _instructionLogger;

    public UserControl? Control { get; set; }

    public ReactiveCommand<Unit, Task> SelectLogFileFileCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> StartLoggingCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> StopLoggingCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> ClearLogFileCommand { get; private set; }

    public LoggingViewModel(Emulator emulator)
    {
        _emulator = emulator;
        SelectLogFileFileCommand = ReactiveCommand.Create(HandleSelectLogFileFileAsync,
            this.WhenAnyValue(x => x.IsLoggingRunning, x => x == false));

        StartLoggingCommand = ReactiveCommand.Create(StartLogging,
            this.WhenAnyValue(x => x.IsLoggingRunning, x => x == false));

        StopLoggingCommand = ReactiveCommand.Create(StopLogging,
            this.WhenAnyValue(x => x.IsLoggingRunning, x => x == true));

        ClearLogFileCommand= ReactiveCommand.Create(ClearLogFile,
            this.WhenAnyValue(x => x.LogFilePath, x => !string.IsNullOrEmpty(x)));
    }

    private async Task HandleSelectLogFileFileAsync()
    {
        var file = await SelectLogFileFileAsync();

        if (file != null)
        {
            LogFilePath = file.Path.LocalPath;
            _instructionLogger = new InstructionLogger(LogFilePath, _emulator);
        }
    }

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

    private void StartLogging()
    {
        IsLoggingRunning = true;
        _instructionLogger?.Enable();
    }

    private void StopLogging()
    {
        IsLoggingRunning = false;
        _instructionLogger?.Disable();
    }

    private void ClearLogFile() => _instructionLogger?.ClearLogFile();

    private string _logFilePath = string.Empty;
    public string LogFilePath
    {
        get => _logFilePath;
        set => this.RaiseAndSetIfChanged(ref _logFilePath, value);
    }

    private bool _isLoggingRunning;
    private bool IsLoggingRunning
    {
        get => _isLoggingRunning;
        set => this.RaiseAndSetIfChanged(ref _isLoggingRunning, value);
    }

    public void Dispose()
    {
        _instructionLogger?.Dispose();

        SelectLogFileFileCommand.Dispose();
        StartLoggingCommand.Dispose();
        StopLoggingCommand.Dispose();
        ClearLogFileCommand.Dispose();

        GC.SuppressFinalize(this);
    }
}