//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.13.2
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from Debugger.g4 by ANTLR 4.13.2

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

namespace OldBit.Debugger.Parser {
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using IToken = Antlr4.Runtime.IToken;

/// <summary>
/// This interface defines a complete generic visitor for a parse tree produced
/// by <see cref="DebuggerParser"/>.
/// </summary>
/// <typeparam name="Result">The return type of the visit operation.</typeparam>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.13.2")]
[System.CLSCompliant(false)]
public interface IDebuggerVisitor<Result> : IParseTreeVisitor<Result> {
	/// <summary>
	/// Visit a parse tree produced by <see cref="DebuggerParser.program"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitProgram([NotNull] DebuggerParser.ProgramContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DebuggerParser.statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitStatement([NotNull] DebuggerParser.StatementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DebuggerParser.assign"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAssign([NotNull] DebuggerParser.AssignContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DebuggerParser.helpstmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitHelpstmt([NotNull] DebuggerParser.HelpstmtContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DebuggerParser.printstmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitPrintstmt([NotNull] DebuggerParser.PrintstmtContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DebuggerParser.pokestmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitPokestmt([NotNull] DebuggerParser.PokestmtContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DebuggerParser.peekfunc"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitPeekfunc([NotNull] DebuggerParser.PeekfuncContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DebuggerParser.outfunc"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitOutfunc([NotNull] DebuggerParser.OutfuncContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DebuggerParser.infunc"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitInfunc([NotNull] DebuggerParser.InfuncContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DebuggerParser.clearstmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitClearstmt([NotNull] DebuggerParser.ClearstmtContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DebuggerParser.gotostmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitGotostmt([NotNull] DebuggerParser.GotostmtContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="DebuggerParser.liststmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitListstmt([NotNull] DebuggerParser.ListstmtContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>Int</c>
	/// labeled alternative in <see cref="DebuggerParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitInt([NotNull] DebuggerParser.IntContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>Hex</c>
	/// labeled alternative in <see cref="DebuggerParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitHex([NotNull] DebuggerParser.HexContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>Bin</c>
	/// labeled alternative in <see cref="DebuggerParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBin([NotNull] DebuggerParser.BinContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>Reg</c>
	/// labeled alternative in <see cref="DebuggerParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitReg([NotNull] DebuggerParser.RegContext context);
}
} // namespace OldBit.Debugger.Parser
