using System;
using System.Collections.ObjectModel;
using System.Reactive;
using Avalonia.Input;
using OldBit.Z80Cpu;
using ReactiveUI;

namespace OldBit.Spectron.ViewModels.Debugger;

public class DebuggerCodeListViewModel : ViewModelBase
{
    public ObservableCollection<DebuggerCodeLineViewModel> CodeLines { get; } = [];

    public ReactiveCommand<TappedEventArgs, Unit> ToggleBreakpointCommand { get; private set; }

    public DebuggerCodeListViewModel()
    {
        ToggleBreakpointCommand = ReactiveCommand.Create<TappedEventArgs>(args =>
        {
            Console.WriteLine($"ToggleBreakpointCommand: {args?.Source}");
        });

        for (var i = 0; i < 10; i++)
        {
            CodeLines.Add(new DebuggerCodeLineViewModel
            {
                Address = "0000",
                Code = "NOP",
                IsCurrent = i == 0,
                IsBreakpoint = i == 2
            });
        }
    }

    public void Update(IMemory memory, ushort pc)
    {
        CodeLines.Clear();

        for (var i = 0; i < 25; i++)
        {
            var address = (ushort)(pc + i);
            var code = memory.Read(address);

            CodeLines.Add(new DebuggerCodeLineViewModel
            {
                Address = address.ToString("X4"),
                Code = code.ToString("X2"),
                IsCurrent = i == 0,
                IsBreakpoint = false  // TODO: Implement breakpoints
            });
        }
    }
}