using OldBit.Spectron.Disassembly.Formatters;
using OldBit.Spectron.Disassembly.Helpers;
using OldBit.Spectron.Disassembly.Instructions;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Disassembly;

public sealed class Disassembler
{
    private readonly IDataReader _dataReader;
    private readonly NumberFormatter _numberFormatter;
    private readonly int _maxCount;

    private IndexContext _indexContext = IndexContext.None;
    private int _address;

    private Disassembler(IDataReader dataReader, int maxCount, DisassemblerOptions? options = null)
    {
        _dataReader = dataReader;
        _numberFormatter = new NumberFormatter(options?.NumberFormat ?? NumberFormat.HexPrefixDollar);

        _maxCount = maxCount;
        _address = _dataReader.Address;
    }

    public Disassembler(byte[] data, int startAddress, int maxCount, DisassemblerOptions? options = null) :
        this(new ByteDataReader(data, startAddress), maxCount, options)
    {
    }

    public Disassembler(IMemory memory, int startAddress, int maxCount, DisassemblerOptions? options = null) :
        this(new MemoryDataReader(memory, startAddress), maxCount, options)
    {
    }

    public List<Instruction> Disassemble(Word? startAddress = null)
    {
        if (startAddress.HasValue)
        {
            _dataReader.Address = startAddress.Value;
        }

        var instructions = new List<Instruction>();

        while (instructions.Count < _maxCount)
        {
            var byteCode = _dataReader.ReadeByte();

            Instruction? instruction;

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

            _address = _dataReader.Address;
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
            ByteCodes = _dataReader
                .GetRange(_address, _dataReader.Address - _address - 1)
                .ToArray()
        };

        instructions.Add(instruction);

        _address = _dataReader.Address - 1;
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
            var byteCode = _dataReader.PeekByte(_dataReader.Address + 1);
            var template = IndexBitShiftRotateInstructions.Index[byteCode];

            instruction = CreateInstruction(template);
            _dataReader.ReadeByte(); // instruction code was peeked above, skip it
        }
        else
        {
            var byteCode = _dataReader.ReadeByte();
            var template = BitShiftRotateInstructions.Index[byteCode];

            instruction = CreateInstruction(template);
        }

        instruction.ByteCodes = GetByteCodes;

        return instruction;
    }

    private Instruction? ProcessExtendedInstruction()
    {
        var byteCode = _dataReader.ReadeByte();

        if (!ExtendedInstructions.Index.TryGetValue(byteCode, out var template))
        {
            return null;
        };

        var instruction = CreateInstruction(template);
        instruction.ByteCodes = GetByteCodes;

        return instruction;
    }

    private byte[] GetByteCodes => _dataReader
        .GetRange(_address, _dataReader.Address - _address)
        .ToArray();

    private Instruction CreateInstruction(InstructionTemplate template) => new(
        _address,
        template,
        _dataReader,
        _numberFormatter,
        _indexContext);
}