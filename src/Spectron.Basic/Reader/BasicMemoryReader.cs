using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Spectron.Emulation.Extensions;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Basic.Reader;

public sealed class BasicMemoryReader(IMemory memory)
{
    private const int SysVarProg = 0x5C53;

    public List<BasicLine> ReadAllLines()
    {
        var lines = new List<BasicLine>();
        var start = memory.ReadWord(SysVarProg);

        while (true)
        {
            var lineNumber = memory.Read(start) << 8 | memory.Read((Word)(start + 1));

            if (lineNumber >= 0x8000)
            {
                break;
            }

            var length = memory.ReadWord((Word)(start + 2));
            var data = memory.ReadBytes((Word)(start + 4), length);

            lines.Add(new BasicLine(lineNumber, data.ToArray()));
            start += (Word)(4 + length);
        }

        return lines;
    }
}