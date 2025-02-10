using Antlr4.Runtime;
using OldBit.Debugger.Parser;
using OldBit.Spectron.Debugger.Extensions;
using OldBit.Spectron.Debugger.Parser.Values;
using OldBit.Z80Cpu;
using OldBit.Z80Cpu.Dasm.Formatters;

namespace OldBit.Spectron.Debugger.Parser;

public class DebuggerVisitor(
    Z80 cpu,
    IMemory memory,
    IOutput output,
    NumberFormat numberFormat) : DebuggerBaseVisitor<Value?>
{
    public List<string> Output { get; } = [];

    public override Value? VisitInt(DebuggerParser.IntContext context) =>
        new Integer(int.Parse(context.INT().GetText()));

    public override Value? VisitHex(DebuggerParser.HexContext context)
    {
        var hex = context.HEX().GetText()
            .Replace("0x", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("h", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("$", string.Empty);

        return new Integer(Convert.ToInt32(hex, 16));
    }

    public override Value? VisitBin(DebuggerParser.BinContext context)
    {
        var bin = context.BIN().GetText()
            .Replace("0b", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("b", string.Empty, StringComparison.OrdinalIgnoreCase);

        return new Integer(Convert.ToInt32(bin, 2));
    }

    public override Value? VisitReg(DebuggerParser.RegContext context) =>
        new Register(context.REG().GetText());

    public override Value? VisitAssign(DebuggerParser.AssignContext context)
    {
        var register = context.REG();

        return base.VisitAssign(context);
    }

    public override Value? VisitPrintstmt(DebuggerParser.PrintstmtContext context)
    {
        var expressions = context.expression();

        foreach (var expression in expressions)
        {
            var expressionValue = base.Visit(expression);

            if (expressionValue is Register register)
            {
                var registerValue = cpu.GetRegisterValue(register.Name);
                var formattedValue = register.Is8Bit ?
                    NumberFormatter.Format((byte)registerValue, numberFormat) :
                    NumberFormatter.Format((Word)registerValue, numberFormat);

                output.Print($"{register.Name}={formattedValue}  ({registerValue})");
            }
            else
            {
                output.Print(expressionValue?.ToString() ?? string.Empty);
            }
        }

        return base.VisitPrintstmt(context);
    }

    public override Value? VisitPokestmt(DebuggerParser.PokestmtContext context)
    {
        var address = Validators.GetValidWordOrThrow(base.Visit(context.address));
        var value = Validators.GetValidByteOrThrow(base.Visit(context.value));

        memory.Write(address, value);

        return base.VisitPokestmt(context);
    }
}