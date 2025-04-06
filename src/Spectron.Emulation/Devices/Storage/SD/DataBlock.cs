namespace OldBit.Spectron.Emulation.Devices.Storage.SD;

/// <summary>
/// Represents a data block received from the SD device. It starts with a 0xFE token
/// and contains 512 bytes of data. The block is terminated with a CRC16 checksum.
/// </summary>
internal class DataBlock
{
    private int _receivedBytesCount;

    internal bool IsReady => _receivedBytesCount == 515; // 512 + start token + CRC

    internal readonly byte[] Data = new byte[512];

    internal void NextByte(byte value)
    {
        switch (_receivedBytesCount)
        {
            case 0 when value != Token.StartBlock:
                // Skip block start token
                break;

            case > 0 when _receivedBytesCount <= Data.Length:
                Data[_receivedBytesCount - 1] = value;
                break;
        }

        _receivedBytesCount += 1;
    }
}