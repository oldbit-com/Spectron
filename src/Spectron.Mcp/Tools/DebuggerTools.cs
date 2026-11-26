using System.ComponentModel;
using System.Text;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using OldBit.Spectron.Debugger.Breakpoints;
using OldBit.Spectron.Debugger.Stepping;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Extensions;

namespace OldBit.Spectron.Mcp.Tools;

/// <summary>
/// Breakpoint and stepping tools. Breakpoints are shared with the debugger window. Stepping over or out and
/// running until a condition use a temporary breakpoint that lives only for the call.
/// </summary>
[McpServerToolType]
public sealed class DebuggerTools(EmulatorController controller)
{
    internal const int MaxStepCount = 1000;
    private const int MaxExecutedLines = 20;

    private const string ConditionSyntax =
        "Use a register condition such as PC==$8000, A==5 or HL==16384 (stops when the register changes to the value), " +
        "an address such as $5C3A (stops after any write to it) or $5C3A==0 (stops after a write of that value). " +
        "Numbers: decimal, $FF, 0xFF, #FF or FFh. Registers: A B C D E H L IXH IXL IYH IYL AF BC DE HL SP PC IX IY.";

    private enum StepMode { Into, Over, Out }

    [McpServerTool(Name = "set_breakpoint")]
    [Description("""
                 Sets a breakpoint. Breakpoints are shared with the user's debugger window and stop run_frames, type_text, press_keys,
                 type_basic, step over/out and run_until; the result of those tools says which breakpoint stopped execution.
                 Condition syntax (same as the debugger):
                 - Register reaches a value: PC==$8000, A==5, HL==16384. Fires when the register changes to the value, so a PC breakpoint
                   stops before the instruction at that address runs.
                 - Memory write: $5C3A (any write to the address) or $5C3A==0 (a write of that value). Stops after the writing instruction.
                 Numbers: decimal, $FF, 0xFF, #FF or FFh. Registers: A B C D E H L IXH IXL IYH IYL AF BC DE HL SP PC IX IY.
                 Turns breakpoints on if the user had them off. Setting an existing condition again returns its id.
                 """)]
    public async Task<string> SetBreakpoint(
        [Description("The condition, e.g. \"PC==$8000\" or \"$5C3A==0\".")] string condition)
    {
        var breakpoint = ParseCondition(condition);

        return await controller.RunHostActionAsync(async host =>
        {
            var handler = host.Breakpoints ?? throw new McpException("Emulator is not running.");
            var manager = handler.BreakpointManager;
            var existing = manager.Breakpoints.FirstOrDefault(b => IsSameCondition(b, breakpoint));

            string result;

            if (existing == null)
            {
                manager.AddBreakpoint(breakpoint);
                result = $"Breakpoint #{breakpoint.Id} set: {breakpoint.Condition}.";
            }
            else if (existing.IsEnabled)
            {
                result = $"Breakpoint #{existing.Id} already exists: {existing.Condition}.";
            }
            else
            {
                manager.UpdateBreakpoint(existing, breakpoint);
                result = $"Breakpoint #{breakpoint.Id} already existed but was disabled, enabled it: {breakpoint.Condition}.";
            }

            if (!handler.IsEnabled)
            {
                await host.EnableBreakpointsAsync();
                result += " Breakpoints were turned off, turned them on.";
            }

            return result;
        });
    }

    [McpServerTool(Name = "list_breakpoints", ReadOnly = true)]
    [Description("Lists the breakpoints (id, condition, disabled ones are marked), whether breakpoints are turned on, " +
                 "and which breakpoint execution is stopped at, if any.")]
    public Task<string> ListBreakpoints() => controller.ExecuteAsync(emulator =>
    {
        var handler = controller.Breakpoints;
        var breakpoints = handler?.BreakpointManager.Breakpoints ?? [];
        var result = new StringBuilder();

        if (breakpoints.Count == 0)
        {
            result.Append("No breakpoints.");
        }
        else
        {
            result.Append(handler!.IsEnabled
                ? "Breakpoints are on."
                : "Breakpoints are turned off, they don't stop execution. set_breakpoint turns them on.");

            foreach (var breakpoint in breakpoints.OrderBy(b => b.Id))
            {
                result.Append($"\n#{breakpoint.Id} {breakpoint.Condition}{(breakpoint.IsEnabled ? "" : " (disabled)")}");
            }
        }

        var stop = FormatCurrentStop(controller, emulator);

        if (stop != null)
        {
            result.Append('\n').Append(stop);
        }

        return result.ToString();
    });

