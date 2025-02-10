using Antlr4.Runtime;
using OldBit.Debugger.Parser;
using OldBit.Z80Cpu;
using OldBit.Z80Cpu.Dasm.Formatters;

namespace OldBit.Spectron.Debugger.Parser;

public class Interpreter(
    Z80 cpu,
    IMemory memory,
    IOutput output,
    NumberFormat numberFormat = NumberFormat.HexDollarPrefix)
{
    private readonly SyntaxErrorListener _syntaxErrorListener = new();

    public List<string> Output { get; private set; } = [];

    public void Execute(string input)
    {
        _syntaxErrorListener.Errors.Clear();

        var inputStream = new AntlrInputStream(input);

        var lexer = CreateLexer(inputStream);
        var tokenStream = new CommonTokenStream(lexer);

        var parser = CreateParser(tokenStream);
        var tree = parser.program();

        var visitor = new DebuggerVisitor(cpu, memory, output, numberFormat);
        visitor.Visit(tree);

        Output = visitor.Output;
    }

    private DebuggerParser CreateParser(CommonTokenStream tokenStream)
    {
        var parser = new DebuggerParser(tokenStream);

        parser.RemoveErrorListeners();
        parser.AddErrorListener(_syntaxErrorListener);

        return parser;
    }

    private DebuggerLexer CreateLexer(AntlrInputStream inputStream)
    {
        var lexer = new DebuggerLexer(inputStream);

        lexer.RemoveErrorListeners();
        lexer.AddErrorListener(_syntaxErrorListener);

        return lexer;
    }
}