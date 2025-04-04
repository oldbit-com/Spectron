namespace OldBit.Spectron.Emulation.Devices.Storage.SD;

internal class DataBlock
{
    private int _count;

    internal bool IsReady => _count == 515; // 512 + start token + CRC

    internal readonly byte[] Data = new byte[512];

    internal void ProcessNextByte(byte value)
    {
        if (_count == 0 && value != 0xFE)
        {
            // Skip block start token
        }
        else
        {
            if (_count > 0 && _count <= Data.Length)
            {
                Data[_count - 1] = value;
            }
        }

        _count += 1;
    }
}