using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.DependencyInjection;
using OldBit.Spectron.Emulation.Extensions;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Spectron.Emulation.Screen.Palettes;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulator.Tests;

public class EmulatorSteppingTests
{
    private const Word CodeAddress = 0x8000;

    private readonly Emulation.Emulator _emulator = CreateEmulator();
    private int _framesCompleted;
    private int _executionStops;

    public EmulatorSteppingTests()
    {
        _emulator.FrameCompleted += (_, _) => _framesCompleted++;
        _emulator.ExecutionStopped += () => _executionStops++;
    }

    [Fact]
    public void Step_ShouldExecuteOneInstruction()
    {
        // LD A,5; INC A; JR $
        WriteCode(0x3E, 0x05, 0x3C, 0x18, 0xFE);
        DisableInterrupts();

        _emulator.Step();

        _emulator.Cpu.Registers.PC.ShouldBe((Word)(CodeAddress + 2));
        _emulator.Cpu.Registers.A.ShouldBe((byte)5);

        _emulator.Step();

        _emulator.Cpu.Registers.PC.ShouldBe((Word)(CodeAddress + 3));
        _emulator.Cpu.Registers.A.ShouldBe((byte)6);

        _framesCompleted.ShouldBe(0);
        _executionStops.ShouldBe(2);
    }

    [Fact]
    public void Step_ShouldContinueTheFrame()
    {
        // NOP; NOP
        WriteCode(0x00, 0x00);
        DisableInterrupts();

        _emulator.Step();
        var ticks = _emulator.Cpu.Clock.FrameTicks;

        _emulator.Step();

        _emulator.Cpu.Clock.FrameTicks.ShouldBe(ticks + 4);
    }

    [Fact]
    public void Step_WhenFrameEnds_ShouldCompleteFrameAndStopAtInterruptRoutine()
    {
        // JR $
        WriteCode(0x18, 0xFE);
        EnableInterrupts();
        StepPastInterruptWindow();

        var steps = 0;

        while (_framesCompleted == 0 && steps++ < 10_000)
        {
            _emulator.Step();
        }

        _framesCompleted.ShouldBe(1);
        _emulator.Cpu.Registers.PC.ShouldBe(CodeAddress);

        _emulator.Step();

        _emulator.Cpu.Registers.PC.ShouldBe((Word)0x0038);
        _emulator.Memory.ReadWord(_emulator.Cpu.Registers.SP).ShouldBe(CodeAddress);
        _framesCompleted.ShouldBe(1);
    }

    [Fact]
    public void Step_WhenHalted_ShouldRunToEndOfFrameThenStopAtInterruptRoutine()
    {
        // EI; HALT
        WriteCode(0xFB, 0x76);
        DisableInterrupts();
        StepPastInterruptWindow();

        _emulator.Step();   // EI
        _emulator.Step();   // HALT

        _framesCompleted.ShouldBe(1);
        _emulator.Cpu.IsHalted.ShouldBeTrue();

        _emulator.Step();

        _emulator.Cpu.Registers.PC.ShouldBe((Word)0x0038);
        _emulator.Cpu.IsHalted.ShouldBeFalse();
        _emulator.Memory.ReadWord(_emulator.Cpu.Registers.SP).ShouldBe((Word)(CodeAddress + 2));
    }

    [Fact]
    public void RunFrame_WhenBreakMidFrame_ShouldContinueSameFrameAfterClearBreak()
    {
        var reference = CreateEmulator();
        var referenceInstructions = 0;
        reference.Cpu.BeforeInstruction += _ => referenceInstructions++;

        var instructions = 0;
        _emulator.Cpu.BeforeInstruction += _ =>
        {
            if (++instructions == 1000)
            {
                _emulator.Break();
            }
        };

        reference.RunFrame();
        _emulator.RunFrame();

        _emulator.IsDebuggerBreak.ShouldBeTrue();
        _framesCompleted.ShouldBe(0);
        _executionStops.ShouldBe(1);

        _emulator.ClearBreak();
        _emulator.RunFrame();

        _framesCompleted.ShouldBe(1);
        _emulator.Cpu.Registers.PC.ShouldBe(reference.Cpu.Registers.PC);
        _emulator.Cpu.Clock.FrameTicks.ShouldBe(reference.Cpu.Clock.FrameTicks);

        // The instruction where the break happened is announced again when the frame continues
        instructions.ShouldBe(referenceInstructions + 1);
    }

