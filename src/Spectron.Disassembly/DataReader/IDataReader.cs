namespace OldBit.Spectron.Disassembly.DataReader;

internal interface IDataReader
{
    int Address { get; set; }

    byte ReadeByte();

    byte PeekByte(int address);

    Word ReadWord()
    {
        var lowByte = ReadeByte();
        var highByte = ReadeByte();

        return (Word)(lowByte | (highByte << 8));
    }

    IEnumerable<byte> GetRange(int start, int count);
}