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
using IParseTreeListener = Antlr4.Runtime.Tree.IParseTreeListener;
using IToken = Antlr4.Runtime.IToken;

/// <summary>
/// This interface defines a complete listener for a parse tree produced by
/// <see cref="DebuggerParser"/>.
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.13.2")]
[System.CLSCompliant(false)]
public interface IDebuggerListener : IParseTreeListener {
	/// <summary>
	/// Enter a parse tree produced by <see cref="DebuggerParser.program"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterProgram([NotNull] DebuggerParser.ProgramContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="DebuggerParser.program"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitProgram([NotNull] DebuggerParser.ProgramContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="DebuggerParser.statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterStatement([NotNull] DebuggerParser.StatementContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="DebuggerParser.statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitStatement([NotNull] DebuggerParser.StatementContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="DebuggerParser.assign"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterAssign([NotNull] DebuggerParser.AssignContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="DebuggerParser.assign"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitAssign([NotNull] DebuggerParser.AssignContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="DebuggerParser.helpstmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterHelpstmt([NotNull] DebuggerParser.HelpstmtContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="DebuggerParser.helpstmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitHelpstmt([NotNull] DebuggerParser.HelpstmtContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="DebuggerParser.printstmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterPrintstmt([NotNull] DebuggerParser.PrintstmtContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="DebuggerParser.printstmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitPrintstmt([NotNull] DebuggerParser.PrintstmtContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="DebuggerParser.pokestmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterPokestmt([NotNull] DebuggerParser.PokestmtContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="DebuggerParser.pokestmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitPokestmt([NotNull] DebuggerParser.PokestmtContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="DebuggerParser.peekfunc"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterPeekfunc([NotNull] DebuggerParser.PeekfuncContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="DebuggerParser.peekfunc"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitPeekfunc([NotNull] DebuggerParser.PeekfuncContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="DebuggerParser.outfunc"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterOutfunc([NotNull] DebuggerParser.OutfuncContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="DebuggerParser.outfunc"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitOutfunc([NotNull] DebuggerParser.OutfuncContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="DebuggerParser.infunc"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterInfunc([NotNull] DebuggerParser.InfuncContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="DebuggerParser.infunc"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitInfunc([NotNull] DebuggerParser.InfuncContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="DebuggerParser.clearstmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterClearstmt([NotNull] DebuggerParser.ClearstmtContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="DebuggerParser.clearstmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitClearstmt([NotNull] DebuggerParser.ClearstmtContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="DebuggerParser.gotostmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterGotostmt([NotNull] DebuggerParser.GotostmtContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="DebuggerParser.gotostmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitGotostmt([NotNull] DebuggerParser.GotostmtContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="DebuggerParser.liststmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterListstmt([NotNull] DebuggerParser.ListstmtContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="DebuggerParser.liststmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitListstmt([NotNull] DebuggerParser.ListstmtContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="DebuggerParser.savestmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterSavestmt([NotNull] DebuggerParser.SavestmtContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="DebuggerParser.savestmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitSavestmt([NotNull] DebuggerParser.SavestmtContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>Reg</c>
	/// labeled alternative in <see cref="DebuggerParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterReg([NotNull] DebuggerParser.RegContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>Reg</c>
	/// labeled alternative in <see cref="DebuggerParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitReg([NotNull] DebuggerParser.RegContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>MulDiv</c>
	/// labeled alternative in <see cref="DebuggerParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterMulDiv([NotNull] DebuggerParser.MulDivContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>MulDiv</c>
	/// labeled alternative in <see cref="DebuggerParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitMulDiv([NotNull] DebuggerParser.MulDivContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>AddSub</c>
	/// labeled alternative in <see cref="DebuggerParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterAddSub([NotNull] DebuggerParser.AddSubContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>AddSub</c>
	/// labeled alternative in <see cref="DebuggerParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitAddSub([NotNull] DebuggerParser.AddSubContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>Bin</c>
	/// labeled alternative in <see cref="DebuggerParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterBin([NotNull] DebuggerParser.BinContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>Bin</c>
	/// labeled alternative in <see cref="DebuggerParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitBin([NotNull] DebuggerParser.BinContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>Parens</c>
	/// labeled alternative in <see cref="DebuggerParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterParens([NotNull] DebuggerParser.ParensContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>Parens</c>
	/// labeled alternative in <see cref="DebuggerParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitParens([NotNull] DebuggerParser.ParensContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>Hex</c>
	/// labeled alternative in <see cref="DebuggerParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterHex([NotNull] DebuggerParser.HexContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>Hex</c>
	/// labeled alternative in <see cref="DebuggerParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitHex([NotNull] DebuggerParser.HexContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>Int</c>
	/// labeled alternative in <see cref="DebuggerParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterInt([NotNull] DebuggerParser.IntContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>Int</c>
	/// labeled alternative in <see cref="DebuggerParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitInt([NotNull] DebuggerParser.IntContext context);
}
} // namespace OldBit.Debugger.Parser
