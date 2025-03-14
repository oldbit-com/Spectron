using OldBit.Debugger.Parser;
using OldBit.Spectron.Debugger.Extensions;
using OldBit.Spectron.Debugger.Parser.Values;
using OldBit.Spectron.Disassembly.Formatters;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Debugger.Parser;

public class DebuggerVisitor(
    Z80 cpu,
    IMemory memory,
    IBus bus,
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
            .Replace("b", string.Empty, StringComparison.OrdinalIgnoreCase);

        return new Integer(Convert.ToInt32(bin, 2));
    }

    public override Value? VisitReg(DebuggerParser.RegContext context) =>
        new Register(context.REG().GetText());

    public override Value? VisitAssign(DebuggerParser.AssignContext context)
    {
        var register = context.REG();
        var expression = context.expression();
        var expressionValue = base.Visit(expression);

        var value = GetValue(expressionValue);
        cpu.SetRegisterValue(register.GetText(), value);

        return new Success();
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

        return new Success();
    }

    public override Value? VisitPeekfunc(DebuggerParser.PeekfuncContext context)
    {
        var address = GetAddressValue(base.Visit(context.address));

        var value = memory.Read(address);

        return new Integer(value, value.GetType());
    }

    public override Value? VisitOutfunc(DebuggerParser.OutfuncContext context)
    {
        var address = Validators.GetValidWordOrThrow(base.Visit(context.address));
        var value = Validators.GetValidByteOrThrow(base.Visit(context.value));

        bus.Write(address, value);

        return new Success();
    }

    public override Value? VisitInfunc(DebuggerParser.InfuncContext context)
    {
        var address = GetAddressValue(base.Visit(context.address));

        var value = bus.Read(address);

        return new Integer(value, value.GetType());
    }

    public override Value? VisitHelpstmt(DebuggerParser.HelpstmtContext context)
    {
        output.Print("Available commands:");
        output.Print("  CLEAR - Clear this output window");
        output.Print("  PRINT or ? <expression> - Print the value of an expression, register");
        output.Print("  POKE <address>,<value> - Write a value to a memory address");
        output.Print("  PEEK <address> - Read a value from a memory address");
        output.Print("  OUT <port>,<value> - Write a value to an I/O port");
        output.Print("  IN <port> - Read a value from an I/O port");
        output.Print("  R = <value> - Set register value (A, B, C, D, E, H, L, I, R, IXH, IXL, IYH, IYL, AF, AF', BC, BC', DE, DE', HL, HL', IX, IY, PC, SP)");
        output.Print("  Accepted value formats: decimal, hexadecimal or binary (e.g. 255, 0xFF, $FF, FFh, 0b11111111, 11111111b)");
        output.Print(string.Empty);

        return base.VisitHelpstmt(context);
    }

    public override Value? VisitClearstmt(DebuggerParser.ClearstmtContext context)
    {
        output.Clear();

        return base.VisitClearstmt(context);
    }

    private Word GetAddressValue(Value? arg)
    {
        Word address;

        if (arg is Register register)
        {
            address = (Word)cpu.GetRegisterValue(register.Name);
        }
        else
        {
            address = Validators.GetValidWordOrThrow(arg);
        }

        return address;
    }

    private int GetValue(Value? arg) => arg switch
    {
        Register register => cpu.GetRegisterValue(register.Name),
        Integer integer => integer.Value,
        _ => throw new ArgumentException("Invalid value")
    };
}