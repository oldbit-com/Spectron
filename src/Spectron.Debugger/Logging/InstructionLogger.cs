using System.IO.IsolatedStorage;
using OldBit.Spectron.Disassembly;
using OldBit.Spectron.Emulation;
using OldBit.Z80Cpu.Events;

namespace OldBit.Spectron.Debugger.Logging;

internal sealed class InstructionLogger : IDisposable
{
    private readonly string _logFilePath;
    private readonly Emulator _emulator;

    private StreamWriter _textWriter;
    private bool _isEnabled;

    internal InstructionLogger(string logFilePath, Emulator emulator)
    {
        _logFilePath = logFilePath;
        _emulator = emulator;
        _emulator.Cpu.BeforeInstruction += CpuOnBeforeInstruction;

        _textWriter = new StreamWriter(logFilePath);
    }

    internal void Enable() => _isEnabled = true;

    internal void Disable() => _isEnabled = false;

    internal void ClearLogFile()
    {
        _textWriter.Close();
        _textWriter = new StreamWriter(_logFilePath, false);
    }

    private void CpuOnBeforeInstruction(BeforeInstructionEventArgs e)
    {
        if (!_isEnabled)
        {
            return;
        }

        var address = _emulator.Cpu.Registers.PC;
        var ticks = _emulator.Cpu.Clock.CurrentFrameTicks;

        _textWriter.WriteLine($"{address:X4} {ticks}");
    }

    public void Dispose()
    {
        _isEnabled = false;

        _emulator.Cpu.BeforeInstruction -= CpuOnBeforeInstruction;
        _textWriter.Dispose();
    }
}