    [McpServerTool(Name = "clear_breakpoint", Destructive = true)]
    [Description("Removes a breakpoint by id (from set_breakpoint or list_breakpoints), or all breakpoints. " +
                 "Breakpoints are shared with the user's debugger window, so this removes them there too.")]
    public async Task<string> ClearBreakpoint(
        [Description("The breakpoint id.")] int? id = null,
        [Description("True to remove all breakpoints.")] bool all = false)
    {
        if (id == null && !all)
        {
            throw new McpException("Give the id of the breakpoint to remove, or all=true.");
        }

        return await controller.RunHostActionAsync(host =>
        {
            var manager = (host.Breakpoints ?? throw new McpException("Emulator is not running.")).BreakpointManager;

            if (all)
            {
                var breakpoints = manager.Breakpoints;

                foreach (var breakpoint in breakpoints)
                {
                    manager.RemoveBreakpoint(breakpoint);
                }

                return Task.FromResult($"Removed {breakpoints.Count} breakpoint(s).");
            }

            var match = manager.Breakpoints.FirstOrDefault(b => b.Id == id) ??
                        throw new McpException($"There is no breakpoint #{id}. Use list_breakpoints to see them.");

            manager.RemoveBreakpoint(match);

            return Task.FromResult($"Removed breakpoint #{match.Id}: {match.Condition}.");
        });
    }

    [McpServerTool(Name = "step")]
    [Description("""
                 Executes Z80 code one step at a time while paused, and shows the executed instructions, registers and the next instruction.
                 - into: one instruction. A CALL or RST goes into the routine. If an interrupt is accepted, the step stops at the start
                   of the interrupt routine ($0038 in IM 1) without running the instruction. On HALT, the step runs to the end of the frame.
                 - over: like into, but runs a CALL, CALL cc, RST, DJNZ, conditional JP/JR, repeating block instruction (LDIR etc.) or HALT
                   until execution reaches the next instruction after it, so routines and loops run as one step.
                 - out: runs until the current routine returns (a RET, RETI or RETN that takes the stack above where it was).
                 over and out also stop at breakpoints, and after maxFrames frames if the target is never reached.
                 They use a temporary target, so nothing is added to the breakpoint list. Emulation stays paused afterwards.
                 """)]
    public Task<string> Step(
        [Description("into, over or out.")] string mode = "into",
        [Description("Number of steps, 1-1000.")] int count = 1,
        [Description("over and out only: frames to run before giving up on reaching the target (50 frames = 1 second).")] int maxFrames = 50)
    {
        var stepMode = mode.Trim().ToLowerInvariant() switch
        {
            "into" => StepMode.Into,
            "over" => StepMode.Over,
            "out" => StepMode.Out,
            _ => throw new McpException($"Unknown mode '{mode}'. Use into, over or out.")
        };

        if (count is < 1 or > MaxStepCount)
        {
            throw new McpException($"Count must be between 1 and {MaxStepCount}.");
        }

        ValidateMaxFrames(maxFrames);

        return controller.RunAsync(emulator => ExecuteSteps(emulator, stepMode, count, maxFrames), leavePaused: true);
    }

    [McpServerTool(Name = "run_until")]
    [Description("""
                 Runs emulation until a condition becomes true, then pauses. Takes the same condition syntax as set_breakpoint
                 (PC==$8000, A==5, $5C3A, $5C3A==0), but the condition is only used for this call and never appears in the breakpoint list.
                 A register condition fires the next time the register changes to the value, so it doesn't fire at once if the value
                 is already there (e.g. PC==$8000 while PC is $8000 runs until execution comes back to $8000).
                 Also stops at breakpoints, and after maxFrames frames. Emulation stays paused afterwards.
                 """)]
    public Task<string> RunUntil(
        [Description("The condition, e.g. \"PC==$8000\".")] string condition,
        [Description("Frames to run before giving up (50 frames = 1 second).")] int maxFrames = 500)
    {
        var breakpoint = ParseCondition(condition);

        ValidateMaxFrames(maxFrames);

        return controller.RunAsync(emulator =>
        {
            var result = controller.RunToBreakpoint(emulator, breakpoint, maxFrames);

            var text = result.StopReason switch
            {
                StopReason.None => $"The condition {breakpoint.Condition} was not met within {result.Frames} frame(s).",
                StopReason.Breakpoint when result.Hit is { IsTemporary: true } =>
                    $"Condition {breakpoint.Condition} met after {result.Frames} frame(s).",
                StopReason.Breakpoint =>
                    $"{FormatHit(emulator, result.Hit)} The condition {breakpoint.Condition} was not met yet, after {result.Frames} frame(s).",
                _ => FormatInterruption(result.StopReason, result.Frames)
            };

            return $"{text}\n{FormatPosition(emulator, result.Hit?.PreviousAddress)}";
        }, leavePaused: true);
    }

