namespace OldBit.Spectron.Emulation.Devices.Storage.SD;

internal class ResponseBuffer
{
    private readonly byte[] _buffer = new byte[20];
    private int _endPosition;
    private int _currentPosition;

    internal void R1(Status status)
    {
        _buffer[0] = (byte)(status);

        _endPosition = 1;
        _currentPosition = 0;
    }

    internal void R1(Status status, byte token, byte[] data, byte crc1, byte crc2)
    {
        _buffer[0] = (byte)(status);
        _buffer[1] = token;

        Array.Copy(data, 0, _buffer, 2, data.Length);

        _buffer[18] = crc1;
        _buffer[19] = crc2;

        _endPosition = 20;
        _currentPosition = 0;
    }

    internal void R7(Status status, byte voltage, byte checkPattern)
    {
        _buffer[0] = (byte)(status);
        _buffer[1] = 0;
        _buffer[2] = 0;
        _buffer[3] = (byte)(voltage & 0x0F);
        _buffer[4] = checkPattern;

        _endPosition = 5;
        _currentPosition = 0;
    }

    internal byte Read() => _currentPosition < _endPosition ? _buffer[_currentPosition++] : (byte)0xFF;

    internal void Reset()
    {
        _endPosition = 0;
        _currentPosition = 0;
    }
}