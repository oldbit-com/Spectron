using OldBit.Spectron.Disassembly;
using OldBit.Spectron.Emulation;

namespace OldBit.Spectron.Debugger.Logging;

internal sealed class InstructionLogger : IDisposable
{
    private readonly string _logFilePath;
    private readonly Emulator _emulator;
    private readonly Disassembler _disassembler;

    private StreamWriter _textWriter;

    internal bool IsEnabled { get; private set; }

    internal bool ShouldLogTicks { get; set; } = true;

    internal InstructionLogger(string logFilePath, Emulator emulator)
    {
        _logFilePath = logFilePath;
        _emulator = emulator;
        _emulator.Cpu.BeforeInstruction += CpuOnBeforeInstruction;

        _disassembler = new Disassembler(emulator.Memory, 0, 1);

        _textWriter = new StreamWriter(logFilePath);
    }

    internal void Enable() => IsEnabled = true;

    internal void Disable() => IsEnabled = false;

    internal void ClearLogFile()
    {
        _textWriter.Close();
        _textWriter = new StreamWriter(_logFilePath, false);
    }

    private void CpuOnBeforeInstruction(Word pc)
    {
        if (!IsEnabled)
        {
            return;
        }

        var address = _emulator.Cpu.Registers.PC;
        var ticks = _emulator.Cpu.Clock.FrameTicks;
        var instruction = string.Empty;

        var instructions = _disassembler.Disassemble(address);

        if (instructions.Count > 0)
        {
            instruction = instructions[0].ToString();
        }

        _textWriter.WriteLine(ShouldLogTicks ?
            $"{ticks} {address:X4} {instruction}" :
            $"{address:X4} {instruction}");
    }

    public void Dispose()
    {
        IsEnabled = false;

        _emulator.Cpu.BeforeInstruction -= CpuOnBeforeInstruction;
        _textWriter.Dispose();
    }
}