    private string ExecuteSteps(Emulator emulator, StepMode mode, int count, int maxFrames)
    {
        var executed = new List<string>();
        var frames = 0;
        Word? previousPc = null;
        string? note = null;

        for (var i = 0; i < count && note == null; i++)
        {
            var pc = emulator.Cpu.Registers.PC;
            var target = mode == StepMode.Over ? StepOver.GetReturnAddress(emulator.Memory, pc) : null;

            RunResult result;

            if (mode == StepMode.Into || (mode == StepMode.Over && target == null))
            {
                var instruction = DescribeInstruction(emulator, pc);
                var (sp, iff1, isHalt) = (emulator.Cpu.Registers.SP, emulator.Cpu.IFF1, emulator.Memory.Read(pc) == 0x76);

                result = controller.Step(emulator);

                if (result.StopReason is StopReason.ResumedByUser or StopReason.EmulatorReplaced)
                {
                    note = FormatInterruption(result.StopReason, frames);
                    break;
                }

                if (IsInterruptAccepted(emulator, pc, sp, iff1))
                {
                    executed.Add($"{instruction}  (not executed, an interrupt was accepted)");
                    note = "Stopped at the start of the interrupt routine.";
                }
                else
                {
                    executed.Add(instruction);
                    previousPc = pc;

                    if (isHalt)
                    {
                        note = "HALT ran to the end of the frame, the next step accepts the interrupt.";
                    }
                }
            }
            else
            {
                var budget = maxFrames - frames;

                if (budget <= 0)
                {
                    note = $"Stopped, the frame budget of {maxFrames} frame(s) is used up.";
                    break;
                }

                if (mode == StepMode.Over)
                {
                    executed.Add($"{DescribeInstruction(emulator, pc)}  (stepped over)");
                    result = controller.RunToBreakpoint(emulator, new RegisterBreakpoint(Register.PC, target!.Value), budget);
                }
                else
                {
                    executed.Add($"${pc:X4}  ... ran until the routine returned");
                    result = controller.RunToReturn(emulator, budget);
                }

                frames += result.Frames;

                if (result.Hit is { IsTemporary: true })
                {
                    previousPc = result.Hit.PreviousAddress;
                }
                else if (result.StopReason == StopReason.None)
                {
                    note = mode == StepMode.Over
                        ? $"Did not reach ${target!.Value:X4} within {maxFrames} frame(s), stopped."
                        : $"The routine did not return within {maxFrames} frame(s), stopped.";
                }
            }

            switch (result.StopReason)
            {
                case StopReason.Breakpoint when result.Hit is not { IsTemporary: true }:
                    previousPc = result.Hit?.PreviousAddress ?? previousPc;
                    note = FormatHit(emulator, result.Hit);
                    break;

                case StopReason.ResumedByUser or StopReason.EmulatorReplaced:
                    note = FormatInterruption(result.StopReason, frames);
                    break;
            }
        }

        var text = new StringBuilder();
        var skipped = executed.Count - MaxExecutedLines;

        text.Append(mode switch
        {
            StepMode.Into => $"Stepped into {executed.Count} instruction(s)",
            StepMode.Over => $"Stepped over {executed.Count} instruction(s)",
            _ => $"Stepped out {executed.Count} time(s)"
        });

        text.Append(frames > 0 ? $", ran {frames} frame(s):" : ":");

        if (skipped > 0)
        {
            text.Append($"\n  ... {skipped} earlier step(s)");
        }

        foreach (var line in executed.Skip(Math.Max(0, skipped)))
        {
            text.Append("\n  ").Append(line);
        }

        if (note != null)
        {
            text.Append('\n').Append(note);
        }

        text.Append('\n').Append(FormatPosition(emulator, previousPc));

        return text.ToString();
    }

    // An accepted interrupt pushes the address of the instruction that didn't run and clears IFF1
    private static bool IsInterruptAccepted(Emulator emulator, Word pc, Word sp, bool iff1) =>
        iff1 && !emulator.Cpu.IFF1 &&
        emulator.Cpu.Registers.SP == (Word)(sp - 2) &&
        emulator.Memory.ReadWord(emulator.Cpu.Registers.SP) == pc;

