using System.Text;
using System.Text.RegularExpressions;
using OldBit.Spectron.Disassembly.Formatters;
using OldBit.Spectron.Disassembly.Helpers;

namespace OldBit.Spectron.Disassembly;

internal sealed record InstructionTemplate(string Pattern, bool IsUndocumented = false) { }

/// <summary>
/// Represents a disassembled instruction.
/// </summary>
public partial class Instruction
{
    /// <summary>
    /// Gets the address of the instruction.
    /// </summary>
    public Word Address { get; }

    /// <summary>
    /// Gets the disassembled code.
    /// </summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the byte codes of the instruction.
    /// </summary>
    public byte[] ByteCodes { get; internal set; } = [];

    /// <summary>
    /// Gets a value indicating whether the instruction is undocumented.
    /// </summary>
    public bool IsUndocumented { get; }

    /// <summary>
    /// Gets a value indicating whether the instruction is invalid.
    /// </summary>
    public bool IsInvalid { get; }

    internal Instruction(
        int address,
        InstructionTemplate template,
        ByteDataReader reader,
        NumberFormatter formatter,
        IndexContext context)
    {
        Address = (Word)address;
        IsUndocumented = template.IsUndocumented;

        Parse(template.Pattern, reader, formatter, context);
    }

    internal Instruction(int address, string code, bool isInvalid)
    {
        Address = (Word)address;
        Code = code;
        IsInvalid = isInvalid;
    }

    private void Parse(
        string instruction,
        ByteDataReader reader,
        NumberFormatter formatter,
        IndexContext context)
    {
        var code = new StringBuilder(instruction);

        // Displacement argument
        if (instruction.Contains("<d>"))
        {
            if (context == IndexContext.None)
            {
                var value = Address + (sbyte)reader.ReadeByte() + 2;
                code.Replace("<d>", formatter.Format((Word)value));
            }
            else
            {
                var value = (sbyte)reader.ReadeByte();
                code.Replace("<d>", formatter.FormatOffset(value));
            }
        }

        // Byte argument
        if (instruction.Contains("<n>"))
        {
            var value = reader.ReadeByte();
            code.Replace("<n>", formatter.Format(value));
        }

        // Word argument
        if (instruction.Contains("<nn>"))
        {
            var value = reader.ReadWord();
            code.Replace("<nn>", formatter.Format(value));
        }

        // RST instructions
        var match = HexPlaceholderRegex().Match(instruction);
        if (match.Success)
        {
            code.Replace(match.Groups[0].Value, formatter.Format(match.Groups[1].Value));
        }

        // IX or IY argument
        if (instruction.Contains("<xy>"))
        {
            switch (context)
            {
                case IndexContext.IX:
                    code.Replace("<xy>", "IX");
                    break;

                case IndexContext.IY:
                    code.Replace("<xy>", "IY");
                    break;
            }
        }

        Code = code.ToString();
    }

    [GeneratedRegex(@"\<(\d\d)\>")]
    private static partial Regex HexPlaceholderRegex();

    /// <summary>
    /// Returns a string that represents the disassembled instruction.
    /// </summary>
    /// <returns>A string that represents the disassembled instruction.</returns>
    public override string ToString() => Code;
}