    [Fact]
    public void Step_WhenNextInstructionIsRomTrap_ShouldRunTrapOnlyWhenInstructionIsExecuted()
    {
        _emulator.TapeManager.TapeSaveSpeed = TapeSpeed.Instant;
        _emulator.TapeManager.IsTapeSaveEnabled = true;

        // CALL SA_BYTES
        WriteCode(0xCD, RomRoutines.SA_BYTES & 0xFF, RomRoutines.SA_BYTES >> 8);
        DisableInterrupts();

        _emulator.Step();

        _emulator.Cpu.Registers.PC.ShouldBe(RomRoutines.SA_BYTES);
        _emulator.TapeManager.Cassette.IsEmpty.ShouldBeTrue();

        _emulator.Step();

        _emulator.TapeManager.Cassette.IsEmpty.ShouldBeFalse();
    }

    [Fact]
    public void RunFrame_WhenBreakMidFrame_ShouldDrawBorderUpToBeam()
    {
        // LD A,2; OUT ($FE),A; JR $
        WriteCode(0x3E, 0x02, 0xD3, 0xFE, 0x18, 0xFE);
        DisableInterrupts();

        var instructions = 0;
        _emulator.Cpu.BeforeInstruction += _ =>
        {
            // Well past the top border
            if (++instructions == 2000)
            {
                _emulator.Break();
            }
        };

        _emulator.RunFrame();

        _executionStops.ShouldBe(1);
        // Row 20 is in the top border, drawn after the colour change and before the break
        var frameBuffer = _emulator.ScreenBuffer.FrameBuffer;
        frameBuffer.Pixels[20 * frameBuffer.Width].ShouldBe(SpectrumPalette.Red);
    }

    [Fact]
    public void RunFrame_WhenNoBreak_ShouldNotRaiseExecutionStopped()
    {
        _emulator.RunFrame();
        _emulator.RunFrame();

        _framesCompleted.ShouldBe(2);
        _executionStops.ShouldBe(0);
    }

    private void WriteCode(params byte[] code)
    {
        for (var i = 0; i < code.Length; i++)
        {
            _emulator.Memory.Write((Word)(CodeAddress + i), code[i]);
        }

        _emulator.Cpu.Registers.PC = CodeAddress;
        _emulator.Cpu.Registers.SP = 0xFF00;
    }

    private void DisableInterrupts()
    {
        _emulator.Cpu.IFF1 = false;
        _emulator.Cpu.IFF2 = false;
    }

    private void EnableInterrupts()
    {
        _emulator.Cpu.IM = InterruptMode.Mode1;
        _emulator.Cpu.IFF1 = true;
        _emulator.Cpu.IFF2 = true;
    }

    // Moves the clock past the interrupt window without executing the code under test
    private void StepPastInterruptWindow()
    {
        var pc = _emulator.Cpu.Registers.PC;
        var (iff1, iff2) = (_emulator.Cpu.IFF1, _emulator.Cpu.IFF2);

        DisableInterrupts();

        _emulator.Memory.Write(0x9000, 0x18);   // JR $
        _emulator.Memory.Write(0x9001, 0xFE);
        _emulator.Cpu.Registers.PC = 0x9000;

        while (_emulator.Cpu.Clock.FrameTicks < 100)
        {
            _emulator.Step();
        }

        _emulator.Cpu.Registers.PC = pc;
        (_emulator.Cpu.IFF1, _emulator.Cpu.IFF2) = (iff1, iff2);
    }

    private static Emulation.Emulator CreateEmulator()
    {
        var services = new ServiceCollection();

        services.AddEmulation();
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

        using var provider = services.BuildServiceProvider();

        return provider.GetRequiredService<EmulatorFactory>().Create(ComputerType.Spectrum48K, RomType.Original);
    }
}
