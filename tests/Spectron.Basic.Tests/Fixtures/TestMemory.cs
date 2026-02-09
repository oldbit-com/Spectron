using OldBit.Z80Cpu;

namespace Spectron.Basic.Tests.Fixtures;

public class TestMemory : IMemory
{
    private readonly byte[] _memory = new byte[65536];

    public byte Read(Word address) => _memory[address];

    public void Write(Word address, byte data) => _memory[address] = data;

    public void Write(Word address, byte[] data) => Array.Copy(data, 0, _memory, address, data.Length);
}