    private static string DescribeInstruction(Emulator emulator, Word address) =>
        EmulatorTools.FormatDisassembly(emulator, address, 1);

    /// <summary>
    /// Formats where execution is: PC, previous PC, frame T-state, the next instruction and the registers.
    /// </summary>
    private static string FormatPosition(Emulator emulator, Word? previousPc)
    {
        var pc = emulator.Cpu.Registers.PC;
        var previous = previousPc == null ? "" : $", previous PC=${previousPc:X4}";

        return $"""
                PC=${pc:X4}{previous}, frame T-state {emulator.Cpu.Clock.FrameTicks}.
                Next: {DescribeInstruction(emulator, pc)}
                {EmulatorTools.FormatRegisters(emulator.Cpu)}
                """;
    }

    /// <summary>
    /// Describes a stop at a user breakpoint, e.g. "Stopped at breakpoint #2 (PC == $8000), previous PC=$7FFD, frame T-state 31744."
    /// </summary>
    internal static string FormatHit(Emulator emulator, BreakpointHit? hit)
    {
        var ticks = emulator.Cpu.Clock.FrameTicks;

        if (hit == null)
        {
            return $"Stopped at a breakpoint, frame T-state {ticks}.";
        }

        return $"Stopped at {FormatBreakpoint(hit.Breakpoint)}, previous PC=${hit.PreviousAddress:X4}, frame T-state {ticks}.";
    }

    /// <summary>
    /// Describes the breakpoint execution is stopped at, or returns null when it isn't stopped at one.
    /// </summary>
    internal static string? FormatCurrentStop(EmulatorController controller, Emulator emulator) =>
        emulator.IsDebuggerBreak ? FormatHit(emulator, controller.GetCurrentHit(emulator)) : null;

    internal static string FormatBreakpointSummary(BreakpointHandler? handler)
    {
        var count = handler?.BreakpointManager.Breakpoints.Count ?? 0;

        return handler?.IsEnabled == true
            ? $"Breakpoints: {count} (on)"
            : $"Breakpoints: {count} (turned off)";
    }

    private static string FormatBreakpoint(Breakpoint? breakpoint) => breakpoint switch
    {
        null => "a breakpoint",
        RegisterBreakpoint { IsSingleUse: true } single => $"the debugger's step target (PC == ${single.Value:X4})",
        _ => $"breakpoint #{breakpoint.Id} ({breakpoint.Condition})"
    };

    private static string FormatInterruption(StopReason stopReason, int frames) => stopReason switch
    {
        StopReason.ResumedByUser => $"Stopped after {frames} frame(s) because the user resumed emulation.",
        StopReason.EmulatorReplaced => $"Stopped after {frames} frame(s) because the machine was replaced (file loaded or machine reset).",
        _ => string.Empty
    };

    private static void ValidateMaxFrames(int maxFrames)
    {
        if (maxFrames is < 1 or > EmulatorController.MaxFramesPerCall)
        {
            throw new McpException($"maxFrames must be between 1 and {EmulatorController.MaxFramesPerCall}.");
        }
    }

    /// <summary>
    /// Parses a condition with the debugger's syntax and gives it a canonical text, e.g. "pc==32768" becomes "PC == $8000".
    /// </summary>
    internal static Breakpoint ParseCondition(string? condition)
    {
        if (!BreakpointParser.TryParse(condition?.Trim(), out var parsed))
        {
            throw new McpException($"Invalid condition '{condition}'. {ConditionSyntax}");
        }

        return parsed switch
        {
            RegisterBreakpoint register => new RegisterBreakpoint(register.Register, register.Value)
            {
                Condition = register.Register < Register.AF
                    ? $"{register.Register} == ${register.Value:X2}"
                    : $"{register.Register} == ${register.Value:X4}"
            },
            MemoryBreakpoint memory => new MemoryBreakpoint(memory.Address, memory.Value)
            {
                Condition = memory.Value == null
                    ? $"${memory.Address:X4}"
                    : $"${memory.Address:X4} == ${memory.Value:X2}"
            },
            _ => parsed
        };
    }

    private static bool IsSameCondition(Breakpoint a, Breakpoint b) => (a, b) switch
    {
        (RegisterBreakpoint x, RegisterBreakpoint y) => x.Register == y.Register && x.Value == y.Value,
        (MemoryBreakpoint x, MemoryBreakpoint y) => x.Address == y.Address && x.Value == y.Value,
        _ => false
    };
}
