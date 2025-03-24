using OldBit.Debugger.Parser;
using OldBit.Spectron.Debugger.Extensions;
using OldBit.Spectron.Debugger.Parser.Values;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Debugger.Parser;

public class DebuggerVisitor(Z80 cpu, IMemory memory, IBus bus, IOutput output) : DebuggerBaseVisitor<Value?>
{
    public List<string> Output { get; } = [];

    public override Value? VisitInt(DebuggerParser.IntContext context) =>
        new Integer(int.Parse(context.INT().GetText()));

    public override Value? VisitHex(DebuggerParser.HexContext context)
    {
        var hex = context.HEX().GetText()
            .Replace("0x", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("h", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("$", string.Empty)
            .Replace("#", string.Empty);

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

        var values = expressions.Select(expression => base.Visit(expression)).ToList();

        return new Print(values);
    }

    public override Value? VisitGotostmt(DebuggerParser.GotostmtContext context)
    {
        var address = GetAddressValue(base.Visit(context.address));

        cpu.Registers.PC = address;

        return new GotoAction(address);
    }

    public override Value? VisitListstmt(DebuggerParser.ListstmtContext context)
    {
        var address = context.address == null ? cpu.Registers.PC : GetAddressValue(base.Visit(context.address));

        return new ListAction(address);
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

    public override Value? VisitSavestmt(DebuggerParser.SavestmtContext context)
    {
        var filename = context.filepath.Text.Substring(1, context.filepath.Text.Length - 2);
        var address = context.address == null ? (Word)0 : GetAddressValue(base.Visit(context.address));
        var length = context.length == null ? null : base.Visit(context.length);

        return new SaveAction(filename, address, length == null ? null : GetValue(length));
    }

    public override Value? VisitHelpstmt(DebuggerParser.HelpstmtContext context)
    {
        output.Print("Available commands:");
        output.Print("  CLEAR - Clear this output window");
        output.Print("  PRINT or ? <expression> - Prints a value of the expression, for example: PRINT HL'");
        output.Print("  GOTO <address> - Jump to a memory address, for example: GOTO 16384, equivalent to PC=16384");
        output.Print("  LIST [<address>] - Disassemble instructions starting at a memory address, for example: LIST 0xC000");
        output.Print("  POKE <address>,<value> - Write a value to a memory address, for example: POKE 16384,255");
        output.Print("  PEEK <address> - Read a value from a memory address, for example: PEEK 16384");
        output.Print("  OUT <port>,<value> - Write a value to an I/O port, for example: OUT 254,255");
        output.Print("  IN <port> - Read a value from an I/O port, for example: IN 254");
        output.Print("  SAVE \"<filename>\" [<address> [,<length>]] - Save the current memory to a file, for example: SAVE \"rom.bin\" 0,16384");
        output.Print("  R = <value> - Set register value (A, B, C, D, E, H, L, I, R, IXH, IXL, IYH, IYL, AF, AF', BC, BC', DE, DE', HL, HL', IX, IY, PC, SP)");
        output.Print("  Accepted value formats: decimal, hexadecimal or binary (e.g. 255, 0xFF, $FF, #FF, FFh, 0b11111111, 11111111b)");
        output.Print(string.Empty);

        return base.VisitHelpstmt(context);
    }

    public override Value? VisitClearstmt(DebuggerParser.ClearstmtContext context)
    {
        output.Clear();

        return base.VisitClearstmt(context);
    }

    public override Value? VisitParens(DebuggerParser.ParensContext context)
    {
        return base.Visit(context.expression());
    }

    public override Value? VisitAddSub(DebuggerParser.AddSubContext context)
    {
        var left = GetValue(Visit(context.expression(0)));
        var right = GetValue(Visit(context.expression(1)));

        return context.operation.Text switch
        {
            "+" => new Integer(left + right),
            "-" => new Integer(left - right),
            _ => base.VisitAddSub(context)
        };
    }

    public override Value? VisitMulDiv(DebuggerParser.MulDivContext context)
    {
        var left = GetValue(Visit(context.expression(0)));
        var right = GetValue(Visit(context.expression(1)));

        return context.operation.Text switch
        {
            "*" => new Integer(left * right),
            "/" => new Integer(left / right),
            _ => base.VisitMulDiv(context)
        };
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