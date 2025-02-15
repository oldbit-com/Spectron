using OldBit.Spectron.Disassembly.Formatters;
using OldBit.Spectron.Disassembly.Helpers;
using OldBit.Spectron.Disassembly.Instructions;

namespace OldBit.Spectron.Disassembly;

/// <summary>
/// Represents a disassembler that can be used to disassemble Z80 machine code.
/// </summary>
public sealed class Disassembler
{
    private readonly ByteDataReader _byteDataReader;
    private readonly NumberFormatter _numberFormatter;
    private readonly int _maxCount;

    private IndexContext _indexContext = IndexContext.None;
    private int _address;

    /// <summary>
    /// Initializes a new instance of the <see cref="Disassembler"/> class.
    /// </summary>
    /// <param name="data">The byte data to disassemble.</param>
    /// <param name="startAddress">The start address for disassembly.</param>
    /// <param name="maxCount">The maximum number of instructions to disassemble.</param>
    /// <param name="options">The options to configure the disassembler.</param>
    public Disassembler(byte[] data,
        int startAddress,
        int maxCount,
        DisassemblerOptions? options = null)
    {
        _byteDataReader = new ByteDataReader(data, startAddress);
        _numberFormatter = new NumberFormatter(options?.NumberFormat ?? NumberFormat.HexDollarPrefix);

        _maxCount = maxCount;
        _address = _byteDataReader.Address;
    }

    /// <summary>
    /// Disassembles the memory and returns a list of instructions.
    /// </summary>
    /// <returns>A list of disassembled instructions.</returns>
    public List<Instruction> Disassemble()
    {
        var instructions = new List<Instruction>();

        while (instructions.Count < _maxCount)
        {
            var byteCode = _byteDataReader.ReadeByte();

            Instruction? instruction = null;

            switch (byteCode)
            {
                // Extended instructions
                case 0xED:
                    CheckInvalidIndexPrefix(instructions);

                    instruction = ProcessExtendedInstruction();
                    break;

                // Bit instructions
                case 0xCB:
                    instruction = ProcessBitInstruction();
                    break;

                // IX instructions
                case 0xDD:
                    CheckInvalidIndexPrefix(instructions);

                    _indexContext = IndexContext.IX;
                    continue;

                // IY instructions
                case 0xFD:
                    CheckInvalidIndexPrefix(instructions);

                    _indexContext = IndexContext.IY;
                    continue;

                default:
                    instruction = ProcessMainInstruction(byteCode);
                    break;
            }

            instruction ??= new Instruction(_address, "???", true)
            {
                ByteCodes = GetByteCodes
            };

            instructions.Add(instruction);

            _address = _byteDataReader.Address;
            _indexContext = IndexContext.None;
        }

        return instructions;
    }

    private void CheckInvalidIndexPrefix(List<Instruction> instructions)
    {
        if (_indexContext == IndexContext.None)
        {
            return;
        }

        var instruction = new Instruction(_address, "NOP?", true)
        {
            ByteCodes = _byteDataReader
                .GetRange(_address, _byteDataReader.Address - _address - 1)
                .ToArray()
        };

        instructions.Add(instruction);

        _address = _byteDataReader.Address - 1;
    }

    private Instruction ProcessMainInstruction(int byteCode)
    {
        Instruction? instruction = null;

        if (_indexContext is IndexContext.IX or IndexContext.IY)
        {
            if (IndexRegisterInstructions.Index.TryGetValue(byteCode, out var template))
            {
                instruction = CreateInstruction(template);
            }
        }

        if (instruction == null)
        {
            var template = MainInstructions.Index[byteCode];
            instruction = CreateInstruction(template);
        }

        instruction.ByteCodes = GetByteCodes;

        return instruction;
    }

    private Instruction ProcessBitInstruction()
    {
        Instruction instruction;

        if (_indexContext is IndexContext.IX or IndexContext.IY)
        {
            var byteCode = _byteDataReader.PeekByte(_byteDataReader.Address + 1);
            var template = IndexBitShiftRotateInstructions.Index[byteCode];

            instruction = CreateInstruction(template);
            _byteDataReader.ReadeByte(); // instruction code was peeked above, skip it
        }
        else
        {
            var byteCode = _byteDataReader.ReadeByte();
            var template = BitShiftRotateInstructions.Index[byteCode];

            instruction = CreateInstruction(template);
        }

        instruction.ByteCodes = GetByteCodes;

        return instruction;
    }

    private Instruction? ProcessExtendedInstruction()
    {
        var byteCode = _byteDataReader.ReadeByte();

        if (!ExtendedInstructions.Index.TryGetValue(byteCode, out var template))
        {
            return null;
        };

        var instruction = CreateInstruction(template);
        instruction.ByteCodes = GetByteCodes;

        return instruction;
    }

    private byte[] GetByteCodes => _byteDataReader
        .GetRange(_address, _byteDataReader.Address - _address)
        .ToArray();

    private Instruction CreateInstruction(InstructionTemplate template) => new(
        _address,
        template,
        _byteDataReader,
        _numberFormatter,
        _indexContext);
}