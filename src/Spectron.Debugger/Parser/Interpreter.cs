using Antlr4.Runtime;
using OldBit.Debugger.Parser;
using OldBit.Spectron.Debugger.Parser.Values;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Debugger.Parser;

public class Interpreter(Z80 cpu, IMemory memory, IBus bus, IOutput output)
{
    private readonly SyntaxErrorListener _syntaxErrorListener = new();

    public List<string> Output { get; private set; } = [];

    public Value? Execute(string input)
    {
        _syntaxErrorListener.Errors.Clear();

        var inputStream = new AntlrInputStream(input);

        var lexer = CreateLexer(inputStream);
        var tokenStream = new CommonTokenStream(lexer);

        var parser = CreateParser(tokenStream);
        var tree = parser.program();

        if (_syntaxErrorListener.Errors.Count > 0)
        {
            throw new SyntaxErrorException(_syntaxErrorListener.Errors[0]);
        }

        var visitor = new DebuggerVisitor(cpu, memory, bus, output);
        var result = visitor.Visit(tree);

        Output = visitor.Output;

        return result;
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